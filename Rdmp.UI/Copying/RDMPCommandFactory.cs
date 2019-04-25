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
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data.Cache;
using Rdmp.Core.CatalogueLibrary.Data.Cohort;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CatalogueLibrary.Data.Pipelines;
using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.UI.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace Rdmp.UI.Copying
{
    public class RDMPCommandFactory:ICommandFactory
    {
        public ICommand Create(ModelDropEventArgs e)
        {
            return Create((OLVDataObject)e.DataObject);
        }

        public ICommand Create(DragEventArgs e)
        {
            return Create(e.Data as OLVDataObject);
        }

        public ICommand Create(OLVDataObject o)
        {
            if (o == null || o.ModelObjects == null)
                return null;

            //does the data object already contain a command? 
            if (o.ModelObjects.OfType<ICommand>().Count() == 1)
                return o.ModelObjects.OfType<ICommand>().Single();//yes

            //otherwise is it something that can be turned into a command?
            if (o.ModelObjects.Count == 0)
                return null;
            
            //try to create command from the single data object
            if (o.ModelObjects.Count == 1)
                return Create(o.ModelObjects[0]);
            
            //try to create command from all the data objects as an array
            return Create(o.ModelObjects.Cast<object>().ToArray());
        }

        public ICommand Create(FileInfo[] files)
        {
            return new FileCollectionCommand(files);
        }

        public ICommand Create(object modelObject)
        {
            IMasqueradeAs masquerader = modelObject as IMasqueradeAs;

            if (masquerader != null)
                modelObject = masquerader.MasqueradingAs();

            //Extractable column e.g. ExtractionInformation,AggregateDimension etc
            var icolumn = modelObject as IColumn;
            if (icolumn != null)
                return new ColumnCommand(icolumn);

            var pipeline = modelObject as Pipeline;
            if (pipeline != null)
                return new PipelineCommand(pipeline);

            //table column pointers (not extractable)
            var columnInfo = modelObject as ColumnInfo; //ColumnInfo is not an IColumn btw because it does not have column order or other extraction rule stuff (alias, hash etc)
            var linkedColumnInfo = modelObject as LinkedColumnInfoNode;
            
            if (columnInfo != null || linkedColumnInfo != null)
                return new ColumnInfoCommand(columnInfo ?? linkedColumnInfo.ColumnInfo);

            var columnInfoArray = IsArrayOf<ColumnInfo>(modelObject);
            if(columnInfoArray != null)
                return new ColumnInfoCommand(columnInfoArray);
            
            var tableInfo = modelObject as TableInfo;
            if (tableInfo != null)
                return new TableInfoCommand(tableInfo);

            //catalogues
            var catalogues = IsArrayOf<Catalogue>(modelObject);
            
            if (catalogues != null)
                if(catalogues.Length == 1)
                    return new CatalogueCommand(catalogues[0]);
                else
                    return new ManyCataloguesCommand(catalogues);

            //filters
            var filter = modelObject as IFilter;
            if (filter != null)
                return new FilterCommand(filter);

            //containers
            var container = modelObject as IContainer;
            if (container != null)
                return new ContainerCommand(container);

            //aggregates
            var aggregate = modelObject as AggregateConfiguration;
            if (aggregate != null)
                return new AggregateConfigurationCommand(aggregate);

            //aggregate containers
            var aggregateContainer = modelObject as CohortAggregateContainer;
            if (aggregateContainer != null)
                return new CohortAggregateContainerCommand(aggregateContainer);
            
            var extractableCohort = modelObject as ExtractableCohort;
            if (extractableCohort != null)
                return new ExtractableCohortCommand(extractableCohort);

            //extractable data sets
            var extractableDataSets = IsArrayOf<ExtractableDataSet>(modelObject);
            if (extractableDataSets != null)
                return new ExtractableDataSetCommand(extractableDataSets);

            var extractableDataSetPackage = modelObject as ExtractableDataSetPackage;
            if(extractableDataSetPackage != null)
                return new ExtractableDataSetCommand(extractableDataSetPackage);

            var dataAccessCredentials = modelObject as DataAccessCredentials;
            if (dataAccessCredentials != null)
                return new DataAccessCredentialsCommand(dataAccessCredentials);

            var processTask = modelObject as ProcessTask;
            if (processTask != null)
                return new ProcessTaskCommand(processTask);

            var cacheProgress = modelObject as CacheProgress;
            if (cacheProgress != null)
                return new CacheProgressCommand(cacheProgress);

            var cic = modelObject as CohortIdentificationConfiguration;
            var cicAssociation = modelObject as ProjectCohortIdentificationConfigurationAssociation;
            if (cic != null || cicAssociation != null)
                return new CohortIdentificationConfigurationCommand(cic ?? cicAssociation.CohortIdentificationConfiguration);

            var commandSource = modelObject as ICommandSource;

            if (commandSource != null)
                return commandSource.GetCommand();

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
