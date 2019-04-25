// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.DataExport.Data.LinkCreators;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.SqlDialogs;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandViewThenVsNowSql : BasicUICommandExecution, IAtomicCommand
    {
        private FlatFileReleasePotential _releasePotential;

        public ExecuteCommandViewThenVsNowSql(IActivateItems activator, SelectedDataSets selectedDataSet):base(activator)
        {
            try
            {
                var rp = new FlatFileReleasePotential(Activator.RepositoryLocator, selectedDataSet);

                rp.Check(new IgnoreAllErrorsCheckNotifier());

                if (string.IsNullOrWhiteSpace(rp.SqlCurrentConfiguration))
                    SetImpossible("Could not generate Sql for dataset");
                else
                if(string.IsNullOrWhiteSpace(rp.SqlExtracted))
                    SetImpossible("Dataset has never been extracted");
                else
                if(rp.SqlCurrentConfiguration == rp.SqlExtracted)
                    SetImpossible("No differences");

                _releasePotential = rp;
            }
            catch (Exception)
            {
                SetImpossible("Could not make assesment");
            }
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new SQLBeforeAndAfterViewer(_releasePotential.SqlCurrentConfiguration, _releasePotential.SqlExtracted, "Current Configuration", "Configuration when last run", "Sql Executed", MessageBoxButtons.OK);
            dialog.Show();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Diff);
        }
    }
}