using System.Collections.Generic;
using System.Data.Common;
using DataQualityEngine.Data;
using HIC.Common.Validation.Constraints;

namespace DataQualityEngine.Reports.PeriodicityHelpers
{
    /// <summary>
    /// Records the number of records passing / failing validation with each consquence (See PeriodicityState).
    /// 
    /// This class handles the Consequence dimension (See PeriodicityCubesOverTime for the time aspect handling).
    /// </summary>
    public class PeriodicityCube
    {
        readonly Dictionary<Consequence, PeriodicityState> _consequenceCube = new Dictionary<Consequence, PeriodicityState>();
        readonly PeriodicityState _passingValidation;

        public PeriodicityCube(int year, int month)
        {
            _passingValidation = new PeriodicityState(year, month, null);

            _consequenceCube.Add(Consequence.Missing, new PeriodicityState(year, month, Consequence.Missing));
            _consequenceCube.Add(Consequence.Wrong, new PeriodicityState(year, month, Consequence.Wrong));
            _consequenceCube.Add(Consequence.InvalidatesRow, new PeriodicityState(year, month, Consequence.InvalidatesRow));

        }
        public PeriodicityState GetStateForConsequence(Consequence? consequence)
        {
            if (consequence == null)
                return _passingValidation;

            return _consequenceCube[(Consequence)consequence];
        }

        public void CommitToDatabase(Evaluation evaluation, string pivotCategory)
        {
            foreach (PeriodicityState state in _consequenceCube.Values)
                state.Commit(evaluation, pivotCategory);

            _passingValidation.Commit(evaluation, pivotCategory);
        }
    }
}