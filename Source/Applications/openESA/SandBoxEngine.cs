//******************************************************************************************************
//  SandBoxEngine.cs - Gbtc
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
//  09/04/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using FaultData.Database;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using GSF.Threading;
using log4net;
using openESA.Configuration;

namespace openESA
{
    public class SandBoxEngine
    {
        #region [ Members ]

        // Fields
        private string m_dbConnectionString;
        private SystemSettings m_systemSettings;
        private LongSynchronizedOperation m_processLatestDataOperation;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SandBoxEngine"/> class.
        /// </summary>
        public SandBoxEngine()
        {
            m_processLatestDataOperation = new LongSynchronizedOperation(ProcessLatestDataOperation, ex => Log.Error(ex.Message, ex));
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Processes data not yet processed
        /// by this SandBox instance.
        /// </summary>
        public void ProcessLatestData()
        {
            m_processLatestDataOperation.TryRun();
        }

        /// <summary>
        /// Processes data not yet processed
        /// by this SandBox instance.
        /// </summary>
        private void ProcessLatestDataOperation()
        {
            string latestDataFile = FilePath.GetAbsolutePath(@"LatestData.bin");
            int latestFileGroupID;
            FileInfoDataContext fileInfo;
            List<int> newFileGroups;

            if ((object)m_systemSettings == null)
                ReloadSystemSettings();

            using (FileBackedDictionary<string, int> dictionary = new FileBackedDictionary<string, int>(latestDataFile))
            using (DbAdapterContainer dbAdapterContainer = new DbAdapterContainer(m_systemSettings.DbConnectionString, m_systemSettings.DbTimeout))
            {
                fileInfo = dbAdapterContainer.GetAdapter<FileInfoDataContext>();

                do
                {
                    dictionary.Compact();

                    if (!dictionary.TryGetValue("latestFileGroupID", out latestFileGroupID))
                        latestFileGroupID = 0;

                    newFileGroups = fileInfo.FileGroups
                        .Select(fileGroup => fileGroup.ID)
                        .Where(id => id > latestFileGroupID)
                        .Take(100)
                        .OrderBy(id => id)
                        .ToList();

                    foreach (int fileGroupID in newFileGroups)
                    {
                        MeterDataProcessor processor = new MeterDataProcessor(LoadSystemSettings());
                        processor.ProcessFileGroup(fileGroupID);
                        dictionary["latestFileGroupID"] = fileGroupID;
                    }
                }
                while (newFileGroups.Count > 0);
            }
        }

        /// <summary>
        /// Reloads system settings from the database.
        /// </summary>
        public void ReloadSystemSettings()
        {
            ConfigurationFile configurationFile;
            CategorizedSettingsElementCollection category;

            // Reload the configuration file
            configurationFile = ConfigurationFile.Current;
            configurationFile.Reload();

            // Retrieve the connection string from the config file
            category = configurationFile.Settings["systemSettings"];
            category.Add("ConnectionString", "Data Source=localhost; Initial Catalog=openXDA; Integrated Security=SSPI", "Defines the connection to the openXDA database.");
            m_dbConnectionString = category["ConnectionString"].Value;

            // Load system settings from the database
            m_systemSettings = new SystemSettings(LoadSystemSettings());
        }

        private string LoadSystemSettings()
        {
            using (SystemInfoDataContext systemInfo = new SystemInfoDataContext(m_dbConnectionString))
            {
                return LoadSystemSettings(systemInfo);
            }
        }

        private string LoadSystemSettings(SystemInfoDataContext systemInfo)
        {
            // Convert the Setting table to a dictionary
            Dictionary<string, string> settings = systemInfo.Settings
                .ToDictionary(setting => setting.Name, setting => setting.Value, StringComparer.OrdinalIgnoreCase);

            // Add the database connection string if there is not
            // already one explicitly specified in the Setting table
            if (!settings.ContainsKey("dbConnectionString"))
                settings.Add("dbConnectionString", m_dbConnectionString);

            // Convert dictionary to a connection string and return it
            return SystemSettings.ToConnectionString(settings);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(SandBoxEngine));

        #endregion
    }
}
