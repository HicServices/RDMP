using System;
using System.Collections.Generic;
using System.Linq;

namespace ReusableLibraryCode.Checks
{
    public class ToMemoryCheckNotifier : ICheckNotifier
    {
        private readonly ICheckNotifier _childToPassEventsTo;
        public List<CheckEventArgs> Messages { get; private set; }

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
            Messages.Add(args);

            if (_childToPassEventsTo != null)
                return _childToPassEventsTo.OnCheckPerformed(args);

            return false;
        }

        public CheckResult GetWorst()
        {
            if (!Messages.Any())
                return CheckResult.Success;

            return Messages.Max(e => e.Result);
        }

        public void WriteToConsole()
        {
            foreach (var msg in Messages)
            {
                Console.WriteLine("(" + msg.Result + ")" + msg.Message);

                if (msg.Ex != null)
                    Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(msg.Ex));
            }

        }
    }
}