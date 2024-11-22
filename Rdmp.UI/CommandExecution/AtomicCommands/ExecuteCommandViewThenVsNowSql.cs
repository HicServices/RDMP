// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal class ExecuteCommandViewThenVsNowSql : BasicUICommandExecution
{
    private readonly SelectedDataSets _selectedDataSet;
    private FlatFileReleasePotential _releasePotential;

    public ExecuteCommandViewThenVsNowSql(IActivateItems activator, SelectedDataSets selectedDataSet) : base(activator)
    {
        _selectedDataSet = selectedDataSet;
    }

    public override void Execute()
    {
        base.Execute();

        var rp = new FlatFileReleasePotential(Activator.RepositoryLocator, _selectedDataSet);

        rp.Check(IgnoreAllErrorsCheckNotifier.Instance);

        if (string.IsNullOrWhiteSpace(rp.SqlCurrentConfiguration))
            Show("Could not generate Sql for dataset");
        else if (string.IsNullOrWhiteSpace(rp.SqlExtracted))
            Show("Dataset has never been extracted");
        else if (rp.SqlCurrentConfiguration == rp.SqlExtracted)
            Show("No differences");
        else
            _releasePotential = rp;

        if (_releasePotential == null)
            return;


        var dialog = new SQLBeforeAndAfterViewer(_releasePotential.SqlCurrentConfiguration,
            _releasePotential.SqlExtracted, "Current Configuration", "Configuration when last run", "Sql Executed",
            MessageBoxButtons.OK);
        dialog.Show();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.Diff);
}