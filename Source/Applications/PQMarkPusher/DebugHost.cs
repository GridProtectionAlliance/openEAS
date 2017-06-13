//*********************************************************************************************************************
//  DebugHost.cs
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

using System;
using System.Windows.Forms;
using GSF.Reflection;

namespace PQMarkPusher
{
    public partial class DebugHost : Form
    {
        #region [ Members ]

        // Fields
        private string m_productName;

        #endregion

        #region [ Properties ]

        public ServiceHost Host {
            get {
                return m_serviceHost;
            }
        }

        #endregion
        
        #region [ Constructors ]

        public DebugHost()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void DebugHost_Load(object sender, EventArgs e)
        {
            // Initialize text.
            m_productName = AssemblyInfo.EntryAssembly.Title;
            this.Text = string.Format(this.Text, m_productName);
            m_notifyIcon.Text = string.Format(m_notifyIcon.Text, m_productName);
            LabelNotice.Text = string.Format(LabelNotice.Text, m_productName);
            m_exitToolStripMenuItem.Text = string.Format(m_exitToolStripMenuItem.Text, m_productName);

            // Minimize the window.
            this.WindowState = FormWindowState.Minimized;

            // Start the windows service.
            m_serviceHost.StartDebugging(Environment.CommandLine.Split(' '));
        }

        private void DebugHost_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show(string.Format("Are you sure you want to stop {0} windows service? ",
                m_productName), "Stop Service", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Stop the windows service.
                m_serviceHost.StopDebugging();
            }
            else
            {
                // Stop the application from exiting.
                e.Cancel = true;
            }
        }

        private void DebugHost_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                // Don't show the window in taskbar when minimized.
                this.ShowInTaskbar = false;
            }
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the window in taskbar the in normal mode (visible).
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Close this window which will cause the application to exit.
            this.Close();
        }

        #endregion
    }
}
