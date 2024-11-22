// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Linq;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandShowCacheFetchFailures : BasicUICommandExecution
{
    private readonly CacheProgress _cacheProgress;
    private readonly ICacheFetchFailure[] _failures;

    public ExecuteCommandShowCacheFetchFailures(IActivateItems activator, CacheProgress cacheProgress) : base(activator)
    {
        _cacheProgress = cacheProgress;

        _failures = _cacheProgress.CacheFetchFailures.Where(static f => f.ResolvedOn == null).ToArray();

        if (_failures.Length == 0)
            SetImpossible("There are no unresolved CacheFetchFailures");
    }

    public override void Execute()
    {
        base.Execute();

        // for now just show a modal dialog with a data grid view of all the failure rows

        var dt = new DataTable();
        dt.BeginLoadData();
        dt.Columns.Add("FetchRequestStart");
        dt.Columns.Add("FetchRequestEnd");
        dt.Columns.Add("ExceptionText");
        dt.Columns.Add("LastAttempt");
        dt.Columns.Add("ResolvedOn");

        foreach (var f in _failures)
            dt.Rows.Add(f.FetchRequestStart, f.FetchRequestEnd, f.ExceptionText, f.LastAttempt, f.ResolvedOn);
        dt.EndLoadData();

        var ui = new DataTableViewerUI(dt, "Cache Failures");
        Activator.ShowWindow(ui, true);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(_cacheProgress);
}