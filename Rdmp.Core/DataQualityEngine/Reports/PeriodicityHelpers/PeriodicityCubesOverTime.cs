// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Validation.Constraints;

namespace Rdmp.Core.DataQualityEngine.Reports.PeriodicityHelpers;

/// <summary>
/// Accumulates counts for populating into the PeriodicityState table of the DQE.  This table contains row evaluation counts (passing / failing validation) and
/// 4 dimensions:
///  Evaluation_ID - when in realtime the DQE was run (e.g. Evaluation run on Feb 2017)
///  Year/Month - what in dataset time the result is for (e.g. biochemistry records relating to tests conducted during January 2013)
///  Pivot Category - optional column value subdivision (e.g. Healthboard column is T or F)
///  Row Evaluation - final dimension is one record per Consequence of failed validation (Wrong / Missing / Correct etc).
/// 
/// <para>This class manages the time aspect as a Dictionary of year/month.  Other dimensions are managed by PeriodicityCube</para>
/// </summary>
public class PeriodicityCubesOverTime
{
    private readonly string _pivotCategory;
    private List<PeriodicityCube> allCubes = new();
    private Dictionary<int, Dictionary<int, PeriodicityCube>> hyperCube = new();

    public PeriodicityCubesOverTime(string pivotCategory)
    {
        _pivotCategory = pivotCategory;
    }

    public Dictionary<int, Dictionary<int, PeriodicityCube>> GetHyperCube()
    {
        return hyperCube;
    }

    public string GetPivotCategory()
    {
        return _pivotCategory;
    }

    public static void PeriodicityCube()
    {
    }

    public void IncrementHyperCube(int year, int month, Consequence? worstConsequenceInRow)
    {
        PeriodicityCube newCube = null;

        //if year is missing
        if (!hyperCube.TryGetValue(year, out var cubes))
        {
            //create month dictionary
            var perMonth = new Dictionary<int, PeriodicityCube> {
                //add month user wants to month dictionary
                { month, newCube = new PeriodicityCube(year, month) } };
            cubes = perMonth;

            //add month dictionary to year dictionary
            hyperCube.Add(year, cubes);
        }

        //if month is missing
        if (!cubes.ContainsKey(month))
            cubes.Add(month, newCube = new PeriodicityCube(year, month)); //add the month to the year dictionary
        cubes[month].GetStateForConsequence(worstConsequenceInRow).CountOfRecords++;

        if (newCube != null)
            allCubes.Add(newCube);
    }

    internal void CommitToDatabase(Evaluation evaluation)
    {
        allCubes.ForEach(c => c.CommitToDatabase(evaluation, _pivotCategory));
    }
}