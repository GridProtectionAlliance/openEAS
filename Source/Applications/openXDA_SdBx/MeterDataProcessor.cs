//******************************************************************************************************
//  MeterDataProcessor.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/26/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Transactions;
using FaultData.DataAnalysis;
using FaultData.Database;
using FaultData.Database.MeterDataTableAdapters;
using FaultData.DataOperations;
using FaultData.DataSets;
using FaultData.DataWriters;
using GSF.Annotations;
using GSF.Configuration;
using log4net;
using openXDA_SdBx.Configuration;
using SandBox;

namespace openXDA_SdBx
{
    public class MeterDataProcessor
    {
        #region [ Members ]

        // Fields
        private string m_connectionString;

        #endregion

        #region [ Constructors ]

        public MeterDataProcessor(string connectionString)
        {
            m_connectionString = connectionString;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Processes the file at the given path.
        /// </summary>
        /// <param name="fileGroupID">The identifier for the file group to be processed.</param>
        /// <returns>False if the file was not able to be processed and needs to be processed again later.</returns>
        public bool ProcessFileGroup(int fileGroupID)
        {
            SystemSettings systemSettings;
            TimeZoneInfo defaultMeterTimeZone;
            TimeZoneInfo xdaTimeZone;

            FileInfoDataContext fileInfo;
            FileGroup fileGroup;
            DataFile dataFile = null;

            List<MeterDataSet> meterDataSets;

            try
            {
                systemSettings = new SystemSettings(m_connectionString);

                using (DbAdapterContainer dbAdapterContainer = new DbAdapterContainer(systemSettings.DbConnectionString, systemSettings.DbTimeout))
                {
                    fileInfo = dbAdapterContainer.GetAdapter<FileInfoDataContext>();
                    defaultMeterTimeZone = systemSettings.DefaultMeterTimeZoneInfo;
                    xdaTimeZone = systemSettings.XDATimeZoneInfo;

                    // Create a file group for this file in the database
                    fileGroup = LoadFileGroup(fileInfo, fileGroupID);

                    if ((object)fileGroup == null)
                        return true;

                    dataFile = fileGroup.DataFiles.FirstOrDefault();

                    if ((object)dataFile == null)
                        return true;

                    // Parse the file
                    meterDataSets = LoadMeterDataSets(dbAdapterContainer, fileGroup);

                    // Set properties on each of the meter data sets
                    foreach (MeterDataSet meterDataSet in meterDataSets)
                    {
                        meterDataSet.ConnectionString = systemSettings.DbConnectionString;
                        meterDataSet.FilePath = dataFile.FilePath;
                        meterDataSet.FileGroup = fileGroup;
                        ShiftTime(meterDataSet, defaultMeterTimeZone, xdaTimeZone);
                    }

                    // Process meter data sets
                    OnStatusMessage("Processing meter data from file \"{0}\"...", dataFile.FilePath);
                    ProcessMeterDataSets(meterDataSets, systemSettings, dbAdapterContainer);
                    OnStatusMessage("Finished processing data from file \"{0}\".", dataFile.FilePath);
                }
            }
            catch (Exception ex)
            {
                string message;

                if ((object)dataFile != null)
                    message = string.Format("Failed to process file \"{0}\" due to exception: {1}", dataFile.FilePath, ex.Message);
                else
                    message = string.Format("Failed to process file group \"{0}\" due to exception: {1}", fileGroupID, ex.Message);

                OnHandleException(new InvalidOperationException(message, ex));
            }

            return true;
        }

        private List<MeterDataSet> LoadMeterDataSets(DbAdapterContainer dbAdapterContainer, FileGroup fileGroup)
        {
            List<MeterDataSet> meterDataSets = new List<MeterDataSet>();

            MeterInfoDataContext meterInfo = dbAdapterContainer.GetAdapter<MeterInfoDataContext>();
            EventTableAdapter eventAdapter = dbAdapterContainer.GetAdapter<EventTableAdapter>();
            EventDataTableAdapter eventDataAdapter = dbAdapterContainer.GetAdapter<EventDataTableAdapter>();

            MeterData.EventDataTable eventTable = eventAdapter.GetDataByFileGroup(fileGroup.ID);

            MeterDataSet meterDataSet;
            DataGroup dataGroup;

            foreach (IGrouping<int, MeterData.EventRow> eventGroup in eventTable.GroupBy(evt => evt.MeterID))
            {
                meterDataSet = new MeterDataSet();
                meterDataSet.Meter = meterInfo.Meters.SingleOrDefault(meter => meter.ID == eventGroup.Key);

                foreach (MeterData.EventRow evt in eventGroup)
                {
                    dataGroup = new DataGroup();
                    dataGroup.FromData(meterDataSet.Meter, eventDataAdapter.GetTimeDomainData(evt.EventDataID));

                    foreach (DataSeries dataSeries in dataGroup.DataSeries)
                        meterDataSet.DataSeries.Add(dataSeries);
                }

                meterDataSets.Add(meterDataSet);
            }

            return meterDataSets;
        }

        public void ProcessMeterDataSets(List<MeterDataSet> meterDataSets, SystemSettings systemSettings, DbAdapterContainer dbAdapterContainer)
        {
            TimeZoneInfo xdaTimeZone;
            DateTime processingEndTime;

            try
            {
                foreach (MeterDataSet meterDataSet in meterDataSets)
                    ProcessMeterData(meterDataSet, dbAdapterContainer);

                xdaTimeZone = systemSettings.XDATimeZoneInfo;
                processingEndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, xdaTimeZone);

                foreach (MeterDataSet meterDataSet in meterDataSets)
                    meterDataSet.FileGroup.ProcessingEndTime = processingEndTime;

                dbAdapterContainer.GetAdapter<FileInfoDataContext>().SubmitChanges();
            }
            catch (Exception ex)
            {
                OnHandleException(ex);
            }
        }

        private void ProcessMeterData(MeterDataSet meterDataSet, DbAdapterContainer dbAdapterContainer)
        {
            try
            {
                meterDataSet.ConnectionString = m_connectionString;
                ExecuteDataOperation(meterDataSet, dbAdapterContainer);
                ExecuteDataWriters(meterDataSet, dbAdapterContainer);
            }
            catch (Exception ex)
            {
                try
                {
                    OnHandleException(ex);
                    meterDataSet.FileGroup.Error = 1;
                    dbAdapterContainer.GetAdapter<FileInfoDataContext>().SubmitChanges();
                }
                catch
                {
                    // Ignore errors here as they are most likely
                    // related to the error we originally caught
                }
            }
        }

        private void ExecuteDataOperation(MeterDataSet meterDataSet, DbAdapterContainer dbAdapterContainer)
        {
            IDataOperation dataOperation = null;

            try
            {
                // Create the data operation
                dataOperation = new SandBoxOperation();

                // Provide system settings to the data operation
                ConnectionStringParser.ParseConnectionString(meterDataSet.ConnectionString, dataOperation);

                // Prepare for execution of the data operation
                dataOperation.Prepare(dbAdapterContainer);

                // Execute the data operation
                dataOperation.Execute(meterDataSet);

                // Load data from all data operations in a single transaction
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, GetTransactionOptions()))
                {
                    dataOperation.Load(dbAdapterContainer);
                    transactionScope.Complete();
                }
            }
            finally
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if ((object)dataOperation != null)
                    TryDispose(dataOperation as IDisposable);
            }
        }

        private void ExecuteDataWriters(MeterDataSet meterDataSet, DbAdapterContainer dbAdapterContainer)
        {
            IDataWriter dataWriter = null;

            try
            {
                // Create the data writer
                dataWriter = new SandBoxWriter();

                // Provide system settings to the data operation
                ConnectionStringParser.ParseConnectionString(meterDataSet.ConnectionString, dataWriter);

                // Prepare for execution of the data operation
                dataWriter.WriteResults(dbAdapterContainer, meterDataSet);
            }
            finally
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if ((object)dataWriter != null)
                    TryDispose(dataWriter as IDisposable);
            }
        }

        // Loads a file group containing information about the file on the given
        // file path, as well as the files related to it, into the database.
        private FileGroup LoadFileGroup(FileInfoDataContext dataContext, int fileGroupID)
        {
            return dataContext.FileGroups.FirstOrDefault(fileGroup => fileGroup.ID == fileGroupID);
        }

        // Adjusts the timestamps in the given data sets to the time zone of XDA.
        private void ShiftTime(MeterDataSet meterDataSet, TimeZoneInfo defaultMeterTimeZone, TimeZoneInfo xdaTimeZone)
        {
            TimeZoneInfo meterTimeZone;

            if (!string.IsNullOrEmpty(meterDataSet.Meter.TimeZone))
                meterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(meterDataSet.Meter.TimeZone);
            else
                meterTimeZone = defaultMeterTimeZone;

            foreach (DataSeries dataSeries in meterDataSet.DataSeries)
            {
                foreach (DataPoint dataPoint in dataSeries.DataPoints)
                {
                    dataPoint.Time = TimeZoneInfo.ConvertTimeToUtc(dataPoint.Time, meterTimeZone);
                    dataPoint.Time = TimeZoneInfo.ConvertTimeFromUtc(dataPoint.Time, xdaTimeZone);
                }
            }
        }

        private void TryDispose(IDisposable obj)
        {
            try
            {
                if ((object)obj != null)
                    obj.Dispose();
            }
            catch (Exception ex)
            {
                OnHandleException(ex);
            }
        }

        [StringFormatMethod("format")]
        private void OnStatusMessage(string format, params object[] args)
        {
            Log.Info(string.Format(format, args));
        }

        private void OnHandleException(Exception ex)
        {
            Log.Error(ex.Message, ex);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConnectionStringParser<SettingAttribute, CategoryAttribute> ConnectionStringParser = new ConnectionStringParser<SettingAttribute, CategoryAttribute>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(MeterDataProcessor));

        // Static Methods
        private static TransactionOptions GetTransactionOptions()
        {
            return new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TransactionManager.MaximumTimeout
            };
        }

        #endregion
    }
}
