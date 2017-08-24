using System;
using System.Collections.Generic;

namespace DataExportLibrary.ExtractionTime
{
    public class ExtractionTimeTimeCoverageAggregatorBucket
    {
        public enum BucketSize
        {
            Month,
            Year,
            Day
        }

        public DateTime Time { get; set; }
        public int CountOfTimesSeen { get; set; }
        public int CountOfDistinctIdentifiers {
            get { return _identifiersSeen.Count; }
        }
        private readonly HashSet<object> _identifiersSeen = new HashSet<object>();

        public ExtractionTimeTimeCoverageAggregatorBucket(DateTime time)
        {
            Time = time;
        }

        public bool IsTimeInBucket(DateTime toCheck, BucketSize bucketSize)
        {
            DateTime upperLimit = Time;
            if (bucketSize == BucketSize.Day)
                upperLimit = upperLimit.AddDays(1);
            else
                if (bucketSize == BucketSize.Month)
                    upperLimit = upperLimit.AddMonths(1);
                else
                    if (bucketSize == BucketSize.Year)
                        upperLimit = upperLimit.AddYears(1);

            return toCheck >= Time && toCheck < upperLimit;
        }

        public void SawIdentifier(object identifier)
        {
            CountOfTimesSeen++;

            if(identifier == DBNull.Value)
                return;

            if(identifier == null)
                return;
            
            if (!_identifiersSeen.Contains(identifier))
                _identifiersSeen.Add(identifier);
        }

        public static DateTime RoundDateTimeDownToNearestBucketFloor(DateTime toRound, BucketSize bucketSize)
        {
            if(bucketSize == BucketSize.Day)
                return new DateTime(toRound.Year,toRound.Month,toRound.Day);
            
            if(bucketSize == BucketSize.Month)
                return new DateTime(toRound.Year,toRound.Month,1);

            if(bucketSize == BucketSize.Year)
                return new DateTime(toRound.Year,1,1);

            throw new NotSupportedException("Unknown bucket size " + bucketSize);
        }

        public static DateTime IncreaseDateTimeBy(DateTime toAdd, BucketSize bucketSize)
        {
            if (bucketSize == BucketSize.Day)
                return toAdd.AddDays(1);

            if (bucketSize == BucketSize.Month)
                return toAdd.AddMonths(1);

            if (bucketSize == BucketSize.Year)
                return toAdd.AddYears(1);

            throw new NotSupportedException("Unknown bucket size " + bucketSize);
        }

        public static DateTime DecreaseDateTimeBy(DateTime toAdd, BucketSize bucketSize)
        {
            if (bucketSize == BucketSize.Day)
                return toAdd.AddDays(-1);

            if (bucketSize == BucketSize.Month)
                return toAdd.AddMonths(-1);

            if (bucketSize == BucketSize.Year)
                return toAdd.AddYears(-1);

            throw new NotSupportedException("Unknown bucket size " + bucketSize);
        }
    }
}