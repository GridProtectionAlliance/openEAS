//*********************************************************************************************************************
//  ServiceClient.Designer.cs
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

namespace PQMarkPusherConsole
{
    partial class ServiceClient
    {
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
            if (disposing)
            {
                m_clientHelper.Disconnect();
                if (components != null)
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
            this.m_clientHelper = new GSF.ServiceProcess.ClientHelper(this.components);
            this.m_remotingClient = new GSF.Communication.TcpClient(this.components);
            this.m_errorLogger = new GSF.ErrorManagement.ErrorLogger(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.m_clientHelper)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingClient)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_errorLogger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_errorLogger.ErrorLog)).BeginInit();
            // 
            // m_clientHelper
            // 
            this.m_clientHelper.PersistSettings = true;
            this.m_clientHelper.RemotingClient = this.m_remotingClient;
            // 
            // m_remotingClient
            // 
            this.m_remotingClient.ConnectionString = "Server=localhost:8888";
            this.m_remotingClient.IntegratedSecurity = true;
            this.m_remotingClient.PayloadAware = true;
            this.m_remotingClient.PersistSettings = true;
            this.m_remotingClient.SettingsCategory = "RemotingClient";
            // 
            // m_errorLogger
            // 
            // 
            // 
            // 
            this.m_errorLogger.ErrorLog.FileName = "PQMarkPusherConsole.ErrorLog.txt";
            this.m_errorLogger.LogToUI = true;
            ((System.ComponentModel.ISupportInitialize)(this.m_clientHelper)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_remotingClient)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_errorLogger.ErrorLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_errorLogger)).EndInit();

        }

        #endregion

        private GSF.ServiceProcess.ClientHelper m_clientHelper;
        private GSF.Communication.TcpClient m_remotingClient;
        private GSF.ErrorManagement.ErrorLogger m_errorLogger;


    }
}
