// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Imports the entire tree from another <see cref="ISelectedDataSets" /> or <see cref="AggregateConfiguration" /> into
///     a given <see cref="SelectedDataSets" /> (as new copies)
/// </summary>
public class ExecuteCommandImportFilterContainerTree : BasicCommandExecution
{
    /// <summary>
    ///     ID of the Catalogue that is being extracted by <see cref="_into" /> to ensure that we only import filters from the
    ///     same table
    /// </summary>
    private readonly ICatalogue _catalogue;

    private readonly IRootFilterContainerHost _into;
    private const float DEFAULT_WEIGHT = 1.2f;

    /// <summary>
    ///     May be null, if populated this is the explicit subcontainer into which the tree should be imported i.e. not
    ///     <see cref="_into" />
    /// </summary>
    private readonly IContainer _intoSubContainer;

    /// <summary>
    ///     May be null, if populated then this is the explicit one the user wants and we shouldn't ask them again
    /// </summary>
    private readonly IContainer _explicitChoice;

    private ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator) : base(activator)
    {
        Weight = DEFAULT_WEIGHT;

        if (activator.CoreChildProvider is not DataExportChildProvider)
            SetImpossible("Data export functions unavailable");
    }

    public ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator, IRootFilterContainerHost into) :
        this(activator)
    {
        Weight = DEFAULT_WEIGHT;

        _into = into;

        if (into.RootFilterContainer_ID != null)
            SetImpossible("Dataset already has a root container");


        if (into is AggregateConfiguration ac && ac.Catalogue.IsApiCall())
            SetImpossible(ExecuteCommandAddNewFilterContainer.FiltersCannotBeAddedToApiCalls);

        _catalogue = _into.GetCatalogue();

        SetImpossibleIfReadonly(into);
    }

    /// <summary>
    ///     constructor for explicit choices, use this aggregates root container
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="into"></param>
    /// <param name="from"></param>
    [UseWithObjectConstructor]
    public ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator, IRootFilterContainerHost into,
        IRootFilterContainerHost from) : this(activator, into)
    {
        Weight = DEFAULT_WEIGHT;

        if (from.RootFilterContainer_ID == null)
            SetImpossible("AggregateConfiguration has no root container");
        else
            _explicitChoice = from.RootFilterContainer;
    }

    /// <summary>
    ///     Constructor for explicitly specifying the container to import
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="into"></param>
    /// <param name="explicitChoice"></param>
    public ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator, IRootFilterContainerHost into,
        IContainer explicitChoice) : this(activator, into)
    {
        Weight = DEFAULT_WEIGHT;

        _explicitChoice = explicitChoice;
    }

    /// <summary>
    ///     Constructor for importing into a sub container
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="into"></param>
    /// <param name="explicitChoice"></param>
    public ExecuteCommandImportFilterContainerTree(IBasicActivateItems activator, IContainer into,
        IContainer explicitChoice) : this(activator)
    {
        Weight = DEFAULT_WEIGHT;

        _intoSubContainer = into;
        _explicitChoice = explicitChoice;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Import);
    }

    public override void Execute()
    {
        base.Execute();

        if (_explicitChoice != null)
        {
            Import(_explicitChoice);
        }
        else
        {
            if (_into == null)
                throw new NotSupportedException(
                    "Interactive mode is only supported when specifying a root object to import into");

            //prompt user to pick one
            var childProvider = (DataExportChildProvider)BasicActivator.CoreChildProvider;

            var ecById = childProvider.ExtractionConfigurations.ToDictionary(k => k.ID);

            // The root object that makes most sense to the user e.g. they select an extraction
            var fromConfiguration
                =
                childProvider.AllCohortIdentificationConfigurations.Where(IsEligible)
                    .Cast<DatabaseEntity>()
                    .Union(childProvider.SelectedDataSets.Where(IsEligible)
                        .Select(sds => ecById[sds.ExtractionConfiguration_ID])).ToList();

            if (!fromConfiguration.Any())
            {
                Show("There are no extractions or cohort builder configurations of this dataset that use filters");
                return;
            }

            if (SelectOne(fromConfiguration, out var selected))
            {
                if (selected is ExtractionConfiguration ec) Import(GetEligibleChild(ec).RootFilterContainer);
                if (selected is CohortIdentificationConfiguration cic)
                {
                    var chosen = SelectOne(GetEligibleChildren(cic).ToList(), null, true);

                    if (chosen != null)
                        Import(chosen.RootFilterContainer);
                }
            }
        }

        Publish((DatabaseEntity)_into ?? (DatabaseEntity)_intoSubContainer);
    }

    private void Import(IContainer from)
    {
        var factory =
            _into != null ? _into.GetFilterFactory() : _intoSubContainer.GetFilterFactory();

        IContainer intoContainer;

        if (_into != null)
        {
            var newRoot = factory.CreateNewContainer();
            newRoot.Operation = from.Operation;
            newRoot.SaveToDatabase();
            _into.RootFilterContainer_ID = newRoot.ID;
            _into.SaveToDatabase();

            intoContainer = newRoot;
        }
        else
        {
            intoContainer = _intoSubContainer;
        }

        DeepClone(intoContainer, from, factory);
    }

    private void DeepClone(IContainer into, IContainer from, IFilterFactory factory)
    {
        //clone the subcontainers
        foreach (var container in from.GetSubContainers())
        {
            var subContainer = factory.CreateNewContainer();
            subContainer.Operation = container.Operation;
            subContainer.SaveToDatabase();
            into.AddChild(subContainer);

            DeepClone(subContainer, container, factory);
        }

        var wizard = new FilterImportWizard(BasicActivator);

        //clone the filters
        foreach (var filter in from.GetFilters())
            into.AddChild(wizard.Import(into, filter));
    }

    private bool IsEligible(CohortIdentificationConfiguration arg)
    {
        return GetEligibleChildren(arg).Any();
    }


    /// <summary>
    ///     Returns all <see cref="AggregateConfiguration" /> from the <paramref name="arg" /> where the dataset is the same
    ///     and there are filters defined
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private IEnumerable<AggregateConfiguration> GetEligibleChildren(CohortIdentificationConfiguration arg)
    {
        return arg.RootCohortAggregateContainer_ID == null
            ? Array.Empty<AggregateConfiguration>()
            : arg.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                .Where(ac => ac.Catalogue_ID == _catalogue.ID && ac.RootFilterContainer_ID != null);
    }

    /// <summary>
    ///     Returns the <see cref="ISelectedDataSets" /> that matches the dataset <see cref="_into" /> if it is one of the
    ///     datasets in the <see cref="ExtractionConfiguration" /> <paramref name="arg" /> (each dataset can only be extracted
    ///     once in a given <see cref="ExtractionConfiguration" />)
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private ISelectedDataSets GetEligibleChild(ExtractionConfiguration arg)
    {
        return arg.SelectedDataSets.FirstOrDefault(IsEligible);
    }

    private bool IsEligible(ISelectedDataSets arg)
    {
        return arg.RootFilterContainer_ID != null;
    }
}