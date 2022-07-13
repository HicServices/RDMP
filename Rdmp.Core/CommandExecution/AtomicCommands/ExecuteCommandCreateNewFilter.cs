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
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewFilter : BasicCommandExecution,IAtomicCommand
    {
        private IFilterFactory _factory;
        private IContainer _container;
        private IRootFilterContainerHost _host;
        private const float DEFAULT_WEIGHT = 0.1f;

        public IFilter BasedOn { get; private set; }
        public ExtractionFilterParameterSet ParameterSet { get; private set; }

        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, 
            IRootFilterContainerHost host,
            [DemandsInitialization("Optional. A filter to clone as the new filter instead of new being blank")]
            IFilter basedOn = null,
            [DemandsInitialization("Optional. Parameter set to populate parameter values in the new filter instead of being blank.  Requires basedOn to be populated")]
            string valueSetName = null):base(activator)
        {
            Weight = DEFAULT_WEIGHT;

            _factory = host.GetFilterFactory();
            _container = host.RootFilterContainer;
            _host = host;
            
            SetBasedOn(host.GetCatalogue(),basedOn,valueSetName);
            
            if (IsImpossible)
                return;

            if (_container == null && _host is AggregateConfiguration ac)
            {
                if (ac.Catalogue.IsApiCall())
                    SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);

                if(ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                   SetImpossible("Aggregate is set to use another's filter container tree");
            }

            SetImpossibleIfReadonly(host);
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

            SetImpossibleIfReadonly(container);
        }
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IContainer container, IFilter basedOn) : base(activator)
        {
            Weight = DEFAULT_WEIGHT;

            _container = container;
            BasedOn = basedOn;

            SetBasedOn(container.GetCatalogueIfAny(), basedOn, null);

            SetImpossibleIfReadonly(container);
        }

        private void SetBasedOn(ICatalogue targetCatalogue, IFilter basedOn, string valueSetName)
        {
            BasedOn = basedOn;

            if (BasedOn != null)
            {
                //if source catalogue is known
                var sourceCatalogue = BasedOn.GetCatalogue();
                if (sourceCatalogue != null)
                {
                    if (targetCatalogue != null && !sourceCatalogue.Equals(targetCatalogue))
                    {
                        SetImpossible("Cannot Import Filter from '" + sourceCatalogue + "' into '" + targetCatalogue + "'");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(valueSetName))
                {
                    if (BasedOn is not ExtractionFilter f)
                    {
                        SetImpossible($"Cannot specify parameter value sets when filter is not a '{nameof(ExtractionFilter)}'.  Filter was a '{BasedOn.GetType().Name}'");
                        return;
                    }

                    var match = f.ExtractionFilterParameterSets.FirstOrDefault(s => s.Name.Equals(valueSetName));
                    if (match == null)
                    {
                        SetImpossible($"Could not find a value set called '{valueSetName}'. Declared value sets are:{Environment.NewLine}{string.Join(Environment.NewLine, f.ExtractionFilterParameterSets.Select(p => p.Name).ToArray())}");
                        return;
                    }

                    ParameterSet = match;
                }

            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            IFilter f;
            IContainer container = _container;

            if (_host != null && container == null)
            {
                if (_host.RootFilterContainer_ID == null)
                    _host.CreateRootContainerIfNotExists();

                container = _host.RootFilterContainer;
            }

            // if importing an existing filter instead of creating blank
            if (BasedOn != null)
            {
                var wizard = new FilterImportWizard(BasicActivator);
                f = wizard.Import(container, BasedOn, ParameterSet);
            }
            else
            {
                f = _factory.CreateNewFilter("New Filter " + Guid.NewGuid());
            }

            if (container != null)
                container.AddChild(f);

            if (f is ExtractionFilter ef)
                Publish(ef.ExtractionInformation);
            else
                Publish((DatabaseEntity) container ?? (DatabaseEntity)f);

            Emphasise(f);
            Activate((DatabaseEntity)f);
        }
    }
}