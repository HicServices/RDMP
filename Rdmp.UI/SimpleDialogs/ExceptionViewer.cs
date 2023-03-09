// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode;


namespace Rdmp.UI.SimpleDialogs
{
    /// <summary>
    /// Used by the RDMP to tell you about something that went wrong.  You can select bits of the message text and copy them with Ctrl+C or select 'Copy to Clipboard' to copy all the
    /// message text in one go.  Clicking 'View Exception' will launch a ExceptionViewerStackTraceWithHyperlinks for viewing the location of the error in the codebase (including viewing
    /// the source code at the point of the error).
    /// </summary>
    public class ExceptionViewer : WideMessageBox
    {
        private readonly Exception _exception;
        
        public ExceptionViewer(string title, string message, Exception exception):base(new WideMessageBoxArgs(title,message,GetStackTrace(exception,Environment.StackTrace),null,WideMessageBoxTheme.Exception))
        {
            _exception = exception;

            var aggregateException = _exception as AggregateException;

            if (aggregateException != null)
            {
                _exception = aggregateException.Flatten();

                if(aggregateException.InnerExceptions.Count == 1)
                    _exception = aggregateException.InnerExceptions[0];
            }
        }

        /// <summary>
        /// Returns the first stack trace from the <paramref name="exception"/> (including examining inner exceptions where stack trace is missing).
        /// Otherwise returns <paramref name="ifNotFound"/>. 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="ifNotFound"></param>
        /// <returns></returns>
        private static string GetStackTrace(Exception exception, string ifNotFound)
        {
            while (exception != null)
            {
                if (exception.StackTrace != null)
                    return exception.StackTrace;

                exception = exception.InnerException;
            }

            return ifNotFound;
        }

        public static void Show(Exception exception, bool isModalDialog = true)
        {
            var longMessage = "";

            if(exception.InnerException != null)
                longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception.InnerException );

            ExceptionViewer ev;
            if (longMessage == "")
                ev = new ExceptionViewer(exception.GetType().Name,exception.Message, exception);
            else
                ev = new ExceptionViewer(exception.Message,longMessage, exception);

            if (isModalDialog)
                ev.ShowDialog();
            else
                ev.Show();
        }
        public static void Show(string message, Exception exception, bool isModalDialog = true)
        {
            var longMessage = "";

            //if the API user is not being silly and passing a message that is the exception anyway!
            if (message.StartsWith(exception.Message))
            {
                if (exception.InnerException != null)
                    longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception.InnerException);
            }
            else
                longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            if (message.Trim().Contains("\n"))
            {
                var split = message.Trim().Split('\n');
                message = split[0];

                longMessage = string.Join(Environment.NewLine,split.Skip(1)) + Environment.NewLine + Environment.NewLine + longMessage;
            }

            //if there's still no body to the error make the title the body and put a generic title in
            if (string.IsNullOrWhiteSpace(longMessage))
            {
                longMessage = message;
                message = "Error";
            }

            ExceptionViewer ev = new ExceptionViewer(message,longMessage,exception);

            if(isModalDialog)
                ev.ShowDialog();
            else
                ev.Show();
        }

        protected override void OnViewStackTrace()
        {
            if (ExceptionViewerStackTraceWithHyperlinks.IsSourceCodeAvailable(_exception))
                ExceptionViewerStackTraceWithHyperlinks.Show(_exception);
            else
                base.OnViewStackTrace();
        }
    }
}
