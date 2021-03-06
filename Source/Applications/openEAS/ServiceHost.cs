﻿//*********************************************************************************************************************
//  ServiceHost.cs
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using GSF;
using GSF.Console;
using GSF.IO;
using GSF.ServiceProcess;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using openEAS.Logging;
using GSF.Configuration;
using GSF.Data;
using GSF.TimeSeries;
using openEAS.Model;
using GSF.Web.Hosting;
using Microsoft.Owin.Hosting;
using GSF.Reflection;
using GSF.Web.Model;
using System.Reflection;
using openEASSandBox;
using GSF.Security.Model;
using GSF.Web.Security;

namespace openEAS
{
    public partial class ServiceHost : ServiceBase
    {
        #region [ Members ]

        // Fields
        private ServiceMonitors m_serviceMonitors;
        private SandBoxEngine m_extensibleDisturbanceAnalysisEngine;
        private IDisposable m_webAppHost;
        private bool m_disposed;

        /// <summary>
        /// Raised when there is a new status message reported to service.
        /// </summary>
        public event EventHandler<EventArgs<Guid, string, UpdateType>> UpdatedStatus;



        /// <summary>
        /// Raised when there is a new exception logged to service.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> LoggedException;


        #endregion

        #region [ Constructors ]

        public ServiceHost()
        {
            InitializeComponent();

            // Register event handlers.
            m_serviceHelper.ServiceStarted += ServiceHelper_ServiceStarted;
            m_serviceHelper.ServiceStopping += ServiceHelper_ServiceStopping;
        }

        public ServiceHost(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Properties ]
        /// <summary>
        /// Gets the configured default web page for the application.
        /// </summary>
        public string DefaultWebPage
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the model used for the application.
        /// </summary>
        public AppModel Model
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets current performance statistics.
        /// </summary>
        public string PerformanceStatistics => m_extensibleDisturbanceAnalysisEngine.Status;


        #endregion

        #region [ Methods ]


        private void ServiceHelper_ServiceStarted(object sender, EventArgs e)
        {
            ServiceHelperAppender serviceHelperAppender;
            RollingFileAppender fileAppender;
            ServiceProcess process;

            // Set current working directory to fix relative paths
            Directory.SetCurrentDirectory(FilePath.GetAbsolutePath(""));

            // Set up logging
            serviceHelperAppender = new ServiceHelperAppender(m_serviceHelper);

            fileAppender = new RollingFileAppender();
            fileAppender.StaticLogFileName = false;
            fileAppender.AppendToFile = true;
            fileAppender.RollingStyle = RollingFileAppender.RollingMode.Composite;
            fileAppender.MaxSizeRollBackups = 10;
            fileAppender.PreserveLogFileNameExtension = true;
            fileAppender.MaximumFileSize = "1MB";
            fileAppender.Layout = new PatternLayout("%date [%thread] %-5level %logger - %message%newline");

            try
            {
                if (!Directory.Exists("Debug"))
                    Directory.CreateDirectory("Debug");

                fileAppender.File = @"Debug\openEAS.log";
            }
            catch (Exception ex)
            {
                fileAppender.File = "openEAS.log";
                m_serviceHelper.ErrorLogger.Log(ex);
            }

            fileAppender.ActivateOptions();
            BasicConfigurator.Configure(serviceHelperAppender, fileAppender);
            // Set up the analysis engine
            m_extensibleDisturbanceAnalysisEngine = new SandBoxEngine();
            AppDomain.CurrentDomain.AssemblyResolve += m_extensibleDisturbanceAnalysisEngine.AssemblyResolveHandler;

            // Set up heartbeat and client request handlers
            m_serviceHelper.AddScheduledProcess(ProcessLatestData, "ProcessLatestData", "* * * * *");
            m_serviceHelper.AddScheduledProcess(ServiceHeartbeatHandler, "ServiceHeartbeat", "* * * * *");
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("ReloadSystemSettings", "Reloads system settings from the database", ReloadSystemSettingsRequestHandler));
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("MsgServiceMonitors", "Sends a message to all service monitors", MsgServiceMonitorsRequestHandler));

            // Set up adapter loader to load service monitors
            m_serviceMonitors = new ServiceMonitors();
            m_serviceMonitors.AdapterCreated += ServiceMonitors_AdapterCreated;
            m_serviceMonitors.AdapterLoaded += ServiceMonitors_AdapterLoaded;
            m_serviceMonitors.AdapterUnloaded += ServiceMonitors_AdapterUnloaded;
            m_serviceHelper.UpdatedStatus += UpdatedStatusHandler;

            m_serviceMonitors.Initialize();


            // Process latest data at startup
            process = m_serviceHelper.FindProcess("ProcessLatestData");

            if ((object)process != null)
                process.Start();

            bool webUI = false;

            while(!webUI)
                webUI = TryStartWebUI();
        }

        private void ProcessLatestData(string arg1, object[] arg2)
        {
            try
            {
                m_extensibleDisturbanceAnalysisEngine.ProcessLatestData();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            if (!m_disposed)
            {
                try
                {
                    m_webAppHost?.Dispose();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose();    // Call base class Dispose().
                }
            }

            // Dispose of adapter loader for service monitors
            m_serviceMonitors.AdapterLoaded -= ServiceMonitors_AdapterLoaded;
            m_serviceMonitors.AdapterUnloaded -= ServiceMonitors_AdapterUnloaded;
            m_serviceHelper.UpdatedStatus -= UpdatedStatusHandler;

            m_serviceMonitors.Dispose();
        }

        private void ServiceHeartbeatHandler(string s, object[] args)
        {
            // Go through all service monitors to notify of the heartbeat
            foreach (IServiceMonitor serviceMonitor in m_serviceMonitors.Adapters)
            {
                try
                {
                    // If the service monitor is enabled, notify it of the heartbeat
                    if (serviceMonitor.Enabled)
                        serviceMonitor.HandleServiceHeartbeat();
                }
                catch (Exception ex)
                {
                    // Handle each service monitor's exceptions individually
                    HandleException(ex);
                }
            }
        }

        // Reloads system settings from the database
        private void ReloadSystemSettingsRequestHandler(ClientRequestInfo requestInfo)
        {
            m_extensibleDisturbanceAnalysisEngine.ReloadSystemSettings();
        }

        // Send a message to the service monitors on request
        private void MsgServiceMonitorsRequestHandler(ClientRequestInfo requestInfo)
        {
            Arguments arguments = requestInfo.Request.Arguments;

            if (arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Sends a message to all service monitors.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       MsgServiceMonitors [Options] [Args...]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                string[] args = Enumerable.Range(1, arguments.OrderedArgCount)
                    .Select(arg => arguments[arguments.OrderedArgID + arg])
                    .ToArray();

                // Go through all service monitors and handle the message
                foreach (IServiceMonitor serviceMonitor in m_serviceMonitors.Adapters)
                {
                    try
                    {
                        // If the service monitor is enabled, notify it of the message
                        if (serviceMonitor.Enabled)
                            serviceMonitor.HandleClientMessage(args);
                    }
                    catch (Exception ex)
                    {
                        // Handle each service monitor's exceptions individually
                        HandleException(ex);
                    }
                }

                SendResponse(requestInfo, true);
            }
        }

        // Send the error to the service helper, error logger, and each service monitor
        private void HandleException(Exception ex)
        {
            string newLines = string.Format("{0}{0}", Environment.NewLine);

            m_serviceHelper.ErrorLogger.Log(ex);
            m_serviceHelper.UpdateStatus(UpdateType.Alarm, "{0}", ex.Message + newLines);

            foreach (IServiceMonitor serviceMonitor in m_serviceMonitors.Adapters)
            {
                try
                {
                    if (serviceMonitor.Enabled)
                        serviceMonitor.HandleServiceError(ex);
                }
                catch (Exception ex2)
                {
                    // Exceptions encountered while handling exceptions can be tricky,
                    // so we just log them rather than risk a recursive loop
                    m_serviceHelper.ErrorLogger.Log(ex2);
                    m_serviceHelper.UpdateStatus(UpdateType.Alarm, ex2.Message + newLines);
                }
            }
        }


        /// <summary>
        /// Sends a command request to the service.
        /// </summary>
        /// <param name="clientID">Client ID of sender.</param>
        /// <param name="userInput">Request string.</param>
        public void SendRequest(Guid clientID, string userInput)
        {
            ClientRequest request = ClientRequest.Parse(userInput);

            if ((object)request != null)
            {
                ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(request.Command);

                if ((object)requestHandler != null)
                    requestHandler.HandlerMethod(new ClientRequestInfo(new ClientInfo() { ClientID = clientID }, request));
                else
                    DisplayStatusMessage($"Command \"{request.Command}\" is not supported\r\n\r\n", UpdateType.Alarm);
            }
        }


        public void DisconnectClient(Guid clientID)
        {
            m_serviceHelper.DisconnectClient(clientID);
        }


        #region [ Service Monitor Handlers ]

        // Ensure that service monitors save their settings to the configuration file
        private void ServiceMonitors_AdapterCreated(object sender, EventArgs<IServiceMonitor> e)
        {
            e.Argument.PersistSettings = true;
        }

        // Display a message when service monitors are loaded
        private void ServiceMonitors_AdapterLoaded(object sender, EventArgs<IServiceMonitor> e)
        {
            m_serviceHelper.UpdateStatusAppendLine(UpdateType.Information, "{0} has been loaded", e.Argument.GetType().Name);
        }

        // Display a message when service monitors are unloaded
        private void ServiceMonitors_AdapterUnloaded(object sender, EventArgs<IServiceMonitor> e)
        {
            m_serviceHelper.UpdateStatusAppendLine(UpdateType.Information, "{0} has been unloaded", e.Argument.GetType().Name);
        }

        #endregion

        #region [ Broadcast Message Handling ]

        /// <summary>
        /// Sends an actionable response to client.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success)
        {
            SendResponseWithAttachment(requestInfo, success, null, null);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message and attachment.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="attachment">Attachment to send with response.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponseWithAttachment(ClientRequestInfo requestInfo, bool success, object attachment, string status, params object[] args)
        {
            try
            {
                // Send actionable response
                m_serviceHelper.SendActionableResponse(requestInfo, success, attachment, status, args);

                // Log details of client request as well as response
                if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
                {
                    string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
                    string arguments = requestInfo.Request.Arguments.ToString();
                    string message = responseType + (string.IsNullOrWhiteSpace(arguments) ? "" : "(" + arguments + ")");

                    if (status != null)
                    {
                        if (args.Length == 0)
                            message += " - " + status;
                        else
                            message += " - " + string.Format(status, args);
                    }

                    m_serviceHelper.StatusLog.WriteTimestampedLine(message);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to send client response due to an exception: {0}", ex.Message);
                HandleException(new InvalidOperationException(message, ex));
            }
        }

        /// <summary>
        /// Displays a response message to client requestor.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            try
            {
                m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, "{0}\r\n\r\n", status, args);
            }
            catch (Exception ex)
            {
                string message = string.Format("Failed to update client status \"{0}\" due to an exception: {1}", status.ToNonNullString(), ex.Message);
                HandleException(new InvalidOperationException(message, ex));
            }
        }

        private void UpdatedStatusHandler(object sender, EventArgs<Guid, string, UpdateType> e)
        {
            if ((object)UpdatedStatus != null)
                UpdatedStatus(sender, new EventArgs<Guid, string, UpdateType>(e.Argument1, e.Argument2, e.Argument3));
        }


        #endregion

        #region [ Web UI Handling]
        // Attempts to start the web UI and logs startup errors.
        private bool TryStartWebUI()
        {
            try
            {
                ConfigurationFile.Current.Reload();
                AdoDataConnection.ReloadConfigurationSettings();

                CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
                CategorizedSettingsElementCollection securityProvider = ConfigurationFile.Current.Settings["securityProvider"];
                systemSettings.Add("ConnectionString", "Data Source=localhost; Initial Catalog=openXDA; Integrated Security=SSPI", "Configuration database ADO.NET data provider assembly type creation string used when ConfigurationType=Database");
                systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089}; ConnectionType=System.Data.SqlClient.SqlConnection; AdapterType=System.Data.SqlClient.SqlDataAdapter", "Configuration database ADO.NET data provider assembly type creation string used when ConfigurationType=Database");
                systemSettings.Add("NodeID", "00000000-0000-0000-0000-000000000000", "Unique Node ID");
                systemSettings.Add("CompanyName", "Grid Protection Alliance", "The name of the company who owns this instance of the openMIC.");
                systemSettings.Add("CompanyAcronym", "GPA", "The acronym representing the company who owns this instance of the openMIC.");
                systemSettings.Add("WebHostURL", "https://+:8790", "The web hosting URL for remote system management.");
                systemSettings.Add("DefaultWebPage", "index.cshtml", "The default web page for the hosted web server.");
                systemSettings.Add("DateFormat", "MM/dd/yyyy", "The default date format to use when rendering timestamps.");
                systemSettings.Add("TimeFormat", "HH:mm.ss.fff", "The default time format to use when rendering timestamps.");
                systemSettings.Add("BootstrapTheme", "Content/bootstrap.min.css", "Path to Bootstrap CSS to use for rendering styles.");

                securityProvider.Add("ConnectionString", "Eval(systemSettings.ConnectionString)", "Connection connection string to be used for connection to the backend security datastore.");
                securityProvider.Add("DataProviderString", "Eval(systemSettings.DataProviderString)", "Configuration database ADO.NET data provider assembly type creation string to be used for connection to the backend security datastore.");
                systemSettings.Add("DefaultCorsOrigins", "", "Comma-separated list of allowed origins (including http:// prefix) that define the default CORS policy. Use '*' to allow all or empty string to disable CORS.");
                systemSettings.Add("DefaultCorsHeaders", "*", "Comma-separated list of supported headers that define the default CORS policy. Use '*' to allow all or empty string to allow none.");
                systemSettings.Add("DefaultCorsMethods", "*", "Comma-separated list of supported methods that define the default CORS policy. Use '*' to allow all or empty string to allow none.");
                systemSettings.Add("DefaultCorsSupportsCredentials", true, "Boolean flag for the default CORS policy indicating whether the resource supports user credentials in the request.");


                DefaultWebPage = systemSettings["DefaultWebPage"].Value;

                Model = new AppModel();
                Model.Global.CompanyName = systemSettings["CompanyName"].Value;
                Model.Global.CompanyAcronym = systemSettings["CompanyAcronym"].Value;
                Model.Global.ApplicationName = "openEAS";
                Model.Global.ApplicationDescription = "open eXtensible Disturbance Analytics";
                Model.Global.ApplicationKeywords = "open source, utility, software, meter, interrogation";
                Model.Global.DateFormat = systemSettings["DateFormat"].Value;
                Model.Global.TimeFormat = systemSettings["TimeFormat"].Value;
                Model.Global.DateTimeFormat = $"{Model.Global.DateFormat} {Model.Global.TimeFormat}";
                Model.Global.BootstrapTheme = systemSettings["BootstrapTheme"].Value;
                Model.Global.DefaultCorsOrigins = systemSettings["DefaultCorsOrigins"].Value;
                Model.Global.DefaultCorsHeaders = systemSettings["DefaultCorsHeaders"].Value;
                Model.Global.DefaultCorsMethods = systemSettings["DefaultCorsMethods"].Value;
                Model.Global.DefaultCorsSupportsCredentials = systemSettings["DefaultCorsSupportsCredentials"].ValueAsBoolean(true);


                // Attach to default web server events
                WebServer webServer = WebServer.Default;
                webServer.StatusMessage += WebServer_StatusMessage;
                webServer.ExecutionException += LoggedExceptionHandler;


                // Define types for Razor pages - self-hosted web service does not use view controllers so
                // we must define configuration types for all paged view model based Razor views here:
                webServer.PagedViewModelTypes.TryAdd("Result.cshtml", new Tuple<Type, Type>(typeof(OpenEASResult), typeof(DataHub)));
                webServer.PagedViewModelTypes.TryAdd("Users.cshtml", new Tuple<Type, Type>(typeof(UserAccount), typeof(SecurityHub)));
                webServer.PagedViewModelTypes.TryAdd("Groups.cshtml", new Tuple<Type, Type>(typeof(SecurityGroup), typeof(SecurityHub)));

                // Create new web application hosting environment
                m_webAppHost = WebApp.Start<Startup>(systemSettings["WebHostURL"].Value);

                // Initiate pre-compile of base templates
                if (AssemblyInfo.EntryAssembly.Debuggable)
                {
                    RazorEngine<CSharpDebug>.Default.PreCompile(HandleException);
                    RazorEngine<VisualBasicDebug>.Default.PreCompile(HandleException);
                }
                else
                {
                    RazorEngine<CSharp>.Default.PreCompile(HandleException);
                    RazorEngine<VisualBasic>.Default.PreCompile(HandleException);
                }

                return true;
            }
            catch (TargetInvocationException ex)
            {
                string message;

                // Log the exception
                message = "Failed to start web UI due to exception: " + ex.InnerException.Message;
                HandleException(new InvalidOperationException(message, ex));

                return false;
            }
            catch (Exception ex)
            {
                string message;

                // Log the exception
                message = "Failed to start web UI due to exception: " + ex.Message;
                HandleException(new InvalidOperationException(message, ex));

                return false;
            }
        }

        private void WebServer_StatusMessage(object sender, EventArgs<string> e)
        {
            //DisplayStatusMessage(e.Argument, UpdateType.Information);
        }

        private void LoggedExceptionHandler(object sender, EventArgs<Exception> e)
        {
            if ((object)LoggedException != null)
                LoggedException(sender, new EventArgs<Exception>(e.Argument));
        }

        /// <summary>
        /// Logs a status message to connected clients.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="type">Type of message to log.</param>
        public void LogStatusMessage(string message, UpdateType type = UpdateType.Information)
        {
            DisplayStatusMessage(message, type);
        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        protected virtual void DisplayStatusMessage(string status, UpdateType type)
        {
            try
            {
                m_serviceHelper.UpdateStatus(type, "{0}\r\n\r\n", status);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Logs an exception to the service.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public void LogException(Exception ex)
        {
            HandleException(ex);
        }


        #endregion

        #endregion
    }
}
