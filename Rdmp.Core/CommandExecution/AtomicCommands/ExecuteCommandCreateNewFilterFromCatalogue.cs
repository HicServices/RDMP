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
    public class ExecuteCommandCreateNewFilterFromCatalogue : BasicCommandExecution, IAtomicCommand
    {
        private IContainer _container;
        private ExtractionFilter[] _filters;
        private IRootFilterContainerHost _host;

        public ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, IContainer container) : this(itemActivator,container.GetCatalogueIfAny())
        {
            _container = container;
        }

        public ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, IRootFilterContainerHost host) : this(itemActivator, host.GetCatalogue())
        {
            _host = host;
        }

        private ExecuteCommandCreateNewFilterFromCatalogue(IBasicActivateItems itemActivator, ICatalogue catalogue) : base(itemActivator)
        {
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
            var import = wizard.ImportOneFromSelection(_container, _filters);

            if (import != null)
            {
                _container.AddChild(import);
                Publish((DatabaseEntity)import);
                Emphasise((DatabaseEntity)import);
            }
        }
    }
}