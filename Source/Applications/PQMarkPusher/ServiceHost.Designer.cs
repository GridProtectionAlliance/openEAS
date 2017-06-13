//*********************************************************************************************************************
//  ServiceHost.Designer.cs
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
//  -------------------------------------------------------------------------------------------------------------------
//  09/10/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//*********************************************************************************************************************

namespace PQMarkPusher
{
    partial class ServiceHost
    {
        #region [ Service Binding ]

        internal void StartDebugging(string[] args)
        {
            OnStart(args);
        }

        internal void StopDebugging()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            m_serviceHelper.OnStart(args);
        }

        protected override void OnStop()
        {
            m_serviceHelper.OnStop();
        }

        protected override void OnPause()
        {
            m_serviceHelper.OnPause();
        }

        protected override void OnContinue()
        {
            m_serviceHelper.OnResume();
        }

        protected override void OnShutdown()
        {
            m_serviceHelper.OnShutdown();
        }

        #endregion

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_serviceHelper = new GSF.ServiceProcess.ServiceHelper(this.components);
            this.m_remotingServer = new GSF.Communication.TcpServer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ErrorLogger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ErrorLogger.ErrorLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ProcessScheduler)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.StatusLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingServer)).BeginInit();
            // 
            // m_serviceHelper
            // 
            // 
            // 
            // 
            // 
            // 
            // 
            this.m_serviceHelper.ErrorLogger.ErrorLog.FileName = "PQMarkPusher.ErrorLog.txt";
            this.m_serviceHelper.ErrorLogger.ErrorLog.PersistSettings = true;
            this.m_serviceHelper.ErrorLogger.ErrorLog.SettingsCategory = "ErrorLog";
            this.m_serviceHelper.ErrorLogger.PersistSettings = true;
            this.m_serviceHelper.ParentService = this;
            this.m_serviceHelper.PersistSettings = true;
            // 
            // 
            // 
            this.m_serviceHelper.ProcessScheduler.PersistSettings = true;
            this.m_serviceHelper.ProcessScheduler.SettingsCategory = "ProcessScheduler";
            this.m_serviceHelper.RemotingServer = this.m_remotingServer;
            // 
            // 
            // 
            this.m_serviceHelper.StatusLog.FileName = "PQMarkPusher.StatusLog.txt";
            this.m_serviceHelper.StatusLog.PersistSettings = true;
            this.m_serviceHelper.StatusLog.SettingsCategory = "StatusLog";
            // 
            // m_remotingServer
            // 
            this.m_remotingServer.ConfigurationString = "Port=8888";
            this.m_remotingServer.IntegratedSecurity = true;
            this.m_remotingServer.PayloadAware = true;
            this.m_remotingServer.PersistSettings = true;
            this.m_remotingServer.SettingsCategory = "RemotingServer";
            // 
            // ServiceHost
            // 
            this.ServiceName = "PQMarkPusher";
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ErrorLogger.ErrorLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ErrorLogger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.ProcessScheduler)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper.StatusLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_serviceHelper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingServer)).EndInit();

        }

        #endregion

        private GSF.ServiceProcess.ServiceHelper m_serviceHelper;
        private GSF.Communication.TcpServer m_remotingServer;
    }
}
