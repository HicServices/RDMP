// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Menus
{
    public abstract class ContainerMenu : RDMPContextMenuStrip
    {
        private readonly IContainer _container;
        
        protected ContainerMenu(IFilterFactory factory,RDMPContextMenuStripArgs args, IContainer container) : base(args, container)
        {
            _container = container;

            string operationTarget = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";

            Items.Add("Set Operation to " + operationTarget, null, (s, e) => FlipContainerOperation());

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewFilter(args.ItemActivator,factory,_container));
            Add(new ExecuteCommandCreateNewFilterFromCatalogue(args.ItemActivator, container));
            
            Items.Add(new ToolStripSeparator());
            Items.Add("Add SubContainer", _activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddSubcontainer());

        }
        private void FlipContainerOperation()
        {
            _container.Operation = _container.Operation == FilterContainerOperation.AND
                ? FilterContainerOperation.OR
                : FilterContainerOperation.AND;

            _container.SaveToDatabase();
            Publish((DatabaseEntity)_container);
        }

        private void AddSubcontainer()
        {
            var newContainer = GetNewFilterContainer();
            _container.AddChild(newContainer);
            Publish((DatabaseEntity)_container);
        }

        protected abstract IContainer GetNewFilterContainer();
    }
}