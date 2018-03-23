using System;
using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Spontaneous;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Spontaneous
{
    public class SpontaneouslyInventedExtractionConfiguration : SpontaneousObject, IExtractionConfiguration
    {
        public int Project_ID { get; private set; }
        public IProject Project { get; private set; }

        public SpontaneouslyInventedExtractionConfiguration(Project project)
        {
            Project = project;
            Project_ID = project.ID;
        }

        public string Name { get; set; }
        public DateTime? dtCreated { get; set; }
        public int? Cohort_ID { get; set; }
        public string RequestTicket { get; set; }
        public string ReleaseTicket { get; set; }
        public string Username { get; private set; }
        public string Separator { get; set; }
        public string Description { get; set; }
        public bool IsReleased { get; set; }
        public int? ClonedFrom_ID { get; set; }
        public int? DefaultPipeline_ID { get; set; }
        public int? CohortIdentificationConfiguration_ID { get; set; }

        public IExtractableCohort GetExtractableCohort()
        {
            throw new NotImplementedException();
        }

        public IProject GetProject()
        {
            throw new NotImplementedException();
        }

        public ISqlParameter[] GlobalExtractionFilterParameters { get; private set; }
        public IReleaseLogEntry[] ReleaseLogEntries { get; private set; }
        public IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults { get; private set; }

        public ConcreteColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset)
        {
            throw new NotImplementedException();
        }

        public IContainer GetFilterContainerFor(IExtractableDataSet dataset)
        {
            throw new NotImplementedException();
        }

        public IExtractableDataSet[] GetAllExtractableDataSets()
        {
            throw new NotImplementedException();
        }

        public ISelectedDataSets[] SelectedDataSets { get; private set; }

        public void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet)
        {
            throw new NotImplementedException();
        }

        public void Unfreeze()
        {
            throw new NotImplementedException();
        }
    }
}