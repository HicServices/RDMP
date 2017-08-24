using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace HIC.Logging.Listeners
{
    public class ToLoggingDatabaseDataLoadEventListener : IDataLoadEventListener
    {

        public Dictionary<string, ITableLoadInfo> TableLoads = new Dictionary<string, ITableLoadInfo>();

        private readonly object _hostingApplication;
        private readonly LogManager _logManager;
        private readonly string _loggingTask;
        private readonly string _runDescription;
        private IDataLoadInfo _dataLoadInfo;

        public ToLoggingDatabaseDataLoadEventListener(object hostingApplication, LogManager logManager, string loggingTask, string runDescription)
        {
            _hostingApplication = hostingApplication;
            _logManager = logManager;
            _loggingTask = loggingTask;
            _runDescription = runDescription;
        }

        public ToLoggingDatabaseDataLoadEventListener(LogManager logManager, IDataLoadInfo dataLoadInfo)
        {
            _dataLoadInfo = dataLoadInfo;
            _logManager = logManager;
        }

        private void StartLogging()
        {
            _dataLoadInfo = _logManager.CreateDataLoadInfo(_loggingTask, _hostingApplication.ToString(), _runDescription, "", false);
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (_dataLoadInfo == null)
                StartLogging();

            switch (e.ProgressEventType)
            {
                case ProgressEventType.Information:
                    _logManager.LogProgress(_dataLoadInfo, ProgressLogging.ProgressEventType.OnInformation, sender.ToString(),e.Message);
                    break;
                case ProgressEventType.Warning:
                    string msg = e.Message + (e.Exception == null?"": Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception,true));
                    _logManager.LogProgress(_dataLoadInfo, ProgressLogging.ProgressEventType.OnWarning, sender.ToString(),msg);
                    break;
                case ProgressEventType.Error:
                    string err = e.Message + (e.Exception == null ? "" : Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(e.Exception, true));
                    _logManager.LogFatalError(_dataLoadInfo,sender.ToString(),err);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if (_dataLoadInfo == null)
                StartLogging();
            
            Debug.Assert(_dataLoadInfo != null, "_dataLoadInfo != null");

            if (e.Progress.UnitOfMeasurement == ProgressType.Records)
            {
                //if(!tableLoads.Any(tbl=>tbl.))
                if(!TableLoads.ContainsKey(e.TaskDescription))
                {
                    var t = _dataLoadInfo.CreateTableLoadInfo("", e.TaskDescription, new[] {new DataSource(sender.ToString())},e.Progress.KnownTargetValue);
                    TableLoads.Add(e.TaskDescription,t);
                }

                if (e.Progress.Value < TableLoads[e.TaskDescription].Inserts)
                    throw new Exception("Received OnProgress event with a TaskDescription '" + e.TaskDescription +"' which has the same name as a previous Task but the number of Inserts is lower in this event.  Progress is not allowed to go backwards!");

                TableLoads[e.TaskDescription].Inserts = e.Progress.Value;
            }
        }

        public void FinalizeTableLoadInfos()
        {
            foreach (var tableLoadInfo in TableLoads.Values)
                tableLoadInfo.CloseAndArchive();

            _dataLoadInfo.CloseAndMarkComplete();
        }
    }
}
