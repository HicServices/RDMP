// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
/// All AggregateFilters must be contained within an AggregateFilterContainer at Query Generation time.  This tells QueryBuilder how to use brackets and whether to AND / OR
/// the various filter lines.  The AggregateFilterContainer serves the same purpose as the FilterContainer in Data Export Manager but for AggregateConfigurations (GROUP BY queries)
/// 
/// <para>FilterContainers are fully hierarchical and must be fetched from the database via recursion from the SubContainer table (AggregateFilterSubContainer).
/// The class deals with all this transparently via GetSubContainers.</para>
/// </summary>
public class AggregateFilterContainer : ConcreteContainer, IDisableable
{
    #region Database Properties

    private bool _isDisabled;


    /// <inheritdoc/>
    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }

    #endregion

    public AggregateFilterContainer()
    {
    }

    /// <summary>
    /// Creates a new IContainer in the dtabase for use with an <see cref="AggregateConfiguration"/>
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="operation"></param>
    public AggregateFilterContainer(ICatalogueRepository repository, FilterContainerOperation operation) : base(
        repository.FilterManager)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object> { { "Operation", operation.ToString() } });
    }


    internal AggregateFilterContainer(ICatalogueRepository repository, DbDataReader r) : base(repository.FilterManager,
        repository, r)
    {
        IsDisabled = Convert.ToBoolean(r["IsDisabled"]);
    }

    /// <inheritdoc/>
    public override string ToString() => Operation.ToString();


    /// <inheritdoc/>
    public override Catalogue GetCatalogueIfAny()
    {
        var agg = GetAggregate();
        return agg?.Catalogue;
    }

    /// <summary>
    /// Returns true if the filter container belongs to a parent <see cref="CohortIdentificationConfiguration"/> that is frozen
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public override bool ShouldBeReadOnly(out string reason)
    {
        var cic = GetAggregate()?.GetCohortIdentificationConfigurationIfAny();
        if (cic == null)
        {
            reason = null;
            return false;
        }

        return cic.ShouldBeReadOnly(out reason);
    }

    /// <summary>
    /// Creates a copy of the current AggregateFilterContainer including new copies of all subcontainers, filters (including those in subcontainers) and parameters of those
    /// filters.  This is a recursive operation that will clone the entire tree no matter how deep.
    /// </summary>
    /// <returns></returns>
    public override IContainer DeepCloneEntireTreeRecursivelyIncludingFilters()
    {
        //clone ourselves
        var clone = ShallowClone();

        //clone our filters
        foreach (AggregateFilter filterToClone in GetFilters())
        {
            //clone it
            var cloneFilter = filterToClone.ShallowClone(clone);

            //clone parameters
            foreach (AggregateFilterParameter parameterToClone in filterToClone.GetAllParameters())
                parameterToClone.ShallowClone(cloneFilter);
        }

        //now clone all subcontainers
        foreach (AggregateFilterContainer toCloneSubcontainer in GetSubContainers())
        {
            //clone the subcontainer recursively
            var clonedSubcontainer =
                toCloneSubcontainer.DeepCloneEntireTreeRecursivelyIncludingFilters();

            //get the returned filter subcontainer and associate it with the cloned version of this
            clone.AddChild(clonedSubcontainer);
        }

        //return the cloned version
        return clone;
    }

    private AggregateFilterContainer ShallowClone()
    {
        var container = new AggregateFilterContainer(CatalogueRepository, Operation);
        CopyShallowValuesTo(container);
        return container;
    }

    /// <summary>
    /// Returns the AggregateConfiguration for which this container is either the root container for or part of the root container subcontainer tree.
    /// Returns null if the container is somehow an orphan.
    /// </summary>
    /// <returns></returns>
    public AggregateConfiguration GetAggregate()
    {
        var aggregateConfiguration = Repository.GetAllObjectsWhere<AggregateConfiguration>("RootFilterContainer_ID", ID)
            .SingleOrDefault();

        if (aggregateConfiguration != null)
            return aggregateConfiguration;

        var parentContainer = GetParentContainerIfAny();

        return ((AggregateFilterContainer)parentContainer)?.GetAggregate();
    }

    public override IFilterFactory GetFilterFactory() => new AggregateFilterFactory(CatalogueRepository);

    public override void DeleteInDatabase()
    {
        base.DeleteInDatabase();

        foreach (var ac in Repository.GetAllObjectsWhere<AggregateConfiguration>(
                     nameof(AggregateConfiguration.RootFilterContainer_ID), ID))
        {
            ac.RootFilterContainer_ID = null;
            ac.SaveToDatabase();
        }
    }
}