using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.UserPicks;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease
{
    /// <summary>
    /// Determines whether a given ExtractableDataSet in an ExtractionConfiguration is ready for Release. 
    /// Extraction Destinations will return an implementation of this class which will run checks on the releasaility of the extracted datasets
    /// based on the extraction method used.
    /// </summary>
    public abstract class ReleasePotential:ICheckable
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IRepository _repository;
        private List<IColumn> _columnsToExtract;

        public ISelectedDataSets SelectedDataSet { get; private set; }
        public IExtractionConfiguration Configuration { get; private set; }
        public IExtractableDataSet DataSet { get; private set; }

        public Dictionary<ExtractableColumn, ExtractionInformation> ColumnsThatAreDifferentFromCatalogue { get; private set; }

        public Exception Exception { get; private set; }
        public ICumulativeExtractionResults DatasetExtractionResult { get; protected set; }
        public DateTime DateOfExtraction { get; private set; }

        /// <summary>
        /// The SQL that was run when the extraction was last performed (or null if no extraction has ever been performed)
        /// </summary>
        public string SqlExtracted { get; private set; }

        /// <summary>
        /// The SQL that would be generated if the configuration/dataset were executed today (if this differes from SqlExtracted then there is an Sql Desynchronisation)
        /// </summary>
        public string SqlCurrentConfiguration { get; private set; }

        /// <summary>
        /// The directory that the extraction configuration last extracted data to (for this dataset).  This may no longer exist if people have been monkeying with the filesystem so check .Exists().  If no extraction has ever been made this will be NULL
        /// </summary>
        public DirectoryInfo ExtractDirectory { get; protected set; }

        /// <summary>
        /// The file that contains the dataset data e.g. biochemistry.csv (will be null if no extract files were found)
        /// </summary>
        public FileInfo ExtractFile { get; set; }

        public Dictionary<IExtractionResults, Releaseability> Assessments { get; protected set; }

        protected ReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISelectedDataSets selectedDataSet)
        {
            _repositoryLocator = repositoryLocator;
            _repository = selectedDataSet.Repository;
            SelectedDataSet = selectedDataSet;
            Configuration = selectedDataSet.ExtractionConfiguration;
            DataSet = selectedDataSet.ExtractableDataSet;
            Assessments = new Dictionary<IExtractionResults, Releaseability>();

            //see what has been extracted before
            var extractionResults = Configuration.CumulativeExtractionResults.FirstOrDefault(r => r.ExtractableDataSet_ID == DataSet.ID);
            if (extractionResults == null || extractionResults.DestinationDescription == null)
                return;

            DatasetExtractionResult = extractionResults;

            Assessments.Add(extractionResults, MakeAssesment(extractionResults));

            foreach (var supplementalResult in extractionResults.SupplementalExtractionResults)
                Assessments.Add(supplementalResult, MakeSupplementalAssesment(supplementalResult));
        }

        private Releaseability MakeAssesment(ICumulativeExtractionResults extractionResults)
        {
            try
            {
                //always try to figure out what the current SQL is
                SqlCurrentConfiguration = GetCurrentConfigurationSQL();

                //let the user know when the data was extracted
                DateOfExtraction = extractionResults.DateOfExtraction;
                SqlExtracted = extractionResults.SQLExecuted;

                //the cohort has been changed in the configuration in the time elapsed since the file we are evaluating was generated (that was when CummatliveExtractionResults was populated)
                if (extractionResults.CohortExtracted != Configuration.Cohort_ID)
                    return Releaseability.CohortDesynchronisation;

                if (SqlOutOfSyncWithDataExportManagerConfiguration(extractionResults))
                    return Releaseability.ExtractionSQLDesynchronisation;

                var finalAssessment = GetSpecificAssessment(extractionResults);

                if (finalAssessment == Releaseability.Undefined)
                    return SqlDifferencesVsLiveCatalogue() ? Releaseability.ColumnDifferencesVsCatalogue : Releaseability.Releaseable;

                return finalAssessment;
            }
            catch (Exception e)
            {
                Exception = e;
                return Releaseability.ExceptionOccurredWhileEvaluatingReleaseability;
            }
        }

        private Releaseability MakeSupplementalAssesment(ISupplementalExtractionResults supplementalExtractionResults)
        {
            try
            {
                var extractedObject = _repositoryLocator.GetArbitraryDatabaseObject(
                        supplementalExtractionResults.RepositoryType,
                        supplementalExtractionResults.ExtractedType, 
                        supplementalExtractionResults.ExtractedId) as INamed;

                if (extractedObject == null)
                    return Releaseability.Undefined;
                    
                if (extractedObject is SupportingSQLTable)
                {
                    if ((extractedObject as SupportingSQLTable).SQL != supplementalExtractionResults.SQLExecuted)
                        return Releaseability.ExtractionSQLDesynchronisation;
                }

                var finalAssessment = GetSupplementalSpecificAssessment(supplementalExtractionResults);

                if (finalAssessment == Releaseability.Undefined)
                    return (extractedObject.Name != supplementalExtractionResults.ExtractedName ? Releaseability.ExtractionSQLDesynchronisation : Releaseability.Releaseable);

                return finalAssessment;
            }
            catch (Exception e)
            {
                Exception = e;
                return Releaseability.ExceptionOccurredWhileEvaluatingReleaseability;
            }
        }

        protected abstract Releaseability GetSupplementalSpecificAssessment(ISupplementalExtractionResults supplementalExtractionResults);

        protected abstract Releaseability GetSpecificAssessment(ICumulativeExtractionResults extractionResults);

        private bool SqlDifferencesVsLiveCatalogue()
        {
            ColumnsThatAreDifferentFromCatalogue = new Dictionary<ExtractableColumn, ExtractionInformation>();

            foreach (ExtractableColumn extractableColumn in _columnsToExtract)
            {
                if(extractableColumn.HasOriginalExtractionInformationVanished())
                {
                    ColumnsThatAreDifferentFromCatalogue.Add(extractableColumn,null);
                    continue;
                }

                var masterCatalogueVersion = extractableColumn.CatalogueExtractionInformation;

                if (!masterCatalogueVersion.SelectSQL.Equals(extractableColumn.SelectSQL))
                    ColumnsThatAreDifferentFromCatalogue.Add(extractableColumn, masterCatalogueVersion);
            }

            return ColumnsThatAreDifferentFromCatalogue.Any();
        }

        private string GetCurrentConfigurationSQL()
        {
            //get the cohort
            var cohort = _repository.GetObjectByID<ExtractableCohort>((int)Configuration.Cohort_ID);

            //get the columns that are configured today
            _columnsToExtract = new List<IColumn>(Configuration.GetAllExtractableColumnsFor(DataSet));
            _columnsToExtract.Sort();

            //get the salt
            var project = _repository.GetObjectByID<Project>(Configuration.Project_ID);
            var salt = new HICProjectSalt(project);

            //create a request for an empty bundle - only the dataset
            var request = new ExtractDatasetCommand(_repositoryLocator, Configuration, cohort, new ExtractableDatasetBundle(DataSet), _columnsToExtract, salt, "", null);
            
            request.GenerateQueryBuilder();

            //Generated the SQL as it would exist today for this extraction
            var resultLive = request.QueryBuilder;

            return resultLive.SQL;
        }
        
        private bool SqlOutOfSyncWithDataExportManagerConfiguration(IExtractionResults extractionResults)
        {
            if (extractionResults.SQLExecuted == null)
                throw new Exception("Cumulative Extraction Results for the extraction in which this dataset was involved in does not have any SQLExecuted recorded for it.");
            
            //if the SQL today is different to the SQL that was run when the user last extracted the data then there is a desync in the SQL (someone has changed something in the catalogue/data export manager configuration since the data was extracted)
            return !SqlCurrentConfiguration.Equals(extractionResults.SQLExecuted);
        }
        
        public override string ToString()
        {
            switch (Assessments[DatasetExtractionResult])
            {
                case Releaseability.ExceptionOccurredWhileEvaluatingReleaseability:
                    return Exception.ToString();
                default:
                    string toReturn = "Dataset: " + DataSet;
                    toReturn += " DateOfExtraction: " + DateOfExtraction;
                    toReturn += " Status: " + Assessments[DatasetExtractionResult];

                    return toReturn;
            }
        }

        public virtual void Check(ICheckNotifier notifier)
        {
            //todo : call MakeAssesment here instead of constructor
            foreach (KeyValuePair<IExtractionResults, Releaseability> kvp in Assessments)
            {
                CheckResult checkResult;
                switch (kvp.Value)
                {
                    case Releaseability.ColumnDifferencesVsCatalogue:
                        checkResult = CheckResult.Warning;
                        break;
                    case Releaseability.Releaseable:
                        checkResult = CheckResult.Success;
                        break;
                    default:
                        checkResult = CheckResult.Fail;
                        break;
                }

                notifier.OnCheckPerformed(new CheckEventArgs(kvp.Key + " is " + kvp.Value, checkResult));
            }
        }
    }

    public enum Releaseability
    {
        Undefined = 0,
        ExceptionOccurredWhileEvaluatingReleaseability,
        NeverBeenSuccessfullyExecuted,
        ExtractFilesMissing,
        ExtractionSQLDesynchronisation,
        CohortDesynchronisation,
        ColumnDifferencesVsCatalogue,
        Releaseable
    }
}
