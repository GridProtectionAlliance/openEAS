//******************************************************************************************************
//  SandBoxEngine.cs
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
//  09/04/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.Collections;
using GSF.Configuration;
using GSF.IO;
using GSF.Threading;
using log4net;
using openEAS.Configuration;
using System.Text;
using GSF.Data;
using openXDA.Model;
using GSF.Data.Model;
using System.Reflection;

namespace openEAS
{
    public class SandBoxEngine
    {
        #region [ Members ]

        // Fields
        private string m_dbConnectionString;
        private SystemSettings m_systemSettings;
        private LongSynchronizedOperation m_processLatestDataOperation;
        private int m_latestFileGroupID = 0;
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the current status of the XDA engine.
        /// </summary>
        public string Status
        {
            get
            {
                SystemSettings systemSettings = m_systemSettings;
                StringBuilder statusBuilder = new StringBuilder();
                KeyValuePair<string, string>[] activeFiles;

                statusBuilder.AppendLine("Meter Data Status:");
                statusBuilder.AppendLine(new string('=', 50));
                statusBuilder.AppendLine($"               XDA Time Zone: {systemSettings.XDATimeZone}");
                statusBuilder.AppendLine($"            System frequency: {systemSettings.SystemFrequency} Hz");
                statusBuilder.AppendLine($"            Database Timeout: {systemSettings.DbTimeout} seconds");
                statusBuilder.AppendLine($"        Max thread pool size: {systemSettings.ProcessingThreadCount}");
                statusBuilder.AppendLine($"             Max Time Offset: {systemSettings.MaxTimeOffset} hours");
                statusBuilder.AppendLine($"             Min Time Offset: {systemSettings.MinTimeOffset} hours");
                statusBuilder.AppendLine($"           Max File Duration: {systemSettings.MaxFileDuration} seconds");
                statusBuilder.AppendLine($"        File Creation Offset: {systemSettings.MaxFileCreationTimeOffset} hours");
                statusBuilder.AppendLine($"Last FileGroup(ID) Processed: {m_latestFileGroupID}");

                statusBuilder.AppendLine();


                return statusBuilder.ToString().TrimEnd();
            }
        }

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
            List<int> newFileGroups;

            if ((object)m_systemSettings == null)
                ReloadSystemSettings();

            using (FileBackedDictionary<string, int> dictionary = new FileBackedDictionary<string, int>(latestDataFile))
            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                do
                {
                    dictionary.Compact();

                    if (!dictionary.TryGetValue("latestFileGroupID", out m_latestFileGroupID))
                        m_latestFileGroupID = 0;

                    newFileGroups = (new TableOperations<FileGroup>(connection)).QueryRecordsWhere("ID > {0}", m_latestFileGroupID)
                        .Select(fileGroup => fileGroup.ID)
                        .Take(100)
                        .OrderBy(id => id)
                        .ToList();

                    foreach (int fileGroupID in newFileGroups)
                    {
                        MeterDataProcessor processor = new MeterDataProcessor(s_connectionString);
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
            s_connectionString = LoadSystemSettings();
            m_systemSettings = new SystemSettings(s_connectionString);
        }

        private static string LoadSystemSettings()
        {
            using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
            {
                TableOperations<Setting> settingTable = new TableOperations<Setting>(connection);
                List<Setting> settingList = settingTable.QueryRecords().ToList();

                foreach (IGrouping<string, Setting> grouping in settingList.GroupBy(setting => setting.Name))
                {
                    if (grouping.Count() > 1)
                        Log.Warn($"Duplicate record for setting {grouping.Key} detected.");
                }

                // Convert the Setting table to a dictionary
                Dictionary<string, string> settings = settingList
                    .DistinctBy(setting => setting.Name)
                    .ToDictionary(setting => setting.Name, setting => setting.Value, StringComparer.OrdinalIgnoreCase);

                // Convert dictionary to a connection string and return it
                return SystemSettings.ToConnectionString(settings);
            }
        }

        public Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string path;

            if (m_systemSettings.openEASSettings.DependentAssemblyLookup.TryGetValue(assemblyName.Name, out path))
                return Assembly.LoadFile(path);

            return null;
        }


        #endregion

        #region [ Static ]
        private static string s_connectionString;

        static SandBoxEngine() {
            s_connectionString = LoadSystemSettings();
        }

        // Static Fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(SandBoxEngine));

        #endregion
    }
}
