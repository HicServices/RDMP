using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataQualityEngine.Data;
using HIC.Common.Validation.Constraints;

namespace DataQualityEngine.Reports.PeriodicityHelpers
{
    /// <summary>
    ///  Hypercube dimensions:
    ///     Year (PeriodicityHyperCube)
    ///     Month (PeriodicityHyperCube)
    ///         ConsequenceX/NoConsequence (PeriodicityCube)
    ///             RecordCount (PeriodicityState)
    /// </summary>
    public class PeriodicityHyperCube
    {
        private readonly string _pivotCategory;
        private List<PeriodicityCube> allCubes = new List<PeriodicityCube>();
        
        Dictionary<int,Dictionary<int,PeriodicityCube>>  hyperCube = new Dictionary<int, Dictionary<int, PeriodicityCube>>();

        public PeriodicityHyperCube(string pivotCategory)
        {
            _pivotCategory = pivotCategory;
        }

        public void PeriodicityCube()
        {
            
        }

        public void IncrementHyperCube(int year, int month, Consequence? worstConsequenceInRow)
        {
            PeriodicityCube newCube = null;

            //if year is missing
            if(!hyperCube.ContainsKey(year))
            {
                //create month dictionary
                var perMonth = new Dictionary<int, PeriodicityCube>();
                
                //add month user wants to month dictionary
                perMonth.Add(month, newCube = new PeriodicityCube(year, month));

                //add month dictionary to year dictionary
                hyperCube.Add(year,perMonth);
            }

            //if month is missing
            if (!hyperCube[year].ContainsKey(month))
                hyperCube[year].Add(month, newCube = new PeriodicityCube(year, month));//add the month to the year dictionary

            //increment the cell
            hyperCube[year][month].GetStateForConsequence(worstConsequenceInRow).CountOfRecords++;

            if(newCube != null)
                allCubes.Add(newCube);

        }

        public void CommitToDatabase(Evaluation evaluation)
        {
            allCubes.ForEach(c => c.CommitToDatabase(evaluation, _pivotCategory));
        }
    }
}
