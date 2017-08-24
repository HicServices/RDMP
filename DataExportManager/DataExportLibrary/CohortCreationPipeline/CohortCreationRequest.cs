using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.Pipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.CohortCreationPipeline
{
    public class CohortCreationRequest :ICohortCreationRequest, ICheckable
    {
        private readonly DataExportRepository _repository;
        public static DataFlowPipelineContext<DataTable> Context;

        static CohortCreationRequest()
        {
            Context = new DataFlowPipelineContext<DataTable>
            {
                MustHaveDestination = typeof (ICohortPipelineDestination),
                MustHaveSource = typeof (IDataFlowSource<DataTable>)
            };
        }

        public CohortCreationRequest(Project project, CohortDefinition newCohortDefinition, DataExportRepository repository, string descriptionForAuditLog)
        {
            _repository = repository;
            Project = project;
            NewCohortDefinition = newCohortDefinition;

            DescriptionForAuditLog = descriptionForAuditLog;
        }

        public IProject Project { get; private set; }
        public ICohortDefinition NewCohortDefinition { get; set; }
        public ExtractableCohort CohortCreatedIfAny { get; set; }

        public string DescriptionForAuditLog { get; set; }
        
        public static readonly CohortCreationRequest Empty = new CohortCreationRequest();

        private CohortCreationRequest()
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            NewCohortDefinition.LocationOfCohort.Check(notifier);
            
            if (NewCohortDefinition.ID != null)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Expected the cohort definition " + NewCohortDefinition +
                        " to have a null ID - we are trying to create this, why would it already exist?",
                        CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Confirmed that cohort ID is null", CheckResult.Success));

            if (Project.ProjectNumber == null)
                notifier.OnCheckPerformed(new CheckEventArgs("Project " + Project + " does not have a ProjectNumber specified, it should have the same number as the CohortCreationRequest ("+NewCohortDefinition.ProjectNumber+")", CheckResult.Fail));
            else
            if (Project.ProjectNumber != NewCohortDefinition.ProjectNumber)
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Project "+Project+" has ProjectNumber=" + Project.ProjectNumber +
                        " but the CohortCreationRequest.ProjectNumber is " + NewCohortDefinition.ProjectNumber + "",
                        CheckResult.Fail));
            
            
            string matchDescription;
            if (!NewCohortDefinition.IsAcceptableAsNewCohort(out matchDescription))
                notifier.OnCheckPerformed(new CheckEventArgs("Cohort failed novelness check:" + matchDescription,
                    CheckResult.Fail));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Confirmed that cohort " + NewCohortDefinition + " does not already exist",
                    CheckResult.Success));

            if (string.IsNullOrWhiteSpace(DescriptionForAuditLog))
                notifier.OnCheckPerformed(new CheckEventArgs("User did not provide a description of the cohort for the AuditLog",CheckResult.Fail));
        }

        public void PushToServer(SqlConnection con, SqlTransaction transaction)
        {
            string reason;
            if(!NewCohortDefinition.IsAcceptableAsNewCohort(out reason))
                throw new Exception(reason);

            NewCohortDefinition.LocationOfCohort.PushToServer(NewCohortDefinition, con, transaction);
        }

        public int ImportAsExtractableCohort()
        {
            if(NewCohortDefinition.ID == null)
                throw new NotSupportedException("CohortCreationRequest cannot be imported because it's ID is null, it is likely that it has not been pushed to the server yet");

            int whoCares;
            var cohort = new ExtractableCohort(_repository, (ExternalCohortTable) NewCohortDefinition.LocationOfCohort, (int)NewCohortDefinition.ID, out whoCares);
            cohort.AppendToAuditLog(DescriptionForAuditLog);

            CohortCreatedIfAny = cohort;

            return cohort.ID;
        }

        
    }
}