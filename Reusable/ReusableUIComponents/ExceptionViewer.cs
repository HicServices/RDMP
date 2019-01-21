using System;
using ReusableLibraryCode;


namespace ReusableUIComponents
{
    /// <summary>
    /// Used by the RDMP to tell you about something that went wrong.  You can select bits of the message text and copy them with Ctrl+C or select 'Copy to Clipboard' to copy all the
    /// message text in one go.  Clicking 'View Exception' will launch a ExceptionViewerStackTraceWithHyperlinks for viewing the location of the error in the codebase (including viewing
    /// the source code at the point of the error).
    /// </summary>
    public class ExceptionViewer : WideMessageBox
    {
        private readonly Exception _exception;
        
        private ExceptionViewer(string title, string message, Exception exception):base(title,message,exception.StackTrace)
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
        
        public static void Show(Exception exception, bool isModalDialog = true)
        {
            var longMessage = "";

            if(exception.InnerException != null)
                longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception.InnerException );
            
            ExceptionViewer ev = new ExceptionViewer(exception.Message,longMessage, exception);

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
                if(exception.InnerException != null)
                    longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception.InnerException);
            }
            else
                longMessage = ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            ExceptionViewer ev = new ExceptionViewer(message,longMessage,exception);

            if(isModalDialog)
                ev.ShowDialog();
            else
                ev.Show();
        }

        protected override void OnViewStackTrace()
        {
            if (ExceptionViewerStackTraceWithHyperlinks.IsSourceCodeAvailable(_exception))
            {
                ExceptionViewerStackTraceWithHyperlinks.Show(_exception);
                return;
            }
            string exceptionAsString = ExceptionHelper.ExceptionToListOfInnerMessages(_exception, true);
            Show("Stack Trace",exceptionAsString,null,false,null,WideMessageBoxTheme.Help);
        }
    }
}
