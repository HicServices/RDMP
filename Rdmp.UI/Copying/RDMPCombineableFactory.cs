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

namespace Rdmp.UI.Copying
{
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
            if (o == null || o.ModelObjects == null)
                return null;

            //does the data object already contain a command? 
            if (o.ModelObjects.OfType<ICombineToMakeCommand>().Count() == 1)
                return o.ModelObjects.OfType<ICombineToMakeCommand>().Single();//yes

            //otherwise is it something that can be turned into a command?
            if (o.ModelObjects.Count == 0)
                return null;
            
            //try to create command from the single data object
            if (o.ModelObjects.Count == 1)
                return Create(o.ModelObjects[0]);
            
            //try to create command from all the data objects as an array
            return Create(o.ModelObjects.Cast<object>().ToArray());
        }

        public ICombineToMakeCommand Create(FileInfo[] files)
        {
            return new FileCollectionCombineable(files);
        }

        public ICombineToMakeCommand Create(object modelObject)
        {
            IMasqueradeAs masquerader = modelObject as IMasqueradeAs;

            if (masquerader != null)
                modelObject = masquerader.MasqueradingAs();

            //Extractable column e.g. ExtractionInformation,AggregateDimension etc
            var icolumn = modelObject as IColumn;
            if (icolumn != null)
                return new ColumnCombineable(icolumn);

            var pipeline = modelObject as Pipeline;
            if (pipeline != null)
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
            
            var tableInfo = modelObject as TableInfo;
            if (tableInfo != null)
                return new TableInfoCombineable(tableInfo);

            if(modelObject is LoadMetadata lmd)
                return new LoadMetadataCombineable(lmd);
            
            if (modelObject is Project p)
                return new ProjectCombineable(p);

            //catalogues
            var catalogues = IsArrayOf<Catalogue>(modelObject);
            
            if (catalogues != null)
                if(catalogues.Length == 1)
                    return new CatalogueCombineable(catalogues[0]);
                else
                    return new ManyCataloguesCombineable(catalogues);

            //filters
            var filter = modelObject as IFilter;
            if (filter != null)
                return new FilterCombineable(filter);

            //containers
            var container = modelObject as IContainer;
            if (container != null)
                return new ContainerCombineable(container);

            //aggregates
            var aggregate = modelObject as AggregateConfiguration;
            if (aggregate != null)
                return new AggregateConfigurationCombineable(aggregate);

            //aggregate containers
            var aggregateContainer = modelObject as CohortAggregateContainer;
            if (aggregateContainer != null)
                return new CohortAggregateContainerCombineable(aggregateContainer);
            
            var extractableCohort = modelObject as ExtractableCohort;
            if (extractableCohort != null)
                return new ExtractableCohortCombineable(extractableCohort);

            //extractable data sets
            var extractableDataSets = IsArrayOf<ExtractableDataSet>(modelObject);
            if (extractableDataSets != null)
                return new ExtractableDataSetCombineable(extractableDataSets);

            var extractableDataSetPackage = modelObject as ExtractableDataSetPackage;
            if(extractableDataSetPackage != null)
                return new ExtractableDataSetCombineable(extractableDataSetPackage);

            var dataAccessCredentials = modelObject as DataAccessCredentials;
            if (dataAccessCredentials != null)
                return new DataAccessCredentialsCombineable(dataAccessCredentials);

            var processTask = modelObject as ProcessTask;
            if (processTask != null)
                return new ProcessTaskCombineable(processTask);

            var cacheProgress = modelObject as CacheProgress;
            if (cacheProgress != null)
                return new CacheProgressCombineable(cacheProgress);

            var cic = modelObject as CohortIdentificationConfiguration;
            var cicAssociation = modelObject as ProjectCohortIdentificationConfigurationAssociation;
            if (cic != null || cicAssociation != null)
                return new CohortIdentificationConfigurationCommand(cic ?? cicAssociation.CohortIdentificationConfiguration);

            if (modelObject is ICombineableSource commandSource)
                return commandSource.GetCombineable();

            return null;
        }

        private T[] IsArrayOf<T>(object modelObject)
        {
            if(modelObject is T)
                return new []{(T)modelObject};

            if (!(modelObject is IEnumerable))
                return null;

            var array = (IEnumerable)modelObject;

            List<T> toReturn = new List<T>();

            foreach (var o in array)
            {
                //if array contains anything that isn't a T
                if (!(o is T))
                    return null; //it's not an array of T
                
                toReturn.Add((T)o);
            }

            //it's an array of T
            return toReturn.ToArray();
        }
    }

}
