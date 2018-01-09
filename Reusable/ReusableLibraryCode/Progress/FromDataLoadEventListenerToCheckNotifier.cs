using System;
using ReusableLibraryCode.Checks;

namespace ReusableLibraryCode.Progress
{
    /// <summary>
    /// Allows an IDataLoadEventListener to respond to check events like an ICheckNotifier.  All OnCheckPerformed events raised are converted to OnNotify events 
    /// and passed to the IDataLoadEventListener.  Since IDataLoadEventListeners cannot respond to ProposedFixes this class will always return false for 
    /// OnCheckPerformed (no fixes will ever be accepted).
    /// </summary>
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