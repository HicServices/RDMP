// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewFilterFromCatalogue : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IContainer _container;
        private ExtractionFilter[] _filters;

        public ExecuteCommandCreateNewFilterFromCatalogue(IActivateItems itemActivator, IContainer container):base(itemActivator)
        {
            _container = container;
            var catalogue = container.GetCatalogueIfAny();

            if(catalogue == null)
            {
                SetImpossible("No Catalogue found for filter container:" + container);
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

            var wizard = new FilterImportWizard();
            var import = wizard.ImportOneFromSelection(_container, _filters);

            if (import != null)
            {
                _container.AddChild(import);
                Publish((DatabaseEntity) import);
                Activate((DatabaseEntity)import);
            }
        }
    }
}