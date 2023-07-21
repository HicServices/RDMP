// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Clears progress recorded in an <see cref="ExtractionProgress" />
/// </summary>
public class ExecuteCommandResetExtractionProgress : BasicCommandExecution
{
    private readonly List<IExtractionProgress> _toClear = new();

    /// <summary>
    ///     Clears one or more <see cref="ExtractionProgress" /> on a single <see cref="ExtractionConfiguration" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="config"></param>
    /// <param name="catalogue"></param>
    [UseWithObjectConstructor]
    public ExecuteCommandResetExtractionProgress(IBasicActivateItems activator,
        [DemandsInitialization("The configuration to clear progress for")]
        ExtractionConfiguration config,
        [DemandsInitialization(
            "Optional.  Pass null to clear all progress for all datasets.  Or pass a single Catalogue to reset progress for it only")]
        Catalogue catalogue) : base(activator)
    {
        foreach (var sds in config.SelectedDataSets)
            if (catalogue == null || sds.ExtractableDataSet.Catalogue_ID == catalogue.ID)
                AddClearTarget(sds);

        EvaluateIsImpossible();
    }

    public ExecuteCommandResetExtractionProgress(IBasicActivateItems activator, SelectedDataSets sds) : base(activator)
    {
        AddClearTarget(sds);

        EvaluateIsImpossible();
    }

    public ExecuteCommandResetExtractionProgress(IBasicActivateItems activator, ExtractionProgress progress) :
        base(activator)
    {
        if (progress.ProgressDate != null) _toClear.Add(progress);

        EvaluateIsImpossible();
    }

    private void EvaluateIsImpossible()
    {
        if (_toClear.Count == 0) SetImpossible("There are no ExtractionProgress underway which could be cleared");
    }

    private void AddClearTarget(ISelectedDataSets sds)
    {
        var progress = sds.ExtractionProgressIfAny;

        if (progress?.ProgressDate != null) _toClear.Add(progress);
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var progress in _toClear)
        {
            progress.ProgressDate = null;
            progress.SaveToDatabase();
        }

        var config = _toClear.FirstOrDefault()?.SelectedDataSets?.ExtractionConfiguration;
        if (config != null) Publish(config);
    }
}