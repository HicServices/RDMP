// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ANOEngineeringUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateANOVersion:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        public ExecuteCommandCreateANOVersion(IActivateItems activator,Catalogue catalogue) : base(activator)
        {
            _catalogue = catalogue;
        }

        public ExecuteCommandCreateANOVersion(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ANOTable);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override string GetCommandHelp()
        {
            return "Create an anonymous version of the dataset.  This will be an initially empty anonymous schema and a load configuration for migrating the data.";
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
                _catalogue = SelectOne<Catalogue>(Activator.CoreChildProvider.AllCatalogues);

            if(_catalogue == null)
                return;

            Activator.Activate<ForwardEngineerANOCatalogueUI, Catalogue>(_catalogue);
        }
    }
}
