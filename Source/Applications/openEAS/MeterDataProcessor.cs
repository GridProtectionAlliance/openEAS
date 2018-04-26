//******************************************************************************************************
//  MeterDataProcessor.cs
//
//  Copyright(c) 2016, Electric Power Research Institute, Inc.
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are met:
//  
//  * Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//  
//  * Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//  
//  * Neither the name of copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//  
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
//  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//  DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
//  FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//  DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
//  OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
//  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
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
using FaultData.DataOperations;
using FaultData.DataSets;
using GSF.Annotations;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using log4net;
using openEAS.Configuration;
using openEASSandBox;
using openXDA.Model;

namespace openEAS
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

            FileGroup fileGroup;
            DataFile dataFile = null;

            List<MeterDataSet> meterDataSets;

            try
            {
                systemSettings = new SystemSettings(m_connectionString);

                using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
                {
                    // Create a file group for this file in the database
                    fileGroup = LoadFileGroup(connection, fileGroupID);

                    if ((object)fileGroup == null)
                        return true;

                    dataFile = fileGroup.DataFiles.FirstOrDefault();

                    if ((object)dataFile == null)
                        return true;

                    // Parse the file
                    meterDataSets = LoadMeterDataSets(connection, fileGroup);

                    // Set properties on each of the meter data sets
                    foreach (MeterDataSet meterDataSet in meterDataSets)
                    {
                        meterDataSet.ConnectionString = m_connectionString;
                        meterDataSet.FilePath = dataFile.FilePath;
                        meterDataSet.FileGroup = fileGroup;
                    }

                    // Process meter data sets
                    OnStatusMessage("Processing meter data from file \"{0}\"...", dataFile.FilePath);
                    ProcessMeterDataSets(meterDataSets, systemSettings, connection);
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

        private List<MeterDataSet> LoadMeterDataSets(AdoDataConnection connection, FileGroup fileGroup)
        {
            List<MeterDataSet> meterDataSets = new List<MeterDataSet>();
            IEnumerable<Event> eventTable = (new TableOperations<Event>(connection)).QueryRecordsWhere("FileGroupID", fileGroup.ID);

            MeterDataSet meterDataSet;
            DataGroup dataGroup;

            foreach (IGrouping<int, Event> eventGroup in eventTable.GroupBy(evt => evt.MeterID))
            {
                meterDataSet = new MeterDataSet();
                meterDataSet.Meter = (new TableOperations<Meter>(connection)).QueryRecordWhere("ID = {0}", eventGroup.Key);

                foreach (Event evt in eventGroup)
                {
                    dataGroup = new DataGroup();
                    dataGroup.FromData(meterDataSet.Meter, (new TableOperations<EventData>(connection)).QueryRecordWhere("ID = {0}", evt.EventDataID).TimeDomainData);

                    foreach (DataSeries dataSeries in dataGroup.DataSeries)
                        meterDataSet.DataSeries.Add(dataSeries);
                }

                meterDataSets.Add(meterDataSet);
            }

            return meterDataSets;
        }

        public void ProcessMeterDataSets(List<MeterDataSet> meterDataSets, SystemSettings systemSettings, AdoDataConnection connection)
        {
            try
            {
                foreach (MeterDataSet meterDataSet in meterDataSets)
                    ProcessMeterData(meterDataSet);
            }
            catch (Exception ex)
            {
                OnHandleException(ex);
            }
        }

        private void ProcessMeterData(MeterDataSet meterDataSet)
        {
            try
            {
                meterDataSet.ConnectionString = m_connectionString;
                ExecuteDataOperation(meterDataSet);
            }
            catch (Exception ex)
            {
                try
                {
                    OnHandleException(ex);
                }
                catch
                {
                    // Ignore errors here as they are most likely
                    // related to the error we originally caught
                }
            }
        }

        private void ExecuteDataOperation(MeterDataSet meterDataSet)
        {
            IDataOperation dataOperation = null;

            try
            {
                // Create the data operation
                dataOperation = new openEASSandBoxOperation();

                // Provide system settings to the data operation
                ConnectionStringParser.ParseConnectionString(meterDataSet.ConnectionString, dataOperation);

                // Execute the data operation
                dataOperation.Execute(meterDataSet);
            }
            finally
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if ((object)dataOperation != null)
                    TryDispose(dataOperation as IDisposable);
            }
        }

        // Loads a file group containing information about the file on the given
        // file path, as well as the files related to it, into the database.
        private FileGroup LoadFileGroup(AdoDataConnection connection, int fileGroupID)
        {
            return (new TableOperations<FileGroup>(connection)).QueryRecordWhere("ID = {0}", fileGroupID);
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
