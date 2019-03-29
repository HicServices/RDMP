// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowRelatedObject : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private ObjectImport _import;
        private ObjectExport _export;
        private string _commandName;

        public ExecuteCommandShowRelatedObject(IActivateItems activator, ObjectImport node) : base(activator)
        {
            _import = node;
            _export = null;
            _commandName = "View the object that relates to this import definition";
        }

        public ExecuteCommandShowRelatedObject(IActivateItems activator, ObjectExport node) : base(activator)
        {
            _export = node;
            _import = null;
            _commandName = "View the object that relates to this export definition";
        }

        public override string GetCommandHelp()
        {
            return _commandName;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AllObjectSharingNode);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            return this;
        }

        public override void Execute()
        {
            if (_import != null)
                Emphasise((DatabaseEntity)_import.GetReferencedObject(Activator.RepositoryLocator));
            
            if (_export != null)
                Emphasise((DatabaseEntity) _export.GetReferencedObject(Activator.RepositoryLocator));
        }
    }
}