// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Diagnostics.TestData;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Diagnostics
{
    public class UserAcceptanceTestDataExportFunctionality : UserAcceptanceTest,ICheckable
    {
        private readonly string _projectExtractionDirectory;
        private Catalogue _catalogue;
        ExtractionInformation[] _coreExtractionInformations;

        private int _cohortForcedIdentity = -500;
        private int _projectNumber = -999;

        /// <summary>
        /// This is the select code from the QueryBuilder to fetch data out of the DMP table
        /// </summary>
        private string _selectFromLiveSQL;
        
        private ExtractionInformation _extractionIdentifer;
        private Project _project;
        private ExtractableDataSet _extractableDataSet;
        private ExtractableDataSet _hospitalAdmissionsExtractableDataSet;

        private ExtractionConfiguration _extractionConfiguration;
        private List<IColumn> _extractableColumns;
        private IExtractableCohort _extractableCohort;
        private ExternalCohortTable _externalCohortTable;

        private bool _identifierIsANOVersion = false;

        List<string> _privateIdentifiersFound;
        DiscoveredServer _liveDataServer;
        private IDataAccessCredentials _liveDataServerCredentials;

        private string _cohortDatabaseName = "DMP_TestCohort";
        

        public UserAcceptanceTestDataExportFunctionality(string projectExtractionDirectory, IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator)
        {
            _projectExtractionDirectory = projectExtractionDirectory;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                //stop when something tells us not to continue
                if (null != (_catalogue = FindTestCatalogue(notifier)))
                   if(CheckWeCanCreateBasicSQLWithQueryBuilder(notifier))
                       if (MarkPrivateIdentifierCatalogueItemAsIsExtractionIdentifier(notifier))
                           if(FigureOutLiveServerAndReadPrivateIdentifiers(notifier))
                               if(CreateCohortDatabase(notifier))
                                   if (CreateReferenceToExternalCohortTable(notifier))
                                        if (PopulateCohortDatabaseFromCatalogueDataTable(notifier))
                                            if(ImportAsExtractableCohort(notifier))
                                                   if(CreateProjectAndExtractionConfiguration(notifier))
                                                       if(CreateHospitalAdmissionsIfPresent(notifier))
                                                            if(CheckLoggingServerHasCorrectDataTask(notifier))
                                                            {
                                                                notifier.OnCheckPerformed(new CheckEventArgs("Setup Complete",CheckResult.Success, null));
                                                                return;
                                                            }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to setup DataExport functionality for test catalogue, check Exception for details",
                    CheckResult.Fail, e));
            }
        }

        /// <summary>
        /// Populates _distinctConnectionToLiveData and _privateIdentifiersFound
        /// </summary>
        /// <param name="notifier"></param>
        /// <returns></returns>
        private bool FigureOutLiveServerAndReadPrivateIdentifiers(ICheckNotifier notifier)
        {
            //read a bunch of data from the live server to decide what cohort identifiers to use in cohort
        
            //cohort identification
            IDataAccessPoint distinctAccessPoint;
            _liveDataServer = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing,true,out distinctAccessPoint);
            _liveDataServerCredentials = distinctAccessPoint.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

            notifier.OnCheckPerformed(new CheckEventArgs(
                "Looked at the TableInfos of Catalogue " + _catalogue.Name +
                " and decided that the connection to get data from the catalogue was:" +
                _liveDataServer, CheckResult.Success, null));

            //make list of the private identifiers found (these will go into our Cohort)
            _privateIdentifiersFound = new List<string>();


            try
            {
                using (var con = _liveDataServer.GetConnection())
                {
                    con.Open();

                    //fetch data 
                    DbCommand cmdFetchData = _liveDataServer.GetCommand(_selectFromLiveSQL, con);
                    DbDataReader sqlDataReader = cmdFetchData.ExecuteReader();

                    //populate the list
                    while (sqlDataReader.Read())
                    {
                        object value = sqlDataReader[_extractionIdentifer.GetRuntimeName()];
                        if (value == DBNull.Value || value == null)
                            continue;

                        _privateIdentifiersFound.Add(value.ToString());
                    }

                    con.Close();

                    if (!_privateIdentifiersFound.Any())
                    {

                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Did not find any data in table underlying Catalogue " + _catalogue.Name +
                            " maybe you need to run the datasetLoaderUI to load the data setup by SetupTestDatasetEnvironment",
                            CheckResult.Fail, null));
                        return false;
                    }

                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Found the following private identifiers (which will be used for cohort generation) in table underlying Catalogue " +
                        _catalogue.Name + " (" +
                        _privateIdentifiersFound.Aggregate("", (s, n) => s + n + ", ").TrimEnd(' ', ',') + ")",
                        CheckResult.Success, null));
                }
            }
            catch (Exception exception)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "We were trying to read all the private identifiers from table underlying Catalogue " +
                    _catalogue.Name + " but there was a problem (see Exception for specifics)", CheckResult.Fail,
                    exception));
                return false;
            }

            return true;
        }

        private bool CreateCohortDatabase(ICheckNotifier notifier)
        {
            try
            {

                //Where {0} is the name of the Cohort database
                //Where {1} is either CHI or ANOCHI depending on whether anonymisation is enabled on the target Catalogue
                //Where {2} is either 10 or 13 -- the column length of either CHI or ANOCHI
                var database = new DiscoveredServer(_liveDataServer.Builder).ExpectDatabase(_cohortDatabaseName);

                if(database.Exists())
                    database.Drop();

                string sql = string.Format(
@"
CREATE DATABASE {0} 
GO

USE {0}

CREATE TABLE [dbo].[Cohort](
       [{1}] [char]({2}) NOT NULL,
       [ReleaseID] [char](10) NOT NULL,
       [cohortDefinition_id] [int] NOT NULL,
CONSTRAINT [PK_Cohort] PRIMARY KEY CLUSTERED 
(
       [cohortDefinition_id] ASC,
       [ReleaseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CohortDefinition](
       [id] [int] IDENTITY(1,1) NOT NULL,
       [projectNumber] [int] NOT NULL,
       [version] [int] NOT NULL,
       [description] [varchar](4000) NOT NULL,
       [dtCreated] [date] NOT NULL,
CONSTRAINT [PK_CohortDefinition] PRIMARY KEY NONCLUSTERED 
(
       [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[CohortDefinition] ADD  CONSTRAINT [DF_CohortDefinition_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO
ALTER TABLE [dbo].[Cohort]  WITH CHECK ADD  CONSTRAINT [FK_Cohort_CohortDefinition] FOREIGN KEY([cohortDefinition_id])
REFERENCES [dbo].[CohortDefinition] ([id])
GO
ALTER TABLE [dbo].[Cohort] CHECK CONSTRAINT [FK_Cohort_CohortDefinition]
GO

CREATE TABLE [dbo].[CohortCustomData](
	[cohortDefinition_id] [int] NOT NULL,
	[customTableName] [varchar](256) NOT NULL,
	[active] [bit] NOT NULL,
 CONSTRAINT [PK_CohortCustomData] PRIMARY KEY CLUSTERED 
(
	[cohortDefinition_id] ASC,
	[customTableName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CohortCustomData] ADD  CONSTRAINT [DF_CohortCustomData_active]  DEFAULT ((1)) FOR [active]
GO

ALTER TABLE [dbo].[CohortCustomData]  WITH CHECK ADD CONSTRAINT [FK_CohortCustomData_CohortDefinition] FOREIGN KEY([cohortDefinition_id])
REFERENCES [dbo].[CohortDefinition] ([id])
GO

ALTER TABLE [dbo].[CohortCustomData] CHECK CONSTRAINT [FK_CohortCustomData_CohortDefinition]
GO

",
//{0}
 _cohortDatabaseName,
 //{1}
 _identifierIsANOVersion?"ANOCHI":"CHI",
 //{2}
 _identifierIsANOVersion?"13":"10");

                using (var con = _liveDataServer.GetConnection())
                {
                    con.Open();
                    PreviewAndExecuteSQL("Recreating test Cohort database", sql, con, notifier);
                    con.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not create database called " + _cohortDatabaseName + " on server " +
                    _liveDataServer + ", see Exception for details", CheckResult.Fail, e));
                return false;
            }

        }


        private bool CheckLoggingServerHasCorrectDataTask(ICheckNotifier notifier)
        {
            if (_catalogue.LiveLoggingServer_ID == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Catalogue " + _catalogue.Name +" does not have a live logging server configured",CheckResult.Fail, null));
                return false;
            }

            try
            {
                 var lm = _catalogue.GetLogManager();

                string[] tasks = lm.ListDataTasks();

                if (!tasks.Any())
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Logging database has no DataTasks! (is database empty?), you must create a task called '" +
                        ExecuteDatasetExtractionSource.AuditTaskName + "' - and you should also have one for loading the dataset",
                        CheckResult.Fail, null));
                    return false;
                }

                if (!tasks.Contains(ExecuteDatasetExtractionSource.AuditTaskName))
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Logging database has no DataTask called '" + ExecuteDatasetExtractionSource.AuditTaskName +"'",
                        CheckResult.Fail, null));
                    return false;
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "A problem occurred when trying to check the logging server for the '" +
                    ExecuteDatasetExtractionSource.AuditTaskName + "' task, see Exception for details", CheckResult.Fail, e));
                return false;
            }

            notifier.OnCheckPerformed(new CheckEventArgs(
                "Correctly identified logging task " + ExecuteDatasetExtractionSource.AuditTaskName + " in logging database",
                CheckResult.Success, null));

            return true;
        }

        private bool CreateProjectAndExtractionConfiguration(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.DataExportRepository;
            Project projectToCleanUp = repository.GetAllObjectsWhere<Project>("ProjectNumber" , _projectNumber).SingleOrDefault();

            if(projectToCleanUp != null)
            {
                try
                {
                    
                    //extractable dataset
                    var toCleanupDataset = repository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).SingleOrDefault();
                 
                    if(toCleanupDataset != null)
                    {
                        //delete configurations
                        foreach (var extractionConfiguration in projectToCleanUp.ExtractionConfigurations)
                        {
                            //delete columns
                            foreach (ExtractableColumn extractableColumn in extractionConfiguration.GetAllExtractableColumnsFor(toCleanupDataset))
                            {

                                extractableColumn.DeleteInDatabase();

                                notifier.OnCheckPerformed(new CheckEventArgs(
                                    "Deleted old reference to ExtractableColumn " + extractableColumn.SelectSQL,
                                    CheckResult.Success));
                            }

                            //unselect datasets
                            extractionConfiguration.RemoveDatasetFromConfiguration(toCleanupDataset);

                            //delete configuration
                            ((ExtractionConfiguration)extractionConfiguration).DeleteInDatabase();

                        }
                        
                        //delete dataset
                        toCleanupDataset.DeleteInDatabase();
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            "Deleted old reference to ExtractableDataset " + toCleanupDataset,
                            CheckResult.Success, null));
                    }

                    //delete project
                    projectToCleanUp.DeleteInDatabase();

                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Deleted old reference to project " + projectToCleanUp.Name + " (and any ExtractionConfigurations)",
                        CheckResult.Success, null));
   
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Problem occurred when trying to delete old reference to project/dataset " + projectToCleanUp.Name,
                        CheckResult.Fail, e));
                    return false;
                }
            }

            try
            {
                _project = new Project(repository, "DMP_TestProject")
                {
                    ProjectNumber = _projectNumber,
                    ExtractionDirectory = _projectExtractionDirectory
                };
                _project.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs("Created project reference " + _project.Name, CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not create new Project in DataExportManager", CheckResult.Fail, e));
                return false;
            }
            try
            {
                _extractableDataSet = new ExtractableDataSet(repository,_catalogue);
                _extractableDataSet.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs("Added reference to Catalogue " + _catalogue.Name + ", making it an ExtractableDataset.  The ID of the ExtractableDataset created was " + _extractableDataSet.ID, CheckResult.Success, null));

                _extractionConfiguration = new ExtractionConfiguration(repository, _project)
                {
                    Cohort_ID = _extractableCohort.ID
                };
                _extractionConfiguration.SaveToDatabase();
                notifier.OnCheckPerformed(new CheckEventArgs("Created new ExtractionConfiguration for project " + _project.Name,CheckResult.Success, null));

                var selectedDataSet = new SelectedDataSets(RepositoryLocator.DataExportRepository,_extractionConfiguration,_extractableDataSet,null);
                notifier.OnCheckPerformed(new CheckEventArgs("Added ExtractableDataset "+ _extractableDataSet + " as a SelectedDataSet of project " + _project.Name, CheckResult.Success, null));
                
                _extractableColumns = new List<IColumn>();
                foreach (ExtractionInformation catalogueExtractable in _coreExtractionInformations)
                {
                    ExtractableColumn importedExtractableColumn = new ExtractableColumn(repository, _extractableDataSet, _extractionConfiguration, catalogueExtractable, catalogueExtractable.Order, catalogueExtractable.SelectSQL)
                    {
                        Alias = catalogueExtractable.Alias,
                        IsExtractionIdentifier = catalogueExtractable.IsExtractionIdentifier,
                        Order = catalogueExtractable.Order
                    };
                    importedExtractableColumn.SaveToDatabase();

                    _extractableColumns.Add(importedExtractableColumn);

                    notifier.OnCheckPerformed(new CheckEventArgs(
                        "Imported column " + catalogueExtractable.SelectSQL +
                        " from Catalogue into Extraction Configuration (it is now a copy of the catalogue one - an ExtractableColumn as opposed to an ExtractionInformation)",
                        CheckResult.Success
                        , null));
                }

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Problem occurred building the extraction configuration (or selecting the dataset/columns to extract)",CheckResult.Fail, e));
            }

            try
            {
                ExtractDatasetCommand request = new ExtractDatasetCommand(_extractionConfiguration,_extractableCohort,new ExtractableDatasetBundle(_extractableDataSet),_extractableColumns,new HICProjectSalt(_project), null);
                request.GenerateQueryBuilder();

                ISqlQueryBuilder extractionCommand = request.QueryBuilder;

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionQueryBuilder has decided that the extraction SQL is:" + extractionCommand.SQL,
                    CheckResult.Success, null));
                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionQueryBuilder could not build extraction SQL for the configuration, see Exception for details",
                    CheckResult.Fail, e));
                return false;

            }
        }

        private bool CreateHospitalAdmissionsIfPresent(ICheckNotifier notifier)
        {
            Catalogue hospitalAdmissionsCatalogue = RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .SingleOrDefault(c => c.Name.Equals(TestHospitalAdmissions.HospitalAdmissionsTableName));

            if (hospitalAdmissionsCatalogue == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Did not find hospital admissions catalogue so skipping importing it into DataExportManager",
                    CheckResult.Warning, null));
                return true;
            }

            var repository = RepositoryLocator.DataExportRepository;
            if (repository.GetAllObjects<ExtractableDataSet>()
                .Any(e => e.Catalogue_ID == hospitalAdmissionsCatalogue.ID))
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Hospital admissions catalogue is already in DataExportManager so skipping importing it",
                    CheckResult.Warning, null));
                return true;
            }

            //create it
            _hospitalAdmissionsExtractableDataSet = new ExtractableDataSet(repository,hospitalAdmissionsCatalogue);
            notifier.OnCheckPerformed(new CheckEventArgs("Added reference to Catalogue " + hospitalAdmissionsCatalogue.Name + ", making it an ExtractableDataset.  The ID of the ExtractableDataset created was " + _hospitalAdmissionsExtractableDataSet.ID, CheckResult.Success));


            new SelectedDataSets(RepositoryLocator.DataExportRepository,_extractionConfiguration, _hospitalAdmissionsExtractableDataSet,null);

            notifier.OnCheckPerformed(new CheckEventArgs("Added ExtractableDataset " + _hospitalAdmissionsExtractableDataSet + " as a SelectedDataSet of project " + _project.Name, CheckResult.Success));

            
            foreach (ExtractionInformation catalogueExtractable in hospitalAdmissionsCatalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            {
                ExtractableColumn importedExtractableColumn = new ExtractableColumn(repository, _hospitalAdmissionsExtractableDataSet, _extractionConfiguration, catalogueExtractable, catalogueExtractable.Order, catalogueExtractable.SelectSQL)
                {
                    Alias = catalogueExtractable.Alias,
                    IsExtractionIdentifier = catalogueExtractable.IsExtractionIdentifier,
                    Order = catalogueExtractable.Order
                };

                importedExtractableColumn.SaveToDatabase();

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Imported column " + catalogueExtractable.SelectSQL +
                    " from Catalogue into Extraction Configuration (it is now a copy of the catalogue one - an ExtractableColumn as opposed to an ExtractionInformation)",
                    CheckResult.Success
                    , null));
            }

            return true;
        }

        private bool CreateReferenceToExternalCohortTable(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.DataExportRepository;
            try
            {
                ExternalCohortTable toCleanup = repository.GetAllObjectsWhere<ExternalCohortTable>("Name",_cohortDatabaseName).SingleOrDefault();
                if(toCleanup != null)
                {
                    toCleanup.DeleteInDatabase();
                    notifier.OnCheckPerformed(new CheckEventArgs("Cleaned up old reference to " + _cohortDatabaseName + " in ExternalCohortTable",CheckResult.Success, null));
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Problem occurred when trying to cleanup old references to " + _cohortDatabaseName +
                    " in ExternalCohortTable", CheckResult.Fail, e));
                
                return false;
            }

            try
            {
                _externalCohortTable = new ExternalCohortTable(repository, "TestExternalCohort", DatabaseType.MicrosoftSQLServer)
                {
                    Server = _liveDataServer.Name,
                    Database = _cohortDatabaseName,
                    TableName = "Cohort",
                    DefinitionTableName = "CohortDefinition",
                    ReleaseIdentifierField = "ReleaseID",
                    PrivateIdentifierField = _identifierIsANOVersion ? "ANOCHI" : "CHI",
                    DefinitionTableForeignKeyField = "cohortDefinition_id"
                };

                //if the distinct live connection has a password the same password should apply to the cohort database
                if (_liveDataServerCredentials != null)
                {
                    _externalCohortTable.Username = _liveDataServerCredentials.Username;
                    _externalCohortTable.Password = _liveDataServerCredentials.GetDecryptedPassword();
                }

                _externalCohortTable.Name = "Test Cohort Database (UserAcceptanceTest Created This)";
                _externalCohortTable.SaveToDatabase();
                
                notifier.OnCheckPerformed(new CheckEventArgs(
                   "Created new references to ExternalCohortTable " + _cohortDatabaseName, CheckResult.Success, null));

                return true;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                   "Problem occurred when trying to create new references to ExternalCohortTable " + _cohortDatabaseName, CheckResult.Fail, e));

                return false;
            }
        }

        private bool ImportAsExtractableCohort(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.DataExportRepository;
            ExtractableCohort oldExtractableCohort = repository.GetAllObjectsWhere<ExtractableCohort>("OriginID" ,_cohortForcedIdentity).SingleOrDefault();

            try
            {
                if(oldExtractableCohort != null)
                {
                    ExternalCohortTable oldExternalCohortTableSource = repository.GetObjectByID<ExternalCohortTable>(oldExtractableCohort.ExternalCohortTable_ID);
                    ExtractableCohort[] otherCohortsUsingOldSource = repository.GetAllObjectsWhere<ExtractableCohort>("ExternalCohortTable_ID" , oldExternalCohortTableSource.ID).ToArray();

                    if (otherCohortsUsingOldSource.Length > 1)
                        throw new Exception("Could not cleanup old reference to an external cohort table with ID=" + oldExternalCohortTableSource.ID + " because it is used by the following cohorts: " + otherCohortsUsingOldSource.Aggregate("", (s, n) => s + "'" + n +"' ") + " you must delete these cohorts before you can delete the source");

                    //delete the cohort
                    oldExtractableCohort.DeleteInDatabase();

                    //delete the source
                    oldExternalCohortTableSource.DeleteInDatabase();
                }
            }
            catch (Exception e)
            {
                //importing a cohort doesn't do much
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Failed to delete existing ExtractableCohort with OriginID " + _cohortForcedIdentity +
                    " (possibly it is used in an existing ExtractionConfiguration?)", CheckResult.Warning, e));
                return true;
            }

            _extractableCohort = new ExtractableCohort(repository, _externalCohortTable, _cohortForcedIdentity);

            notifier.OnCheckPerformed(new CheckEventArgs("Imported Cohort into DataExportManager",CheckResult.Success));
            return true;
        }

        private bool PopulateCohortDatabaseFromCatalogueDataTable(ICheckNotifier notifier)
        {
            try
            {
                using (var con = _liveDataServer.GetConnection())
                {
                    con.Open();
                    
                    //create a delete In statement with every private identifier in the list
                    string deleteSql = "DELETE FROM " + _externalCohortTable.TableName + " where cohortDefinition_id = " + _cohortForcedIdentity;

                    PreviewAndExecuteSQL("Clear any previous records from Cohort table", deleteSql, con, notifier);

                    //delete the definition in the CohortDefinitionsTable
                    deleteSql = "DELETE FROM " + _externalCohortTable.DefinitionTableName + " where id=" + _cohortForcedIdentity;

                    PreviewAndExecuteSQL("Clear any previous records from Cohort definition table", deleteSql, con, notifier);


                    string insertSQL = "SET IDENTITY_INSERT " + _externalCohortTable.DefinitionTableName + " ON ;" + Environment.NewLine;
                    insertSQL += "INSERT INTO " + _externalCohortTable.DefinitionTableName +
                                 " (id,projectNumber,description,version) VALUES (" + _cohortForcedIdentity + "," +
                                 _projectNumber + ",'DMP_TestCohort',1)";


                    PreviewAndExecuteSQL("Add Cohort Definition", insertSQL, con, notifier);

                    con.Close();
                }

                HashSet<string> alreadyUsedIdentifiers = new HashSet<string>();

                
                //here we just invent some random strings to go into the PRO level identifier randomisation.  The reason that we do this rather than call a stored procedure
                //is because unlike ANOTable style anonymisation (data load) this PRO stuff has not been implemented yet within HIC so is supported with a user customisable 
                //framework where they have to write their own randomisation routine and all we care about is that there is a mapping table that contains the private (ANO)
                //identifiers and the public (PRO) release identifiers
                foreach (string identifiersToInsert in _privateIdentifiersFound)
                {
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    string result;

                    do
                    {
                        result = new string(
                            Enumerable.Repeat(chars, 8)
                                .Select(s => s[random.Next(s.Length)])
                                .ToArray());

                    } while (alreadyUsedIdentifiers.Contains(result));//avoid collisions
                    

                    alreadyUsedIdentifiers.Add(result);

                    InsertIntoCohortTable(identifiersToInsert, result);
                    notifier.OnCheckPerformed(new CheckEventArgs("Inserted into cohort database " + identifiersToInsert + "," + result, CheckResult.Success, null));
                }

                return true;

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Problem occurred when trying to populate Cohort database:" + e.Message,
                    CheckResult.Fail, e));
                return false;
            }
            
        }


        

        private bool MarkPrivateIdentifierCatalogueItemAsIsExtractionIdentifier(ICheckNotifier notifier)
        {

           _extractionIdentifer = _coreExtractionInformations.SingleOrDefault(e => e.GetRuntimeName().Equals("CHI") || e.GetRuntimeName().Equals("ANOCHI"));

            if (_extractionIdentifer != null)
            {
                _extractionIdentifer.IsExtractionIdentifier = true;
                _extractionIdentifer.SaveToDatabase();

                if (_extractionIdentifer.GetRuntimeName().Equals("ANOCHI"))
                    _identifierIsANOVersion = true;
                else
                    _identifierIsANOVersion = false;

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Found column called " + _extractionIdentifer.GetRuntimeName() + " so making it release identififer",
                    CheckResult.Success, null));
            }
            else
            {

                notifier.OnCheckPerformed(new CheckEventArgs(
                    "Could not find a column called either CHI or ANOCHI so don't know what field should be the release identifier ",
                    CheckResult.Fail, null));
                return false;
            }

            return true;
        }

        private bool CheckWeCanCreateBasicSQLWithQueryBuilder(ICheckNotifier notifier)
        {
            try
            {
                //Create a query builder and give it all columns from the Catalogue (this is the extractio SQL before it is linked to a cohort and should have already been setup by SetupTestEnvironment
                QueryBuilder qb = new QueryBuilder(null,
                    RepositoryLocator.DataExportRepository.DataExportPropertyManager.GetValue(DataExportProperty.HashingAlgorithmPattern)
                    );

                
                _coreExtractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Core);
                qb.AddColumnRange(_coreExtractionInformations);

                //this Property will actually make QueryBuilder go regenerate the SQL code from the extractionInformation columns
                _selectFromLiveSQL = qb.SQL;

                //tell user what the SQL is
                notifier.OnCheckPerformed(new CheckEventArgs("Current SQL configured for " + _catalogue.Name + " is " + _selectFromLiveSQL,
                    CheckResult.Success, null));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    "ExtractionInformation in Catalogue " + _catalogue.Name +
                    " could not be turned into valid SQL by QueryBuilder.  Check Exception for details",
                    CheckResult.Fail, e));
                
                return false;
            }

            return true;
        }
        private void InsertIntoCohortTable(string privateID, string publicID)
        {
            using(DbConnection con = _liveDataServer.GetConnection())
            {
                con.Open();
                if(string.IsNullOrWhiteSpace(_externalCohortTable.DefinitionTableForeignKeyField))
                    throw new NullReferenceException("_externalCohortTable.DefinitionTableForeignKeyField not set");

                string insertIntoList = "INSERT INTO " + _externalCohortTable.TableName + "(" + _externalCohortTable.PrivateIdentifierField + "," +
                                        _externalCohortTable.ReleaseIdentifierField + "," + _externalCohortTable.DefinitionTableForeignKeyField + ")" + Environment.NewLine;


                string insertValues = " VALUES ('" + privateID + "','" + publicID + "'," + _cohortForcedIdentity + ")";

                DbCommand insertRecord = _liveDataServer.GetCommand(insertIntoList + insertValues, con);

                insertRecord.ExecuteNonQuery();
            }
        }
    }
}