// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandLine.Interactive.Picking;
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

        public IFilter BasedOn { get; set; }
        public ExtractionFilterParameterSet ParameterSet { get; set; }
        public string Name { get; }
        public string WhereSQL { get; }

        private ExecuteCommandCreateNewFilter(IBasicActivateItems activator) : base(activator)
        {

            Weight = DEFAULT_WEIGHT;
        }

        [UseWithCommandLine(
            ParameterHelpList = "<into> <basedOn> <name> <where>", 
            ParameterHelpBreakdown = @"into	A WHERE filter container or IRootFilterContainerHost (e.g. AggregateConfiguration)
basedOn    ExtractionFilter to copy or ExtractionFilterParameterSet
name    Optional name to set for the new filter
where    Optional SQL to set for the filter.  If <basedOn> is not null this will overwrite it")]
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, 
            CommandLineObjectPicker picker):this(activator)
        {
            if(picker.Length == 0)
                throw new ArgumentException("You must supply at least one argument to this command (where you want to create the filter)");

            if(picker.Length > 0)
            {
                if(picker[0].HasValueOfType(typeof(IContainer)))
                {
                    _container = (IContainer)picker[0].GetValueForParameterOfType(typeof(IContainer));
                    SetImpossibleIfReadonly(_container);
                }
                else
                if(picker[0].HasValueOfType(typeof(IRootFilterContainerHost)))
                {
                    _host = (IRootFilterContainerHost)picker[0].GetValueForParameterOfType(typeof(IRootFilterContainerHost));
                    SetImpossibleIfReadonly(_host);
                }
                else
                {
                    throw new ArgumentException($"First argument must be {nameof(IContainer)} or  {nameof(IRootFilterContainerHost)} but it was '{picker[0].RawValue}'");
                }


                _factory = _container?.GetFilterFactory() ?? _host?.GetFilterFactory();

                if(_factory == null)
                    throw new Exception("It was not possible to work out a FilterFactory from the container/host");

            }


            if(picker.Length > 1)
            {
                if (IsImpossible)
                    return;

                if(picker[1].HasValueOfType(typeof(IFilter)))
                {
                    BasedOn = (IFilter)picker[1].GetValueForParameterOfType(typeof(IFilter));
                }
                else
                if(picker[1].HasValueOfType(typeof(ExtractionFilterParameterSet)))
                {
                    ParameterSet = (ExtractionFilterParameterSet)picker[1].GetValueForParameterOfType(typeof(ExtractionFilterParameterSet));
                }
                else if (!picker[1].ExplicitNull)
                {
                    throw new ArgumentException($"Second argument must be {nameof(IFilter)} or  {nameof(ExtractionFilterParameterSet)} or null but it was '{picker[1].RawValue}'");
                }
            }
            if(picker.Length > 2)
            {
                Name = picker[2].RawValue;
            }
            if(picker.Length > 3)
            {
                WhereSQL = picker[3].RawValue;
            }
        }


        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IRootFilterContainerHost host):this(activator)
        {

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

            SetImpossibleIfReadonly(host);
        }

        
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, CatalogueItem ci) : this(activator)
        {

            if(ci.ExtractionInformation == null)
            {
                SetImpossible("CatalogueItem is not extractable so cannot have filters. Make this CatalogueItem extractable to add filters.");
                return;
            }

            _factory = new ExtractionFilterFactory(ci.ExtractionInformation);
        }
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IFilterFactory factory, IContainer container = null)
            :this(activator)
        {

            _factory = factory;
            _container = container;

            SetImpossibleIfReadonly(container);
        }
        public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IContainer container, IFilter basedOn) : this(activator)
        {
            _container = container;
            BasedOn = basedOn;

            SetImpossibleIfReadonly(container);
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

            if(!string.IsNullOrWhiteSpace(Name))
            {
                f.Name = Name;
            }
            if(!string.IsNullOrWhiteSpace(WhereSQL))
            {
                f.WhereSQL = WhereSQL;
            }

            f.SaveToDatabase();

            if (f is ExtractionFilter ef)
                Publish(ef.ExtractionInformation);
            else
                Publish((DatabaseEntity) container ?? (DatabaseEntity)f);

            Emphasise(f);
            Activate((DatabaseEntity)f);
        }
    }
}