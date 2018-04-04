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
    /// Accumulates counts for populating into the PeriodicityState table of the DQE.  This table contains row evaluation counts (passing / failing validation) and
    /// 4 dimensions:
    ///  Evaluation_ID - when in realtime the DQE was run (e.g. Evaluation run on Feb 2017)
    ///  Year/Month - what in dataset time the result is for (e.g. biochemistry records relating to tests conducted during January 2013)
    ///  Pivot Category - optional column value subdivision (e.g. Healthboard column is T or F)
    ///  Row Evaluation - final dimension is one record per Consquence of failed validation (Wrong / Missing / Correct etc).
    /// 
    /// <para>This class manages the time aspect as a Dictionary of year/month.  Other dimensions are managed by PeriodicityCube</para>
    /// </summary>
    public class PeriodicityCubesOverTime
    {
        private readonly string _pivotCategory;
        private List<PeriodicityCube> allCubes = new List<PeriodicityCube>();
        
        Dictionary<int,Dictionary<int,PeriodicityCube>>  hyperCube = new Dictionary<int, Dictionary<int, PeriodicityCube>>();

        public PeriodicityCubesOverTime(string pivotCategory)
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
