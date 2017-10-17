using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReusableLibraryCode.Progress;

namespace ReusableUIComponents.Progress
{
    public class ProgressUIEntry
    {
        public string Sender { get; private set; }
        public string Message { get; private set; }
        public ProgressEventType ProgressEventType { get; private set; }
        public DateTime EventDate { get; private set; }

        public NotifyEventArgs Args { get;private set; }
        public Exception Exception { get; set; }

        public ProgressUIEntry(object sender,DateTime eventDate, NotifyEventArgs args)
        {
            Sender = FormatSender(sender);
            Message = args.Message;
            ProgressEventType = args.ProgressEventType;
            EventDate = eventDate;
            Exception = args.Exception;
            Args = args;
        }
        
        private string FormatSender(object sender)
        {
            return sender != null ? sender.GetType().Name : "Unknown";
        }
    }
}
