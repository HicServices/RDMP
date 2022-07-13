// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    // TODO: Why isn't this just ExecuteCommandCreateNewFilter

    public class ExecuteCommandCreateNewFilterFromCatalogue : BasicCommandExecution, IAtomicCommand
    {
        private IContainer _container;
        private ExtractionFilter[] _filters;
        private IRootFilterContainerHost _host;
        private const float DEFAULT_WEIGHT = 0.2f;

        public ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, IContainer container) : this(itemActivator,container.GetCatalogueIfAny())
        {
            Weight = DEFAULT_WEIGHT;
            _container = container;

            SetImpossibleIfReadonly(container);
        }

        public ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, IRootFilterContainerHost host) : this(itemActivator, host.GetCatalogue())
        {
            Weight = DEFAULT_WEIGHT;
            _host = host;

            SetImpossibleIfReadonly(host);
        }

        private ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, ICatalogue catalogue) : base(itemActivator)
        {
            Weight = DEFAULT_WEIGHT;
            if (catalogue == null)
            {
                SetImpossible("No Catalogue found");
                return;
            }

            _filters = catalogue.GetAllFilters();

            if (!_filters.Any())
                SetImpossible("There are no Filters declared in Catalogue '" + catalogue + "'");
        }
        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Import);
        }

        public override void Execute()
        {
            base.Execute();

            if (_host != null && _container == null)
            {
                _host.CreateRootContainerIfNotExists();
                _container = _host.RootFilterContainer;
            }

            if(_container == null)
                throw new Exception("Container was null, either host failed to create or explicit null container was chosen");

            var wizard = new FilterImportWizard(BasicActivator);

            
            var import = wizard.ImportManyFromSelection(_container, _filters).ToArray();

            foreach (var f in import)
            {
                _container.AddChild(f);
            }

            if (import.Length > 0)
            {
                Publish((DatabaseEntity)_container);
                Emphasise((DatabaseEntity)import.Last());
            }
        }
    }
}