// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.FilterImporting.Construction;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewFilter : BasicUICommandExecution,IAtomicCommand
    {
        private IFilterFactory _factory;
        private readonly IContainer _container;

        public ExecuteCommandCreateNewFilter(IActivateItems activator, IFilterFactory factory, IContainer container = null):base(activator)
        {
            _factory = factory;
            _container = container;
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var f = (DatabaseEntity)_factory.CreateNewFilter("New Filter " + Guid.NewGuid());

            if (_container != null)
                _container.AddChild((IFilter)f);
            
            Publish(f);
            Activate(f);
        }
    }
}