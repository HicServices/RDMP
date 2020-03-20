// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewFilterFromCatalogue : BasicUICommandExecution, IAtomicCommand
    {
        private readonly Func<IContainer> _containerFunc;
        private IContainer _container;
        private ExtractionFilter[] _filters;

        public ExecuteCommandCreateNewFilterFromCatalogue(IActivateItems itemActivator, IContainer container):this(itemActivator,container.GetCatalogueIfAny())
        {
            _container = container;
        }

        public ExecuteCommandCreateNewFilterFromCatalogue(IActivateItems itemActivator,ICatalogue catalogue, Func<IContainer> containerFunc):this(itemActivator,catalogue)
        {
            _containerFunc = containerFunc;
        }

        private ExecuteCommandCreateNewFilterFromCatalogue(IActivateItems itemActivator, ICatalogue catalogue):base(itemActivator)
        {
            if(catalogue == null)
            {
                SetImpossible("No Catalogue found");
                return;
            }

            _filters = catalogue.GetAllFilters();

            if(!_filters.Any())
                SetImpossible("There are Filters declard in Catalogue '" + catalogue +"'");
        }
        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            if (_containerFunc != null)
                _container = _containerFunc();

            var wizard = new FilterImportWizard();
            var import = wizard.ImportOneFromSelection(_container, _filters);

            if (import != null)
            {
                _container.AddChild(import);
                Publish((DatabaseEntity) import);
                Emphasise((DatabaseEntity) import);
                Activate((DatabaseEntity)import);
            }
        }
    }
}