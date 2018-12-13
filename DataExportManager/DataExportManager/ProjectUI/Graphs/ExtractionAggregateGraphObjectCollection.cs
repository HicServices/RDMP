using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Dashboarding;
using DataExportLibrary.Data.LinkCreators;

namespace DataExportManager.ProjectUI.Graphs
{
    public class ExtractionAggregateGraphObjectCollection : PersistableObjectCollection
    {
        public SelectedDataSets SelectedDataSets { get { return (SelectedDataSets) DatabaseObjects[0]; }}
        public AggregateConfiguration Graph { get { return (AggregateConfiguration) DatabaseObjects[1]; }}

        /// <summary>
        /// Constructor used for persistence
        /// </summary>
        public ExtractionAggregateGraphObjectCollection()
        {

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
    }
}