using System;
using System.Collections.Generic;
using System.Linq;

namespace ReusableLibraryCode.Checks
{
    /// <summary>
    /// ICheckNotifier which records all CheckEventArgs received into a public List for later evaluation.  Primarily for use in testing to check for specific 
    /// messages.  Can also be used with a ReplayCheckable in order to check a component (or multiple components) and then at a later time replay the events into
    /// a UI.
    /// </summary>
    public class ToMemoryCheckNotifier : ICheckNotifier
    {
        private readonly ICheckNotifier _childToPassEventsTo;
        public List<CheckEventArgs> Messages { get; private set; }

        public object lockList = new object();

        /// <summary>
        /// Sometimes you want to know what the messages and the worst event encountered were but you still want to pass the messages to a third party e.g.
        /// checksUI, use this constructor to record all messages in memory as they go through but also let the supplied child check notifier actually handle
        /// those events and pass back the bool that child supplies for proposed fixes
        /// </summary>
        /// <param name="childToPassEventsTo"></param>
        public ToMemoryCheckNotifier(ICheckNotifier childToPassEventsTo)
        {
            _childToPassEventsTo = childToPassEventsTo;
            Messages = new List<CheckEventArgs>();
        }

        public ToMemoryCheckNotifier()
        {
            Messages  = new List<CheckEventArgs>();
        }

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            lock (lockList)
            {
                Messages.Add(args);
            }

            if (_childToPassEventsTo != null)
            {
                bool fix = _childToPassEventsTo.OnCheckPerformed(args);

                //if child accepted the fix
                if(fix && !string.IsNullOrWhiteSpace(args.ProposedFix) && args.Result == CheckResult.Fail)
                    args.Result = CheckResult.Warning;
                
                return fix;
            }

            return false;
        }

        public CheckResult GetWorst()
        {
            lock (lockList)
            {
                if (!Messages.Any())
                    return CheckResult.Success;

                return Messages.Max(e => e.Result);
            }
        }
    }
}