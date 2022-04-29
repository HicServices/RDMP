// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewFilter : BasicCommandExecution,IAtomicCommand
    {
        private IFilterFactory _factory;
        private IContainer _container;
        private IRootFilterContainerHost _host;
        private const float DEFAULT_WEIGHT = 0.1f;

        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IRootFilterContainerHost host):base(activator)
        {
            Weight = DEFAULT_WEIGHT;

            _factory = host.GetFilterFactory();
            _container = host.RootFilterContainer;
            _host = host;



            if (_container == null && _host is AggregateConfiguration ac)
            {
                if (ac.Catalogue.IsApiCall())
                    SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);

                if(ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                   SetImpossible("Aggregate is set to use another's filter container tree");
            }
        }

        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, CatalogueItem ci) : base(activator)
        {
            Weight = DEFAULT_WEIGHT;

            if(ci.ExtractionInformation == null)
            {
                SetImpossible("CatalogueItem is not extractable so cannot have filters. Make this CatalogueItem extractable to add filters.");
                return;
            }

            _factory = new ExtractionFilterFactory(ci.ExtractionInformation);
        }
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IFilterFactory factory, IContainer container = null):base(activator)
        {
            Weight = DEFAULT_WEIGHT;

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

            if(_host != null && _container == null)
            {
                if(_host.RootFilterContainer_ID == null)
                    _host.CreateRootContainerIfNotExists();
                
                _container = _host.RootFilterContainer;       
            }

            if (_container != null)
                _container.AddChild((IFilter)f);

            if (f is ExtractionFilter ef)
                Publish(ef.ExtractionInformation);
            else
                Publish((DatabaseEntity) _container ?? f);

            Emphasise(f);
            Activate(f);
        }
    }
}