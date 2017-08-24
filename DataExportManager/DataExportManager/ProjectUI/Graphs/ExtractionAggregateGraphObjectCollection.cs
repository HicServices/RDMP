using System.Collections.Generic;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.LinkCreators;
using MapsDirectlyToDatabaseTable;

namespace DataExportManager.ProjectUI.Graphs
{
    public class ExtractionAggregateGraphObjectCollection : IPersistableObjectCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

        public SelectedDataSets SelectedDataSets { get { return (SelectedDataSets) DatabaseObjects[0]; }}
        public AggregateConfiguration Graph { get { return (AggregateConfiguration) DatabaseObjects[1]; }}

        /// <summary>
        /// Constructor used for persistence
        /// </summary>
        public ExtractionAggregateGraphObjectCollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        /// <summary>
        /// Use this constructor at runtime
        /// </summary>
        /// <param name="selectedDataSet"></param>
        /// <param name="graph"></param>
        public ExtractionAggregateGraphObjectCollection(SelectedDataSets selectedDataSet, AggregateConfiguration graph):this()
        {
            DatabaseObjects.Add(selectedDataSet);
            DatabaseObjects.Add(graph);
        }
        
        public string SaveExtraText()
        {
            return "";//no extra data required
        }

        public void LoadExtraText(string s)
        {
            //no action required
        }
    }
}