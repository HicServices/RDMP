// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.ForwardEngineering;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewCatalogueFromTableInfo : BasicUICommandExecution,IAtomicCommand
    {
        private TableInfo _tableInfo;

        public ExecuteCommandCreateNewCatalogueFromTableInfo(IActivateItems activator, TableInfo tableInfo) : base(activator)
        {
            _tableInfo = tableInfo;

            if(activator.CoreChildProvider.AllCatalogues.Any(c=>c.Name.Equals(tableInfo.GetRuntimeName())))
                SetImpossible("There is already a Catalogue called '" + tableInfo.GetRuntimeName() + "'");
        }


        public override void Execute()
        {
            base.Execute();

            var ui = new ConfigureCatalogueExtractabilityUI(Activator, _tableInfo, "Existing Table", null);
            ui.ShowDialog();
            var cata = ui.CatalogueCreatedIfAny;

            if (cata != null)
            {
                Publish(cata);
                Emphasise(cata);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Shortcut);
        }
    }
}