//******************************************************************************************************
//  openEASSandBoxOperation.cs - Gbtc
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
//  09/07/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using FaultData.Database;
using FaultData.DataOperations;
using FaultData.DataSets;
using log4net;

namespace openEASSandBox
{
    public class openEASSandBoxOperation : DataOperationBase<MeterDataSet>
    {
        public override void Prepare(DbAdapterContainer dbAdapterContainer)
        {
            // Prepare for data analysis
        }

        public override void Execute(MeterDataSet meterDataSet)
        {
            Log.InfoFormat("Processing {0} waveforms from {1}...", meterDataSet.DataSeries.Count, meterDataSet.Meter.Name);

            // Execute data analysis
        }

        public override void Load(DbAdapterContainer dbAdapterContainer)
        {
            int resultsCount = 0;

            // Write analysis results to the database

            Log.InfoFormat("{0} results written to the database.", resultsCount);
        }

        // Used for logging messages
        private static readonly ILog Log = LogManager.GetLogger(typeof(openEASSandBoxOperation));
    }
}
