using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.DataRelease
{
    /// <summary>
    /// Determines whether a given ExtractableDataSet in an ExtractionConfiguration is ready for Release.  This includes making sure that the current configuration
    /// in the database matches the extracted flat files that are destined for release.  It also checks that the user hasn't snuck some additional files into
    /// the extract directory etc.
    /// </summary>
    public class ReleasePotential
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IRepository _repository;
        private List<IColumn> _columnsToExtract;
        public IExtractionConfiguration Configuration { get; set; }
        public IExtractableDataSet DataSet { get; private set; }
        public ICumulativeExtractionResults ExtractionResults { get; private set; }
        public Dictionary<ExtractableColumn, ExtractionInformation> ColumnsThatAreDifferentFromCatalogue { get; private set; }

        public Exception Exception { get; private set; }

        /// <summary>
        /// The file that contains the dataset data e.g. biochemistry.csv (will be null if no extract files were found)
        /// </summary>
        public FileInfo ExtractFile { get; set; }

        /// <summary>
        /// The file that contains metadata for the dataset e.g. biochemistry.docx (will be null if no extract files were found)
        /// </summary>
        public FileInfo MetadataFile { get; set; }

        public ReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration configuration, IExtractableDataSet dataSet)
        {
            _repositoryLocator = repositoryLocator;
            _repository = configuration.Repository;
            Configuration = configuration;
            DataSet = dataSet;

            //see what has been extracted before
            ExtractionResults = Configuration.CumulativeExtractionResults.FirstOrDefault(r => r.ExtractableDataSet_ID == DataSet.ID);

            MakeAssesment();

        }

        private void MakeAssesment()
        {
            try
            {
                //always try to figure out what the current SQL is
                SqlCurrentConfiguration = GetCurrentConfigurationSQL();

                if (ExtractionResults == null || ExtractionResults.Filename == null)
                {
                    Assesment = Releaseability.NeverBeensuccessfullyExecuted;
                    return;
                }

                ExtractDirectory = new FileInfo(ExtractionResults.Filename).Directory;
                //let the user know when the data was extracted
                DateOfExtraction = ExtractionResults.DateOfExtraction;
                SqlExtracted = ExtractionResults.SQLExecuted;
                
                //the cohort has been changed in the configuration in the time elapsed since the file we are evaluating was generated (that was when CummatliveExtractionResults was populated)
                if (ExtractionResults.CohortExtracted != Configuration.Cohort_ID)
                    Assesment = Releaseability.CohortDesynchronisation;
                else
                if (SqlOutOfSyncWithDataExportManagerConfiguration())
                    Assesment = Releaseability.ExtractionSQLDesynchronisation;
                else if (FilesAreMissing())
                    Assesment = Releaseability.ExtractFilesMissing;
                else
                {
                    ThrowIfPollutionFoundInConfigurationRootExtractionFolder();

                    Assesment = SqlDifferencesVsLiveCatalogue() ? Releaseability.ColumnDifferencesVsCatalogue : Releaseability.Releaseable;
                }
            }
            catch (Exception e)
            {
                Exception = e;
                Assesment = Releaseability.ExceptionOccurredWhileEvaluatingReleaseability;
            }
        }

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

        private void ThrowIfPollutionFoundInConfigurationRootExtractionFolder()
        {
            Debug.Assert(ExtractDirectory.Parent != null, "Dont call this method until you have determined that an extracted file was actually produced!");

            if(ExtractDirectory.Parent.GetFiles().Any())
                throw new Exception("The following pollutants were found in the extraction directory\" " + ExtractDirectory.Parent.FullName + "\" pollutants were:" + ExtractDirectory.Parent.GetFiles().Aggregate("",(s,n)=>s + "\""+n+"\""));
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

        private bool FilesAreMissing()
        {
            ExtractFile = new FileInfo(ExtractionResults.Filename);
            MetadataFile = new FileInfo(ExtractionResults.Filename.Replace(".csv", ".docx"));

            if (!ExtractFile.Exists)
                return true;//extract is missing

            if (!ExtractFile.Extension.Equals(".csv"))
                throw new Exception("Extraction file had extension '" + ExtractFile.Extension + "' (expected .csv)");

            if (!MetadataFile.Exists)
                return true;
            
            //see if there is any other polution in the extract directory
            FileInfo unexpectedFile = ExtractFile.Directory.EnumerateFiles().FirstOrDefault(f=>
                !(f.Name.Equals(ExtractFile.Name) || f.Name.Equals(MetadataFile.Name)));

            if(unexpectedFile != null)
                throw new Exception("Unexpected file found in extract directory " + unexpectedFile.FullName + " (pollution of extract directory is not permitted)");

            DirectoryInfo unexpectedDirectory = ExtractFile.Directory.EnumerateDirectories().FirstOrDefault(d =>
                !(d.Name.Equals("Lookups") || d.Name.Equals("SupportingDocuments") || d.Name.Equals(SupportingSQLTable.ExtractionFolderName)));

            if(unexpectedDirectory != null)
                throw new Exception("Unexpected directory found in extraction directory " + unexpectedDirectory.FullName + " (pollution of extract directory is not permitted)");

            return false;
        }
        

        private bool SqlOutOfSyncWithDataExportManagerConfiguration()
        {

            if (ExtractionResults.SQLExecuted == null)
                throw new Exception("Cumulative Extraction Results for the extraction in which this dataset was involved in does not have any SQLExecuted recorded for it.");
            
            //if the SQL today is different to the SQL that was run when the user last extracted the data then there is a desync in the SQL (someone has changed something in the catalogue/data export manager configuration since the data was extracted)
            return !SqlCurrentConfiguration.Equals(ExtractionResults.SQLExecuted);
        }
        
        public Releaseability Assesment { get; private set; }
        public DateTime? DateOfExtraction { get; private set; }

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
        public DirectoryInfo ExtractDirectory { get; private set; }

        public override string ToString()
        {
            switch (Assesment)
            {
                case Releaseability.ExceptionOccurredWhileEvaluatingReleaseability:
                    return Exception.ToString();
                default:
                    string toReturn = "Dataset:" + DataSet;
                    if (DateOfExtraction != null)
                        toReturn += " DateOfExtraction:" + ((DateTime) DateOfExtraction);
                    toReturn += " Status:" + Assesment;

                    return toReturn;
            }
        }
    }


    public enum Releaseability
    {
        ExceptionOccurredWhileEvaluatingReleaseability,
        NeverBeensuccessfullyExecuted,
        ExtractFilesMissing,
        ExtractionSQLDesynchronisation,
        CohortDesynchronisation,
        ColumnDifferencesVsCatalogue,
        Releaseable
    }
}
