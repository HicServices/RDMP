using System;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode.Progress
{
    public class FromDataLoadEventListenerToCheckNotifier : ICheckNotifier
    {
        private readonly IDataLoadEventListener _listener;

        public FromDataLoadEventListenerToCheckNotifier(IDataLoadEventListener listener)
        {
            _listener = listener;
        }

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            switch (args.Result)
            {
                case CheckResult.Success:
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,args.Message));
                    break;
                case CheckResult.Warning:
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, args.Message,args.Ex));
                    break;
                case CheckResult.Fail:
                    _listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, args.Message, args.Ex));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            //reject all proposed fixes
            return false;
        }
    }
}