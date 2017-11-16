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
            _listener.OnNotify(this,args.ToNotifyEventArgs());
            
            //reject all proposed fixes
            return false;
        }
    }
}