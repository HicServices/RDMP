// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewFilter : BasicCommandExecution, IAtomicCommand
{
    private IFilterFactory _factory;
    private IContainer _container;
    private IRootFilterContainerHost _host;
    private const float DEFAULT_WEIGHT = 0.1f;

    public IFilter BasedOn { get; set; }
    public ExtractionFilterParameterSet ParameterSet { get; set; }
    public string Name { get; }
    public string WhereSQL { get; }

    private IFilter[] _offerFilters = [];
    private bool offerCatalogueFilters;

    public bool OfferCatalogueFilters
    {
        get => offerCatalogueFilters;
        set
        {
            offerCatalogueFilters = value;
        }
    }

    private ExecuteCommandCreateNewFilter(IBasicActivateItems activator) : base(activator)
    {
        Weight = DEFAULT_WEIGHT;
    }

    [UseWithCommandLine(
        ParameterHelpList = "<into> <basedOn> <name> <where>",
        ParameterHelpBreakdown =
            @"into	A WHERE filter container or IRootFilterContainerHost (e.g. AggregateConfiguration)
basedOn    Optional ExtractionFilter to copy or ExtractionFilterParameterSet
name    Optional name to set for the new filter
where    Optional SQL to set for the filter.  If <basedOn> is not null this will overwrite it")]
    public ExecuteCommandCreateNewFilter(IBasicActivateItems activator,
        CommandLineObjectPicker picker) : this(activator)
    {
        if (picker.Length == 0)
            throw new ArgumentException(
                "You must supply at least one argument to this command (where you want to create the filter)");

        if (picker.Length > 0)
        {
            if (picker[0].HasValueOfType(typeof(ExtractionInformation)))
            {
                // create a top level Catalogue level filter for reuse later on
                var ei = (ExtractionInformation)picker[0].GetValueForParameterOfType(typeof(ExtractionInformation));
                _factory = new ExtractionFilterFactory(ei);
            }
            else if (picker[0].HasValueOfType(typeof(IContainer)))
            {
                // create a filter in this container
                _container = (IContainer)picker[0].GetValueForParameterOfType(typeof(IContainer));
                SetImpossibleIfReadonly(_container);
            }
            else if (picker[0].HasValueOfType(typeof(IRootFilterContainerHost)))
            {
                // create a container (if none) then add filter to root container of the object
                _host = (IRootFilterContainerHost)picker[0]
                    .GetValueForParameterOfType(typeof(IRootFilterContainerHost));
                SetImpossibleIfReadonly(_host);
            }
            else
            {
                throw new ArgumentException(
                    $"First argument must be {nameof(IContainer)} or  {nameof(IRootFilterContainerHost)} but it was '{picker[0].RawValue}'");
            }


            _factory ??= _container?.GetFilterFactory() ?? _host?.GetFilterFactory();

            if (_factory == null)
                throw new Exception("It was not possible to work out a FilterFactory from the container/host");
        }

        // the index that string arguments begin at (Name and WhereSql)
        var stringArgsStartAt = 2;

        if (picker.Length > 1)
        {
            if (IsImpossible)
                return;

            if (picker[1].HasValueOfType(typeof(IFilter)))
                BasedOn = (IFilter)picker[1].GetValueForParameterOfType(typeof(IFilter));
            else if (picker[1].HasValueOfType(typeof(ExtractionFilterParameterSet)))
                ParameterSet =
                    (ExtractionFilterParameterSet)picker[1]
                        .GetValueForParameterOfType(typeof(ExtractionFilterParameterSet));
            else if (!picker[1].ExplicitNull) stringArgsStartAt = 1;
        }

        if (picker.Length > stringArgsStartAt) Name = picker[stringArgsStartAt].RawValue;
        if (picker.Length > stringArgsStartAt + 1) WhereSQL = picker[stringArgsStartAt + 1].RawValue;
    }


    public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IRootFilterContainerHost host) : this(activator)
    {
        _factory = host.GetFilterFactory();
        _container = host.RootFilterContainer;
        _host = host;
        if (_container == null && _host is AggregateConfiguration ac)
        {
            if (ac.Catalogue.IsApiCall())
                SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);

            if (ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                SetImpossible("Aggregate is set to use another's filter container tree");
        }

        SetImpossibleIfReadonly(host);
    }


    public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, CatalogueItem ci) : this(activator)
    {
        if (ci.ExtractionInformation == null)
        {
            SetImpossible(
                "CatalogueItem is not extractable so cannot have filters. Make this CatalogueItem extractable to add filters.");
            return;
        }

        _factory = new ExtractionFilterFactory(ci.ExtractionInformation);
    }

    public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IFilterFactory factory,
        IContainer container = null)
        : this(activator)
    {
        _factory = factory;
        _container = container;

        SetImpossibleIfReadonly(container);
    }

    public ExecuteCommandCreateNewFilter(IBasicActivateItems activator, IContainer container, IFilter basedOn) :
        this(activator)
    {
        _container = container;
        BasedOn = basedOn;

        SetImpossibleIfReadonly(container);
    }


    private ICatalogue GetCatalogue() => _host?.GetCatalogue() ?? _container?.GetCatalogueIfAny();

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        OfferCatalogueFilters
            ? iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Import)
            : iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);

    public override void Execute()
    {
        base.Execute();

        IFilter f;
        var container = _container;

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
        else if (OfferCatalogueFilters)
        {

            var c = GetCatalogue();
            _offerFilters = c?.GetAllFilters();
            if (_host is SelectedDataSets sds)
            {
                var cohortId = sds.ExtractionConfiguration.Cohort.OriginID;
                var cic = c.CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(cohortId);
                if (cic != null)
                {
                    var filters = cic.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively().SelectMany(ac => ac.RootFilterContainer.GetAllFiltersIncludingInSubContainersRecursively());
                    filters = filters.Where(f => f.GetCatalogue().ID == c.ID);
                    _offerFilters = _offerFilters.Concat(filters).ToArray();
                }
            }
            if (_offerFilters == null || !_offerFilters.Any())
                SetImpossible($"There are no Filters declared in Catalogue '{c?.ToString() ?? "NULL"}'");

            // we want user to make decision about what to import
            ImportExistingFilter(container);
            return;
        }
        else
        {
            f = _factory.CreateNewFilter($"New Filter {Guid.NewGuid()}");
        }

        container?.AddChild(f);

        if (!string.IsNullOrWhiteSpace(Name)) f.Name = Name;
        if (!string.IsNullOrWhiteSpace(WhereSQL)) f.WhereSQL = WhereSQL;

        f.SaveToDatabase();

        if (f is ExtractionFilter ef)
            Publish(ef.ExtractionInformation);
        else
            Publish((DatabaseEntity)container ?? (DatabaseEntity)f);

        Emphasise(f);
        Activate((DatabaseEntity)f);
    }

    private void ImportExistingFilter(IContainer container)
    {
        var wizard = new FilterImportWizard(BasicActivator);

        var filters = _offerFilters;
        var import = wizard.ImportManyFromSelection(container, filters).ToArray();

        foreach (var f in import) container.AddChild(f);

        if (import.Length > 0)
        {
            Publish((DatabaseEntity)container);
            Emphasise((DatabaseEntity)import.Last());
        }
    }
}