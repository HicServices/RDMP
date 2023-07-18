// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.QueryBuilding;
using Rdmp.UI.CommandExecution;

namespace Rdmp.UI.Copying;

/// <inheritdoc/>
public class RDMPCombineableFactory:ICombineableFactory
{
    public ICombineToMakeCommand Create(ModelDropEventArgs e)
    {
        return Create((OLVDataObject)e.DataObject);
    }

    public ICombineToMakeCommand Create(DragEventArgs e)
    {
        return Create(e.Data as OLVDataObject);
    }

    public ICombineToMakeCommand Create(OLVDataObject o)
    {
        if (o?.ModelObjects == null)
            return null;

        //does the data object already contain a command?
        if (o.ModelObjects.OfType<ICombineToMakeCommand>().Count() == 1)
            return o.ModelObjects.OfType<ICombineToMakeCommand>().Single();//yes

        return o.ModelObjects.Count switch
        {
            //otherwise is it something that can be turned into a command?
            0 => null,
            //try to create command from the single data object
            1 => Create(o.ModelObjects[0]),
            //try to create command from all the data objects as an array
            _ => Create(o.ModelObjects.Cast<object>().ToArray())
        };

    }

    public ICombineToMakeCommand Create(FileInfo[] files)
    {
        return new FileCollectionCombineable(files);
    }

    public ICombineToMakeCommand Create(object modelObject)
    {
        if (modelObject is IMasqueradeAs masquerader)
            modelObject = masquerader.MasqueradingAs();

        //Extractable column e.g. ExtractionInformation,AggregateDimension etc
        if (modelObject is IColumn icolumn)
            return new ColumnCombineable(icolumn);

        if (modelObject is Pipeline pipeline)
            return new PipelineCombineable(pipeline);
                       
        if (modelObject is  ExtractionFilterParameterSet efps)
            return new ExtractionFilterParameterSetCombineable(efps);

        if (modelObject is CatalogueItem ci)
            return new CatalogueItemCombineable(ci);

        var arrayOfCatalogueItems = IsArrayOf<CatalogueItem>(modelObject);
        if (arrayOfCatalogueItems != null)
        {
            return new CatalogueItemCombineable(arrayOfCatalogueItems);
        }

        //table column pointers (not extractable)
        var columnInfo = modelObject as ColumnInfo; //ColumnInfo is not an IColumn btw because it does not have column order or other extraction rule stuff (alias, hash etc)
        var linkedColumnInfo = modelObject as LinkedColumnInfoNode;
            
        if (columnInfo != null || linkedColumnInfo != null)
            return new ColumnInfoCombineable(columnInfo ?? linkedColumnInfo.ColumnInfo);

        var columnInfoArray = IsArrayOf<ColumnInfo>(modelObject);
        if(columnInfoArray != null)
            return new ColumnInfoCombineable(columnInfoArray);

        if (modelObject is TableInfo tableInfo)
            return new TableInfoCombineable(tableInfo);

        if(modelObject is LoadMetadata lmd)
            return new LoadMetadataCombineable(lmd);
            
        if (modelObject is Project p)
            return new ProjectCombineable(p);

        //catalogues
        var catalogues = IsArrayOf<Catalogue>(modelObject);
            
        if (catalogues != null)
            return catalogues.Length == 1 ? new CatalogueCombineable(catalogues[0]) : new ManyCataloguesCombineable(catalogues);

        //filters
        if (modelObject is IFilter filter)
            return new FilterCombineable(filter);

        //containers
        if (modelObject is IContainer container)
            return new ContainerCombineable(container);

        //aggregates
        if (modelObject is AggregateConfiguration aggregate)
            return new AggregateConfigurationCombineable(aggregate);

        //aggregate containers
        if (modelObject is CohortAggregateContainer aggregateContainer)
            return new CohortAggregateContainerCombineable(aggregateContainer);

        if (modelObject is ExtractableCohort extractableCohort)
            return new ExtractableCohortCombineable(extractableCohort);

        //extractable data sets
        var extractableDataSets = IsArrayOf<ExtractableDataSet>(modelObject);
        if (extractableDataSets != null)
            return new ExtractableDataSetCombineable(extractableDataSets);

        if(modelObject is ExtractableDataSetPackage extractableDataSetPackage)
            return new ExtractableDataSetCombineable(extractableDataSetPackage);

        if (modelObject is DataAccessCredentials dataAccessCredentials)
            return new DataAccessCredentialsCombineable(dataAccessCredentials);

        if (modelObject is ProcessTask processTask)
            return new ProcessTaskCombineable(processTask);

        if (modelObject is CacheProgress cacheProgress)
            return new CacheProgressCombineable(cacheProgress);

        var cic = modelObject as CohortIdentificationConfiguration;
        var cicAssociation = modelObject as ProjectCohortIdentificationConfigurationAssociation;
        if (cic != null || cicAssociation != null)
            return new CohortIdentificationConfigurationCommand(cic ?? cicAssociation.CohortIdentificationConfiguration);

        return modelObject is ICombineableSource commandSource ? commandSource.GetCombineable() : null;
    }

    private static T[] IsArrayOf<T>(object modelObject)
    {
        if(modelObject is T modelObject1)
            return new [] { modelObject1 };

        if (modelObject is not IEnumerable array)
            return null;

        var toReturn = new List<T>();

        foreach (var o in array)
        {
            //if array contains anything that isn't a T
            if (o is not T o1)
                return null; //it's not an array of T

            toReturn.Add(o1);
        }

        //it's an array of T
        return toReturn.ToArray();
    }
}