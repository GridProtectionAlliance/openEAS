//******************************************************************************************************
//  DataHub.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  06/07/2017 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Data.Model;
using GSF.Identity;
using GSF.Web.Hubs;
using GSF.Web.Model;
using GSF.Web.Security;
using openEASSandBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace openEAS
{
    public class DataHub: RecordOperationsHub<DataHub>
    {
        #region [ Methods ]
        #endregion

        #region [ Misc ]

        public IEnumerable<IDLabel> SearchTimeZones(string searchText, int limit)
        {
            IReadOnlyCollection<TimeZoneInfo> tzi = TimeZoneInfo.GetSystemTimeZones();

            return tzi
                .Select(row => new IDLabel(row.Id, row.ToString()))
                .Where(row => row.label.ToLower().Contains(searchText.ToLower()));
        }


        /// <summary>
        /// Gets UserAccount table ID for current user.
        /// </summary>
        /// <returns>UserAccount.ID for current user.</returns>
        public static Guid GetCurrentUserID()
        {
            Guid userID;
            AuthorizationCache.UserIDs.TryGetValue(Thread.CurrentPrincipal.Identity.Name, out userID);
            return userID;
        }

        /// <summary>
        /// Gets UserAccount table SID for current user.
        /// </summary>
        /// <returns>UserAccount.ID for current user.</returns>
        public static string GetCurrentUserSID()
        {
            return UserInfo.UserNameToSID(Thread.CurrentPrincipal.Identity.Name);
        }

        /// <summary>
        /// Gets UserAccount table name for current user.
        /// </summary>
        /// <returns>User name for current user.</returns>
        public static string GetCurrentUserName()
        {
            return Thread.CurrentPrincipal.Identity.Name;
        }

        #endregion

        #region [ Result Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.QueryRecordCount)]
        public int QueryOpenEASResultCount(string filterString)
        {
            return DataContext.Table<OpenEASResult>().QueryRecordCount(filterString);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.QueryRecords)]
        public IEnumerable<OpenEASResult> QueryOpenEASResults(string sortField, bool ascending, int page, int pageSize, string filterString)
        {
            return DataContext.Table<OpenEASResult>().QueryRecords(sortField, ascending, page, pageSize, filterString);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.DeleteRecord)]
        public void DeleteOpenEASResult(int id)
        {
            DataContext.Table<OpenEASResult>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.CreateNewRecord)]
        public OpenEASResult NewOpenEASResult()
        {
            return DataContext.Table<OpenEASResult>().NewRecord();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.AddNewRecord)]
        public void AddNewOpenEASResult(OpenEASResult record)
        {
            DataContext.Table<OpenEASResult>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(OpenEASResult), RecordOperation.UpdateRecord)]
        public void UpdateOpenEASResult(OpenEASResult record)
        {
            DataContext.Table<OpenEASResult>().UpdateRecord(record);
        }

        #endregion

        #region [ Setting Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.QueryRecordCount)]
        public int QuerySettingCount(string filterString)
        {
            return DataContext.Table<CSALineSetting>().QueryRecordCount(filterString);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.QueryRecords)]
        public IEnumerable<CSALineSetting> QuerySettings(string sortField, bool ascending, int page, int pageSize, string filterString)
        {
            return DataContext.Table<CSALineSetting>().QueryRecords(sortField, ascending, page, pageSize, filterString);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.DeleteRecord)]
        public void DeleteSetting(int id)
        {
            DataContext.Table<CSALineSetting>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.CreateNewRecord)]
        public CSALineSetting NewSetting()
        {
            return DataContext.Table<CSALineSetting>().NewRecord();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.AddNewRecord)]
        public void AddNewSetting(CSALineSetting record)
        {
            DataContext.Table<CSALineSetting>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(CSALineSetting), RecordOperation.UpdateRecord)]
        public void UpdateSetting(CSALineSetting record)
        {
            DataContext.Table<CSALineSetting>().UpdateRecord(record);
        }

        #endregion


    }
}
