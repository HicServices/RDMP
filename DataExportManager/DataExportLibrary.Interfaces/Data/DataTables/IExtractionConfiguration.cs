using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// See ExtractionConfiguration
    /// </summary>
    public interface IExtractionConfiguration:INamed
    {
        DateTime? dtCreated { get; set; }
        int? Cohort_ID { get; set; }
        string RequestTicket { get; set; }
        string ReleaseTicket { get; set; }
        int Project_ID { get; }
        IProject Project { get; }
        string Username { get; }
        string Separator { get; set; }
        string Description { get; set; }
        bool IsReleased { get; set; }
        int? ClonedFrom_ID { get; set; }

        IExtractableCohort Cohort { get;}
        
        int? DefaultPipeline_ID { get; set; }
        int? CohortIdentificationConfiguration_ID { get; set; }

        IExtractableCohort GetExtractableCohort();
        IProject GetProject();

        ISqlParameter[] GlobalExtractionFilterParameters { get; }
        IReleaseLogEntry[] ReleaseLogEntries { get; }
        IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults { get; }

        ConcreteColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset);
        IContainer GetFilterContainerFor(IExtractableDataSet dataset);
        IExtractableDataSet[] GetAllExtractableDataSets();
        ISelectedDataSets[] SelectedDataSets { get; }

        void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet);
        void Unfreeze();
        IMapsDirectlyToDatabaseTable[] GetGlobals();
    }
}