using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReusableLibraryCode.Progress
{
    public class ToMemoryDataLoadEventReceiver:IDataLoadEventListener
    {
        private readonly bool _throwOnErrorEvents;
        public Dictionary<object,List<NotifyEventArgs>> EventsReceivedBySender = new Dictionary<object, List<NotifyEventArgs>>();
        public Dictionary<string,ProgressEventArgs> LastProgressRecieivedByTaskName = new Dictionary<string, ProgressEventArgs>();

        public ToMemoryDataLoadEventReceiver(bool throwOnErrorEvents)
        {
            _throwOnErrorEvents = throwOnErrorEvents;
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {

            if(e.ProgressEventType == ProgressEventType.Error && _throwOnErrorEvents)
                if (e.Exception != null)
                    throw e.Exception;
                else
                    throw new Exception(e.Message);


            if(!EventsReceivedBySender.ContainsKey(sender))
                EventsReceivedBySender.Add(sender,new List<NotifyEventArgs>());

            EventsReceivedBySender[sender].Add(e);
        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
            if (!LastProgressRecieivedByTaskName.ContainsKey(e.TaskDescription))
                LastProgressRecieivedByTaskName.Add(e.TaskDescription, e);//add progress on new item
            else
                LastProgressRecieivedByTaskName[e.TaskDescription] = e;//replace last progress
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<object, List<NotifyEventArgs>> kvp in EventsReceivedBySender)
            {
                sb.AppendLine(kvp.Key + " Messages:");
                foreach (var msg in kvp.Value)
                    sb.AppendLine(msg.ProgressEventType +":"+ msg.Message);

            }

            foreach (var kvp in LastProgressRecieivedByTaskName)
                sb.AppendLine(kvp.Key + " " + kvp.Value.Progress.Value + " " + kvp.Value.Progress.UnitOfMeasurement);

            return sb.ToString();
        }

        public ProgressEventType GetWorst()
        {
            return EventsReceivedBySender.Values.Max(v=>v.Max(e=>e.ProgressEventType));
        }
    }
}