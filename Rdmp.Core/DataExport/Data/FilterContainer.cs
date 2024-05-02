// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     Sometimes you need to limit which records are extracted as part of an ExtractionConfiguration (See
///     DeployedExtractionFilter).  In order to assemble valid WHERE SQL for this use
///     case each DeployedExtractionFilter must be in either an AND or an OR container.  These FilterContainers ensure that
///     each subcontainer / filter beyond the first is seperated by
///     the appropriate operator (AND or OR) and brackets/tab indents where appropriate.
/// </summary>
public class FilterContainer : ConcreteContainer, IContainer
{
    public FilterContainer()
    {
    }

    /// <summary>
    ///     Creates a new instance with the given <paramref name="operation" />
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="operation"></param>
    public FilterContainer(IDataExportRepository repository,
        FilterContainerOperation operation = FilterContainerOperation.AND) : base(repository.FilterManager)
    {
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Operation", operation.ToString() }
        });
    }

    internal FilterContainer(IDataExportRepository repository, DbDataReader r) : base(repository.FilterManager,
        repository, r)
    {
    }

    /// <inheritdoc />
    public override Catalogue GetCatalogueIfAny()
    {
        var sel = GetSelectedDataSetIfAny();
        return (Catalogue)sel?.ExtractableDataSet.Catalogue;
    }

    public override bool ShouldBeReadOnly(out string reason)
    {
        var ec = GetSelectedDataSetsRecursively()?.ExtractionConfiguration;

        if (ec == null)
        {
            reason = null;
            return false;
        }

        return ec.ShouldBeReadOnly(out reason);
    }

    /// <summary>
    ///     Returns the <see cref="ConcreteContainer.Operation" /> "AND" or "OR"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Operation.ToString();
    }


    public override IContainer DeepCloneEntireTreeRecursivelyIncludingFilters()
    {
        //clone ourselves
        var clonedFilterContainer = ShallowClone();

        //clone our filters
        foreach (var deployedExtractionFilter in GetFilters())
        {
            //clone it
            var cloneFilter = ((DeployedExtractionFilter)deployedExtractionFilter).ShallowClone(clonedFilterContainer);

            //clone parameters
            foreach (DeployedExtractionFilterParameter parameter in deployedExtractionFilter.GetAllParameters())
                parameter.ShallowClone(cloneFilter);

            //change the clone to belonging to the cloned container (instead of this - the original container)
            cloneFilter.FilterContainer_ID = clonedFilterContainer.ID;
            cloneFilter.SaveToDatabase();
        }

        //now clone all subcontainers
        foreach (FilterContainer toCloneSubcontainer in GetSubContainers())
        {
            //clone the subcontainer recursively
            var clonedSubcontainer = toCloneSubcontainer.DeepCloneEntireTreeRecursivelyIncludingFilters();

            //get the returned filter subcontainer and assocaite it with the cloned version of this
            clonedFilterContainer.AddChild(clonedSubcontainer);
        }

        //return the cloned version
        return clonedFilterContainer;
    }

    private FilterContainer ShallowClone()
    {
        var clone = new FilterContainer(DataExportRepository, Operation);
        CopyShallowValuesTo(clone);
        return clone;
    }

    /// <summary>
    ///     If this container is a top level root container (as opposed to a subcontainer) this will return which
    ///     <see cref="SelectedDataSets" /> (which dataset in which configuration)
    ///     in which it applies.
    /// </summary>
    /// <returns></returns>
    public SelectedDataSets GetSelectedDataSetIfAny()
    {
        var root = GetRootContainerOrSelf();

        return root == null
            ? null
            : Repository.GetAllObjectsWhere<SelectedDataSets>("RootFilterContainer_ID", root.ID).SingleOrDefault();
    }


    /// <summary>
    ///     ///
    ///     <summary>
    ///         Return which <see cref="SelectedDataSets" /> (which dataset in which configuration) this container resides in
    ///         (or null if it is an orphan).  This
    ///         involves multiple database queries as the container hierarchy is recursively traversed up.
    ///     </summary>
    ///     <returns></returns>
    /// </summary>
    /// <returns></returns>
    public SelectedDataSets GetSelectedDataSetsRecursively()
    {
        //if it is a root
        var dataset = GetSelectedDataSetIfAny();

        //return the root
        if (dataset != null)
            return dataset;

        //it's not a root but it might have a parent (it should do!)
        var parent = (FilterContainer)GetParentContainerIfAny();

        //it doesn't

        //our parent must be the root container maybe? recursive
        return parent?.GetSelectedDataSetsRecursively();
    }

    public override IFilterFactory GetFilterFactory()
    {
        return new DeployedExtractionFilterFactory(DataExportRepository);
    }

    public override void DeleteInDatabase()
    {
        base.DeleteInDatabase();

        foreach (var sds in Repository.GetAllObjectsWhere<SelectedDataSets>(
                     nameof(SelectedDataSets.RootFilterContainer_ID), ID))
        {
            sds.RootFilterContainer_ID = null;
            sds.SaveToDatabase();
        }
    }
}