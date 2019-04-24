// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Rdmp.Core.DataExport.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUnfreezeExtractionConfiguration:BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _configuration;

        public ExecuteCommandUnfreezeExtractionConfiguration(IActivateItems activator, ExtractionConfiguration configuration):base(activator)
        {
            _configuration = configuration;

            if(!_configuration.IsReleased)
                SetImpossible("Extraction Configuration is not Frozen");
        }

        public override string GetCommandHelp()
        {
            return "Reopens a released extraction configuration and deletes all record of it ever having been released";
        }

        public override void Execute()
        {
            base.Execute();

            if(MessageBox.Show("This will mean deleting the Release Audit for the Configuration making it appear like it was never released in the first place.  If you just want to execute the Configuration again you can Clone it instead if you want.  Are you sure you want to Unfreeze?","Confirm Unfreeze",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                _configuration.Unfreeze();
                Publish(_configuration);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.UnfreezeExtractionConfiguration;
        }
    }
}
