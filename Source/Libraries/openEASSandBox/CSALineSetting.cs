//******************************************************************************************************
//  CSALineSetting.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/27/2018 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************
using GSF.Data.Model;
using System.ComponentModel;

namespace openEASSandBox
{
    [TableName("CSA_2_LineSetting")]
    public class CSALineSetting
    {
        [PrimaryKey(true)]
        public int ID { get; set; }
        public int LineID { get; set; }

        [DefaultValue(28)]
        public int NumArgsOut { get; set; }

        [DefaultValue(5.0D)]
        public double ITHDLimit { get; set; }

        [DefaultValue(500.0D)]
        public double NominalVoltage { get; set; }

        [DefaultValue(5.0D)]
        public double UnloadedCurrent { get; set; }
        
        [DefaultValue(161.0D)]
        public double NominalBuskVLL { get; set; }

        [DefaultValue(4.0D)]
        public double T2ndClosing { get; set; }

        [DefaultValue(0.0D)]
        public double CapSwitcherType { get; set; }

        [DefaultValue(18000.0D)]
        public double StepSizeQ3 { get; set; }

    }
}
