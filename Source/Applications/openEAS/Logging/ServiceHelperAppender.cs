//******************************************************************************************************
//  ServiceHelperAppender.cs
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
//  02/27/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF;
using GSF.ServiceProcess;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace openEAS.Logging
{
    public class ServiceHelperAppender : IAppender
    {
        #region [ Members ]

        // Fields
        private string m_name;
        private ServiceHelper m_serviceHelper;

        #endregion

        #region [ Constructors ]

        public ServiceHelperAppender(ServiceHelper serviceHelper)
        {
            m_serviceHelper = serviceHelper;
            m_serviceHelper.Disposed += (sender, args) => m_serviceHelper = null;
        }

        #endregion

        #region [ Properties ]

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        #endregion

        #region [ Methods ]

        public void DoAppend(LoggingEvent loggingEvent)
        {
            object threadID;
            string renderedMessage;
            Exception ex;
            UpdateType updateType;

            // If the service helper has been disposed,
            // do not log the event
            if ((object)m_serviceHelper == null)
                return;

            // Determine the update type based on the log level
            if (loggingEvent.Level.Value >= Level.Error.Value)
                updateType = UpdateType.Alarm;
            else if (loggingEvent.Level.Value >= Level.Warn.Value)
                updateType = UpdateType.Warning;
            else if (loggingEvent.Level.Value >= Level.Info.Value)
                updateType = UpdateType.Information;
            else
                return;

            // Determine the thread ID from the thread's context
            threadID = ThreadContext.Properties["ID"] ?? "0";

            // Get the message and exception object
            renderedMessage = loggingEvent.RenderedMessage;
            ex = loggingEvent.ExceptionObject;

            // If the user didn't supply a message,
            // attempt to use the exception message instead
            if (string.IsNullOrEmpty(renderedMessage))
                renderedMessage = loggingEvent.GetExceptionString();

            // If the event was an exception event,
            // also log to the service helper's error log
            if ((object)ex != null)
            {
                if ((object)m_serviceHelper.ErrorLogger != null)
                    m_serviceHelper.ErrorLogger.Log(ex);

                if (string.IsNullOrEmpty(renderedMessage))
                    renderedMessage = ex.Message;
            }

            // Send the message to clients via the service helper
            if (!string.IsNullOrEmpty(renderedMessage))
                m_serviceHelper.UpdateStatusAppendLine(updateType, "[{0}] {1}", threadID, renderedMessage);
            else
                m_serviceHelper.UpdateStatusAppendLine(updateType, "");
        }

        public void Close()
        {
        }

        #endregion
    }
}
