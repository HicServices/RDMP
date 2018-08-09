using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.Pipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataExportLibrary.CohortCreationPipeline
{
    /// <summary>
    /// All metadata details nessesary to create a cohort including which project it goes into, it's name, version etc.  There are no identifiers for the cohort.
    /// Also functions as the use case for cohort creation (to which it passes itself as an input object).
    /// </summary>
    public class CohortCreationRequest : PipelineUseCase,ICohortCreationRequest, ICheckable
    {
        private readonly IDataExportRepository _repository;

        //for pipeline editing initialization when no known cohort is available

        #region Things that can be turned into cohorts
        
        private FlatFileToLoad _fileToLoad;
        private ExtractionInformation _extractionIdentifierColumn;
        private CohortIdentificationConfiguration _cohortIdentificationConfiguration;

        public FlatFileToLoad FileToLoad
        {
            get { return _fileToLoad; }
            set
            {
                //remove old value if it had one
                Pop(_fileToLoad);

                _fileToLoad = value;
                
                //add the new one
                Push(value);
            }
        }
        
        public CohortIdentificationConfiguration CohortIdentificationConfiguration
        {
            get { return _cohortIdentificationConfiguration; }
            set
            {
                Pop(_cohortIdentificationConfiguration);
                _cohortIdentificationConfiguration = value; 
                Push(value);
            }
        }

        public ExtractionInformation ExtractionIdentifierColumn
        {
            get { return _extractionIdentifierColumn; }
            set
            {
                Pop(_extractionIdentifierColumn);
                _extractionIdentifierColumn = value; 
                Push(_extractionIdentifierColumn);
            }
        }
        
        private void Pop(object oldValue)
        {
            if (oldValue != null && InitializationObjects.Contains(oldValue))
                InitializationObjects.Remove(oldValue);
        }

        private void Push(object newValue)
        {
            AddInitializationObject(newValue);
        }
        #endregion

        public IProject Project { get; private set; }
        public ICohortDefinition NewCohortDefinition { get; set; }
        public ExtractableCohort CohortCreatedIfAny { get; set; }
        
        public CohortCreationRequest(Project project, CohortDefinition newCohortDefinition, IDataExportRepository repository, string descriptionForAuditLog)
        {
            _repository = repository;
            Project = project;
            NewCohortDefinition = newCohortDefinition;

            DescriptionForAuditLog = descriptionForAuditLog;
            
            AddInitializationObject(Project);
        }

        /// <summary>
        /// For refreshing the current extraction configuration CohortIdentificationConfiguration ONLY.  The ExtractionConfiguration must have a cic and a refresh pipeline configured on it.
        /// </summary>
        /// <param name="configuration"></param>
        public CohortCreationRequest(ExtractionConfiguration configuration)
        {
            _repository = (DataExportRepository) configuration.Repository;

            if (configuration.CohortIdentificationConfiguration_ID == null)
                throw new NotSupportedException("Configuration '" + configuration + "' does not have an associated CohortIdentificationConfiguration for cohort refreshing");

            var origCohort = configuration.Cohort;
            var origCohortData = origCohort.GetExternalData();
            CohortIdentificationConfiguration = configuration.CohortIdentificationConfiguration;
            Project = configuration.Project;
            
            if(Project.ProjectNumber == null)
                throw new NotSupportedException("Project '" + Project  + "' does not have a ProjectNumber");

            var definition = new CohortDefinition(null, origCohortData.ExternalDescription, origCohortData.ExternalVersion + 1,(int) Project.ProjectNumber, origCohort.ExternalCohortTable);
            NewCohortDefinition = definition;
            DescriptionForAuditLog = "Cohort Refresh";

            AddInitializationObject(Project);
            AddInitializationObject(CohortIdentificationConfiguration);
            AddInitializationObject(FileToLoad);
            AddInitializationObject(ExtractionIdentifierColumn);
            AddInitializationObject(this);
        }

        protected override IDataFlowPipelineContext GenerateContext()
        {
            return new DataFlowPipelineContext<DataTable>
            {
                MustHaveDestination = typeof(ICohortPipelineDestination),
                MustHaveSource = typeof(IDataFlowSource<DataTable>)
            };
        }


        public string DescriptionForAuditLog { get; set; }
        
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

        public void PushToServer(IManagedConnection connection)
        {
            string reason;
            if(!NewCohortDefinition.IsAcceptableAsNewCohort(out reason))
                throw new Exception(reason);

            NewCohortDefinition.LocationOfCohort.PushToServer(NewCohortDefinition, connection);
        }

        public int ImportAsExtractableCohort()
        {
            if(NewCohortDefinition.ID == null)
                throw new NotSupportedException("CohortCreationRequest cannot be imported because it's ID is null, it is likely that it has not been pushed to the server yet");

            var cohort = new ExtractableCohort(_repository, (ExternalCohortTable) NewCohortDefinition.LocationOfCohort, (int)NewCohortDefinition.ID);
            cohort.AppendToAuditLog(DescriptionForAuditLog);

            CohortCreatedIfAny = cohort;

            return cohort.ID;
        }


        /// <summary>
        /// Design time types
        /// </summary>
        private CohortCreationRequest():base(new Type[]
        {
            typeof(FlatFileToLoad),
            typeof(CohortIdentificationConfiguration),
            typeof(Project),
            typeof(ExtractionInformation),
            typeof(ICohortCreationRequest)
        })
        {
            
        }

        public static PipelineUseCase DesignTime()
        {
            return new CohortCreationRequest();
        }

        public override string ToString()
        {
            if (NewCohortDefinition == null)
                return base.ToString();

            return NewCohortDefinition.Description;
        }
    }
}