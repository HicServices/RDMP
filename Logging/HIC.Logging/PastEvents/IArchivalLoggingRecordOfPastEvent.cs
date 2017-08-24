using System;

namespace HIC.Logging.PastEvents
{
    public interface IArchivalLoggingRecordOfPastEvent: IComparable
    {
        int ID { get; }
    }
}