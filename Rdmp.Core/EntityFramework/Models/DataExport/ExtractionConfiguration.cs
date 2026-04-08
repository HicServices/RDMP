using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.Logging.PastEvents;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    [Table("ExtractionConfiguration")]
    public class ExtractionConfiguration: DatabaseObject, IExtractionConfiguration
    {
        [Key]
        public override int ID { get; set; }

        public int Project_ID { get; set; }
        public int? CohortIdentificationConfiguration_ID { get; set; }

        public int? Cohort_ID { get; set; }

        [ForeignKey("Project_ID")]
        public virtual Project Project { get; set; }

        [ForeignKey("CohortIdentificationConfiguration_ID")]
        public virtual CohortIdentificationConfiguration CohortIdentificationConfiguration { get; set; }

        //is an fk for cohort_ID
        public IExtractableCohort Cohort { get; set; }
        public string Name { get; set; }
        public IPipeline CohortRefreshPipeline { get; internal set; }
        public List<object> ReleaseLog { get; internal set; }
        public bool IsReleased { get; set; }
        public List<object> SelectedDataSets { get; set; }

        public int? DefaultPipeline_ID { get; set; }

        [ForeignKey("DefaultPipeline_ID")]
        public Pipeline DefaultPipeline { get; set; }
        public DateTime? dtCreated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RequestTicket { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ReleaseTicket { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Username => throw new NotImplementedException();

        public string Separator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? ClonedFrom_ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int? CohortRefreshPipeline_ID { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ISqlParameter[] GlobalExtractionFilterParameters => throw new NotImplementedException();

        public IEnumerable<ICumulativeExtractionResults> CumulativeExtractionResults => throw new NotImplementedException();

        public IEnumerable<ISupplementalExtractionResults> SupplementalExtractionResults => throw new NotImplementedException();

        IProject IExtractionConfiguration.Project => Project;

        IReleaseLog[] IExtractionConfiguration.ReleaseLog => throw new NotImplementedException();

        ISelectedDataSets[] IExtractionConfiguration.SelectedDataSets => throw new NotImplementedException();

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ArchivalDataLoadInfo> FilterRuns(IEnumerable<ArchivalDataLoadInfo> runs)
        {
            throw new NotImplementedException();
        }

        public ExtractableColumn[] GetAllExtractableColumnsFor(IExtractableDataSet dataset)
        {
            throw new NotImplementedException();
        }

        public IExtractableDataSet[] GetAllExtractableDataSets()
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase()
        {
            throw new NotImplementedException();
        }

        public DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen)
        {
            throw new NotImplementedException();
        }

        public string GetDistinctLoggingTask()
        {
            throw new NotImplementedException();
        }

        public IExtractableCohort GetExtractableCohort()
        {
            throw new NotImplementedException();
        }

        public IContainer GetFilterContainerFor(IExtractableDataSet dataset)
        {
            throw new NotImplementedException();
        }

        public IMapsDirectlyToDatabaseTable[] GetGlobals()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IProject GetProject()
        {
            throw new NotImplementedException();
        }

        public string GetProjectHint(bool v)
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool IsExtractable(out string reason)
        {
            throw new NotImplementedException();
        }

        public void RemoveDatasetFromConfiguration(IExtractableDataSet extractableDataSet)
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

        public void SaveToDatabase()
        {
            throw new NotImplementedException();
        }

        public bool ShouldBeReadOnly(string context, out string reason)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => Name;

        public void Unfreeze()
        {
            throw new NotImplementedException();
        }
    }
}
