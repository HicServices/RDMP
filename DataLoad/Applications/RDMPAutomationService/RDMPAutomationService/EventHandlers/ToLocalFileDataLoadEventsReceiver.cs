using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace RDMPAutomationService.EventHandlers
{
    /// <summary>
    /// Writes OnNotify messages to the supplied file, does not store OnProgress messages (wrote 100 records to table X etc).  This is because OnProgress is a steady stream of messages updating you 
    /// on progress of a given task and there could be 1 million messages a second so we don't really want to spray all that out to a file.
    /// </summary>
    public class ToLocalFileDataLoadEventsReceiver:IDataLoadEventListener,IDisposable
    {
        private readonly bool _throwOnErrors;
        private StreamWriter _sw;

        public ToLocalFileDataLoadEventsReceiver(string filename, bool throwOnErrors = true)
        {
            _throwOnErrors = throwOnErrors;

            try
            {
                _sw = new StreamWriter(filename);
            }
            catch (Exception)
            {
                _sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), filename));
            }
            _sw.WriteLine("Time,ProgressEventType,Message,Exception");
            _sw.Flush();
        }
            
        public void OnNotify(object sender, NotifyEventArgs e)
        {
            _sw.WriteLine(
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," +
                StringToCSVCell(e.ProgressEventType.ToString()) + "," +
                StringToCSVCell(e.Message) + "," +
                (e.Exception == null?"":
                StringToCSVCell(ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception,true))
                ));
            
            _sw.Flush();

            if(_throwOnErrors && e.ProgressEventType == ProgressEventType.Error)
                throw e.Exception ?? new Exception(e.Message + e.StackTrace);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            
        }

        private string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        public void Dispose()
        {
            _sw.Close();
        }
    }
}
