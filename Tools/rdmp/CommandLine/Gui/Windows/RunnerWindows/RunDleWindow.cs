// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.CommandLine.Gui.Windows.RunnerWindows;

internal class RunDleWindow : RunEngineWindow<DleOptions>
{
    private readonly LoadMetadata lmd;

    public RunDleWindow(IBasicActivateItems activator, LoadMetadata lmd)
        : base(activator, () => GetCommand(lmd))
    {
        this.lmd = lmd;
    }

    private static DleOptions GetCommand(LoadMetadata lmd) =>
        new()
        {
            LoadMetadata = lmd.ID.ToString(),
            Iterative = false
        };

    protected override void AdjustCommand(DleOptions opts, CommandLineActivity activity)
    {
        base.AdjustCommand(opts, activity);

        if (lmd.LoadProgresses.Any() && activity == CommandLineActivity.run)
        {
            var lp = (LoadProgress)BasicActivator.SelectOne("Load Progress", lmd.LoadProgresses, null, true);
            if (lp == null)
                return;

            opts.LoadProgress = lp.ID.ToString();

            if (BasicActivator.SelectValueType("Days to Load", typeof(int), lp.DefaultNumberOfDaysToLoadEachTime,
                    out var chosen))
                opts.DaysToLoad = (int)chosen;
            else
                return;
        }
    }
}