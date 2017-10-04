using System;
using System.Collections.Generic;
using System.IO;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
using DataExportLibrary.Data;
using DataExportLibrary.Repositories;

namespace DataExportLibrary.ExtractionTime.Commands
{
    public class ExtractDatasetCommand : IExtractDatasetCommand
    {
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }

        private IExtractableDatasetBundle _datasetBundle;
        public IExtractionConfiguration Configuration{get;set;}
        
        public IExtractableCohort ExtractableCohort { get; set; }

        public ExtractCommandState State { get; set; }
        public string Name { get { return DatasetBundle.DataSet.ToString(); } }

        public IExtractableDatasetBundle DatasetBundle
        {
            get { return _datasetBundle; }
            set
            {
                _datasetBundle = value; 

                if(value == null)
                    Catalogue = null;
                else
                    if(value.DataSet.Catalogue_ID == null)
                        throw new Exception("Cannot create a request for a dataset which does not have a Catalogue_ID set");
                    else
                        Catalogue = RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>((int)value.DataSet.Catalogue_ID);
            }
        }

        public List<IColumn> ColumnsToExtract{get;set;} 
        public IHICProjectSalt Salt{get;set;}
        public string LimitationSql{get;set;}
        public bool IncludeValidation {get;set;} 
        
        public IExtractionDirectory Directory { get; set; }
        public ICatalogue Catalogue { get; private set; }

        public ISqlQueryBuilder QueryBuilder { get; set; }
        public List<ReleaseIdentifierSubstitution> ReleaseIdentifierSubstitutions { get; private set; }

        public ExtractDatasetCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration configuration, IExtractableCohort extractableCohort, IExtractableDatasetBundle datasetBundle, List<IColumn> columnsToExtract, IHICProjectSalt salt, string limitationSql, IExtractionDirectory directory, bool includeValidation = false, bool includeLookups = false)
        {
            RepositoryLocator = repositoryLocator;
            Configuration = configuration;
            ExtractableCohort = extractableCohort;
            DatasetBundle = datasetBundle;
            ColumnsToExtract = columnsToExtract;
            Salt = salt;
            LimitationSql = limitationSql;
            Directory = directory;
            IncludeValidation = includeValidation;
        }

        /// <summary>
        /// This version has less arguments because it goes back to the database and queries the configuration and explores who the cohort is etc, it will result in more database
        /// queries than the more explicit constructor
        /// </summary>
        /// <param name="repositoryLocator"></param>
        /// <param name="configuration"></param>
        /// <param name="datasetBundle"></param>
        /// <param name="limitationSql"></param>
        /// <param name="includeValidation"></param>
        /// <param name="includeLookups"></param>
        public ExtractDatasetCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IExtractionConfiguration configuration, IExtractableDatasetBundle datasetBundle, string limitationSql = "", bool includeValidation = false, bool includeLookups = false)
        {
            RepositoryLocator = repositoryLocator;
            var project = configuration.Project;

            RepositoryLocator = repositoryLocator;
            Configuration = configuration;
            //ExtractableCohort = ExtractableCohort.GetExtractableCohortByID((int) configuration.Cohort_ID);
            ExtractableCohort = configuration.GetExtractableCohort();
            DatasetBundle = datasetBundle;
            ColumnsToExtract = new List<IColumn>(Configuration.GetAllExtractableColumnsFor(datasetBundle.DataSet));
            Salt = new HICProjectSalt(project);
            LimitationSql = limitationSql;
            Directory = new ExtractionDirectory(project.ExtractionDirectory, configuration);
            IncludeValidation = includeValidation;
        }

        public static readonly ExtractDatasetCommand EmptyCommand = new ExtractDatasetCommand();

        private ExtractDatasetCommand()
        {
        }

        public void GenerateQueryBuilder()
        {
            List<ReleaseIdentifierSubstitution> substitutions;
            var host = new QueryBuilderHost(RepositoryLocator.DataExportRepository);
            QueryBuilder = host.GetSQLCommandForFullExtractionSet(this,out substitutions);
            QueryBuilder.Sort = true;
            ReleaseIdentifierSubstitutions = substitutions;
        }

        public override string ToString()
        {
            if (this == EmptyCommand)
                return "EmptyCommand";

            return DatasetBundle.DataSet.ToString();
        }

        public DirectoryInfo GetExtractionDirectory()
        {
            if (this == EmptyCommand)
                return new DirectoryInfo(Path.GetTempPath());

            return Directory.GetDirectoryForDataset(DatasetBundle.DataSet);
        }
        public string DescribeExtractionImplementation()
        {
            return QueryBuilder.SQL;
        }
    }
}