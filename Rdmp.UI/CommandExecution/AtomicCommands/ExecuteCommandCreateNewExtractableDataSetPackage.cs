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
using ReusableUIComponents;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractableDataSetPackage:BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewExtractableDataSetPackage(IActivateItems activator) : base(activator)
        {
            if(Activator.RepositoryLocator.DataExportRepository == null)
                SetImpossible("Data export database is not setup");

            UseTripleDotSuffix = true;
        }

        public override string GetCommandHelp()
        {
            return "Creates a new grouping of dataset which are commonly extracted together e.g. 'Core datasets on offer'";
        }

        public override void Execute()
        {
            base.Execute();
            var dialog = new TypeTextOrCancelDialog("Name for package", "Name", 500);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var p = new ExtractableDataSetPackage(Activator.RepositoryLocator.DataExportRepository, dialog.ResultText);
                Publish(p);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Add);
        }
    }
}
