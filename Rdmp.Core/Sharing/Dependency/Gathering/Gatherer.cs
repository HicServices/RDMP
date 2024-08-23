// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Sharing.Dependency.Gathering;

/// <summary>
/// Gathers dependencies of a given object in a more advanced/selective way than simply using methods of IHasDependencies
/// </summary>
public class Gatherer
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    private readonly Dictionary<Type, Func<IMapsDirectlyToDatabaseTable, GatheredObject>> _functions = new();

    public Gatherer(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _repositoryLocator = repositoryLocator;

        _functions.Add(typeof(Catalogue), o => GatherDependencies((Catalogue)o));
        _functions.Add(typeof(ColumnInfo), o => GatherDependencies((ColumnInfo)o));
        _functions.Add(typeof(ANOTable), o => GatherDependencies((ANOTable)o));

        _functions.Add(typeof(LoadMetadata), o => GatherDependencies((LoadMetadata)o));

        _functions.Add(typeof(ExtractionFilter), o => GatherDependencies((IFilter)o));
        _functions.Add(typeof(DeployedExtractionFilter), o => GatherDependencies((IFilter)o));
        _functions.Add(typeof(AggregateFilter), o => GatherDependencies((IFilter)o));
    }

    public IMapsDirectlyToDatabaseTable[] GetAllObjectsInAllDatabases()
    {
        var allCatalogueObjects = _repositoryLocator.CatalogueRepository.GetAllObjectsInDatabase();
        var allDataExportObjects = _repositoryLocator.DataExportRepository.GetAllObjectsInDatabase();
        return allCatalogueObjects.Union(allDataExportObjects).ToArray();
    }

    /// <summary>
    /// Invokes the relevant overload if it exists.
    /// <seealso cref="CanGatherDependencies"/>
    /// </summary>
    /// <param name="databaseEntity"></param>
    /// <returns></returns>
    public bool CanGatherDependencies(IMapsDirectlyToDatabaseTable databaseEntity) =>
        _functions.ContainsKey(databaseEntity.GetType());

    public GatheredObject GatherDependencies(IMapsDirectlyToDatabaseTable o) => _functions[o.GetType()](o);

    public static GatheredObject GatherDependencies(ANOTable anoTable)
    {
        var root = new GatheredObject(anoTable.Server);
        root.Children.Add(new GatheredObject(anoTable));

        return root;
    }

    public GatheredObject GatherDependencies(LoadMetadata loadMetadata)
    {
        //Share the LoadMetadata
        var root = new GatheredObject(loadMetadata);
        //and the catalogues behind the load
        foreach (var cata in loadMetadata.GetAllCatalogues())
            root.Children.Add(GatherDependencies(cata));

        //and the load operations
        foreach (var processTask in loadMetadata.ProcessTasks)
        {
            var gpt = new GatheredObject(processTask);
            root.Children.Add(gpt);

            foreach (var a in processTask.GetAllArguments())
            {
                var ga = new GatheredObject(a);
                gpt.Children.Add(ga);
            }
        }

        var linkage = loadMetadata.CatalogueRepository.GetAllObjectsWhere<LoadMetadataCatalogueLinkage>("LoadMetadataID", loadMetadata.ID);
        foreach (var link in linkage)
        {
            var glcl = new GatheredObject(link);
            root.Children.Add(glcl);
        }

        return root;
    }

    public static GatheredObject GatherDependencies(Catalogue catalogue)
    {
        var root = new GatheredObject(catalogue);

        foreach (var cis in catalogue.CatalogueItems)
            root.Children.Add(new GatheredObject(cis));

        return root;
    }

    public static GatheredObject GatherDependencies(IFilter filter)
    {
        var root = new GatheredObject(filter);

        foreach (var param in filter.GetAllParameters())
            root.Children.Add(new GatheredObject((IMapsDirectlyToDatabaseTable)param));

        return root;
    }

    /// <summary>
    /// Gathers dependencies of ColumnInfo, this includes all [Sql] properties on any object in data export / catalogue databases
    /// which references the fully qualified name of the ColumnInfo as well as its immediate network friends that should share its
    /// runtime name e.g. CatalogueItem and ExtractionInformation.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public GatheredObject GatherDependencies(ColumnInfo c)
    {
        var allObjects = GetAllObjectsInAllDatabases();

        var propertyFinder = new AttributePropertyFinder<SqlAttribute>(allObjects);

        var root = new GatheredObject(c);

        foreach (var o in allObjects)
        {
            //don't add a reference to the thing we are gathering dependencies on!
            if (Equals(o, c))
                continue;

            foreach (var propertyInfo in propertyFinder.GetProperties(o))
            {
                var sql = (string)propertyInfo.GetValue(o);

                if (sql != null && sql.Contains(c.Name))
                    root.Children.Add(new GatheredObject(o));
            }
        }

        return root;
    }
}