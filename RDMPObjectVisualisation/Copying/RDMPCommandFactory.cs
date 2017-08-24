using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableUIComponents.Annotations;
using ReusableUIComponents.Copying;
using ReusableUIComponents.TreeHelper;

namespace RDMPObjectVisualisation.Copying
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
            //Extractable column e.g. ExtractionInformation,AggregateDimension etc
            var icolumn = modelObject as IColumn;
            if (icolumn != null)
                return new ColumnCommand(icolumn);

            //table column pointers (not extractable)
            var columnInfo = modelObject as ColumnInfo;
            var linkedColumnInfo = modelObject as LinkedColumnInfoNode;
            if (columnInfo != null || linkedColumnInfo != null)
                return new ColumnInfoCommand(columnInfo ?? linkedColumnInfo.ColumnInfo);

            var tableInfo = modelObject as TableInfo;
            if (tableInfo != null)
                return new TableInfoCommand(tableInfo);

            //catalogues
            var catalogue = modelObject as Catalogue;
            var manyCatalogues = IsArrayOf<Catalogue>(modelObject);
                
            if (catalogue != null)
                return new CatalogueCommand(catalogue);

            if (manyCatalogues != null)
                return new ManyCataloguesCommand(manyCatalogues);

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
            var extractableDataSet = modelObject as ExtractableDataSet;
            if (extractableDataSet != null)
                return new ExtractableDataSetCommand(extractableDataSet);

            var extractableDataSetArray = modelObject as ExtractableDataSet[];
            if (extractableDataSetArray != null)
                return new ExtractableDataSetCommand(extractableDataSetArray);

            var extractableDataSetPackage = modelObject as ExtractableDataSetPackage;
            if(extractableDataSetPackage != null)
                return new ExtractableDataSetCommand(extractableDataSetPackage);

            var dataAccessCredentials = modelObject as DataAccessCredentials;
            if (dataAccessCredentials != null)
                return new DataAccessCredentialsCommand(dataAccessCredentials);

            var processTask = modelObject as ProcessTask;
            if (processTask != null)
                return new ProcessTaskCommand(processTask);

            var commandSource = modelObject as ICommandSource;

            if (commandSource != null)
                return commandSource.GetCommand();

            return null;
        }

        private T[] IsArrayOf<T>(object modelObject)
        {
            if (!(modelObject is IEnumerable))
                return null;

            var array = (IEnumerable)modelObject;

            List<T> toReturn = new List<T>();


            foreach (var o in array)
            {
                if (o == null || o.GetType() != typeof (T))
                    return null;
                
                toReturn.Add((T)o);
            }

            return toReturn.ToArray();
        }
    }
}
