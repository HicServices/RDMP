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
/// Records the number of records passing / failing validation with each consequence (See PeriodicityState).
/// 
/// <para>This class handles the Consequence dimension (See PeriodicityCubesOverTime for the time aspect handling).</para>
/// </summary>
public class PeriodicityCube
{
    private readonly Dictionary<Consequence, PeriodicityState> _consequenceCube = new();
    private readonly PeriodicityState _passingValidation;

    public PeriodicityCube(int year, int month)
    {
        _passingValidation = new PeriodicityState(year, month, null);

        _consequenceCube.Add(Consequence.Missing, new PeriodicityState(year, month, Consequence.Missing));
        _consequenceCube.Add(Consequence.Wrong, new PeriodicityState(year, month, Consequence.Wrong));
        _consequenceCube.Add(Consequence.InvalidatesRow, new PeriodicityState(year, month, Consequence.InvalidatesRow));
    }

    public PeriodicityState GetStateForConsequence(Consequence? consequence) =>
        consequence == null ? _passingValidation : _consequenceCube[(Consequence)consequence];

    public void CommitToDatabase(Evaluation evaluation, string pivotCategory)
    {
        foreach (var state in _consequenceCube.Values)
            state.Commit(evaluation, pivotCategory);

        _passingValidation.Commit(evaluation, pivotCategory);
    }
}