// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs.SubComponents;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByImportingExistingDataTable:BasicUICommandExecution,IAtomicCommand
    {
        private readonly bool _allowImportAsCatalogue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="allowImportAsCatalogue">true to automatically create the catalogue without showing the UI</param>
        public ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(IActivateItems activator,bool allowImportAsCatalogue=true) : base(activator)
        {
            this._allowImportAsCatalogue = allowImportAsCatalogue;
            UseTripleDotSuffix = true;
        }

        public override void Execute()
        {
            base.Execute();

            var importTable = new ImportSQLTableUI(Activator,_allowImportAsCatalogue);
            importTable.ShowDialog();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Import);
        }

        public override string GetCommandHelp()
        {
            return "Creates a New Catalogue by associating it " +
                   "\r\n" +
                   "with an existing Dataset Table in your database";
        }

        public override string GetCommandName()
        {
            if (!_allowImportAsCatalogue)
                return "Create New TableInfo By Importing Existing Data Table...";

            return base.GetCommandName();
        }
    }
}
