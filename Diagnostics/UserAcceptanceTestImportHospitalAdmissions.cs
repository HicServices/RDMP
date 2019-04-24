// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Diagnostics.TestData;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.DataHelper;
using Rdmp.Core.CatalogueLibrary.FilterImporting;
using Rdmp.Core.CatalogueLibrary.FilterImporting.Construction;
using Rdmp.Core.CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace Diagnostics
{
    public class UserAcceptanceTestImportHospitalAdmissions : UserAcceptanceTest,ICheckable
    {
        private Catalogue _demographyCatalogue;
        private DiscoveredServer _dataServer;
        private DiscoveredTable _toImportTable;
        private Catalogue _catalogue;
        private ExtractionInformation _admissionDateExtractionInformation;
        private ExtractionInformation _condition1ExtractionInformation;
        private ExtractionInformation _condition2ExtractionInformation;
        private ExtractionInformation _condition3ExtractionInformation;
        private ExtractionInformation _condition4ExtractionInformation;
        private ExtractionFilter _commonConditionsFilter;
        private ExtractionFilter _specificConditionsFilter;
        private IDataAccessCredentials _demographyCredentials;

        public UserAcceptanceTestImportHospitalAdmissions(IRDMPPlatformRepositoryServiceLocator repositoryLocator) : base(repositoryLocator)
        {
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                if (null != (_demographyCatalogue = FindTestCatalogue(notifier)))
                    if (FindTableAndCleanupOldRuns(notifier))
                        if(ImportTable(notifier))
                            if (ImportLookupTable(notifier))
                                if(CreateAggregates(notifier))
                                    if(CreateConditionFilter(notifier))
                                    {
                                        notifier.OnCheckPerformed(new CheckEventArgs("Completed successfully", CheckResult.Success, null));
                                        return;
                                    }

                notifier.OnCheckPerformed(new CheckEventArgs("A stage returned false indicating that the setup should not proceed",CheckResult.Fail, null));

            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Entire process crashed", CheckResult.Fail, e));
            }
        }

        private bool CreateConditionFilter(ICheckNotifier notifier)
        {
            //does a where Condition in (select top 30 Condition from HospitalAdmissions group by Condition order by count(*) desc)
            _specificConditionsFilter = new ExtractionFilter(RepositoryLocator.CatalogueRepository, "Has Condition X", _condition1ExtractionInformation)
            {
                Description =
                    "Restricts extracted data to only those records for people suffering from a specific condition (in any of the condition fields 1-4)",
                WhereSQL = _condition1ExtractionInformation.SelectSQL + " = @condition OR " +
                           _condition2ExtractionInformation.SelectSQL + " = @condition OR " +
                           _condition3ExtractionInformation.SelectSQL + " = @condition OR " +
                           _condition4ExtractionInformation.SelectSQL + " = @condition"
            };

            var paramCreator = new ParameterCreator(new ExtractionFilterFactory(_condition1ExtractionInformation), null, null);
            paramCreator.CreateAll(_specificConditionsFilter,null);
            
            ExtractionFilterParameter parameter = _specificConditionsFilter.ExtractionFilterParameters.Single();
            parameter.Comment = "The condition you are interested the patient having";
            parameter.Value = "'V493'";
            parameter.SaveToDatabase();
            
            _specificConditionsFilter.SaveToDatabase();
            notifier.OnCheckPerformed(new CheckEventArgs("successfully created Condition Filter in Catalogue", CheckResult.Success, null));

            return true;
        }

        private bool CreateAggregates(ICheckNotifier notifier)
        {
            CreateRecordsOverTimeAggregate(notifier);
            CreateConditionsAggregate(notifier);

            return true;
        }

        private void CreateConditionsAggregate(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            //create common conditions filter
            _commonConditionsFilter = new ExtractionFilter(repository, "CommonConditions", _condition1ExtractionInformation);

            //does a where Condition in (select top 30 Condition from HospitalAdmissions group by Condition order by count(*) desc)
            TableInfo underlyingTableInfo = _condition1ExtractionInformation.ColumnInfo.TableInfo;
            _commonConditionsFilter.Description = "Restricts extracted data to the 30 most common conditions in the dataset";
            _commonConditionsFilter.WhereSQL = _condition1ExtractionInformation.SelectSQL + " IN (SELECT TOP 30 " +
                                               _condition1ExtractionInformation.SelectSQL + " FROM  " + underlyingTableInfo.Name +
                                               " GROUP BY " + _condition1ExtractionInformation.SelectSQL +
                                               " ORDER BY count(*) desc)";
            _commonConditionsFilter.SaveToDatabase();

            //now create an aggregate configuration
            var aggregateConfiguration = new AggregateConfiguration(repository, _catalogue, "Primary Conditions");
            new AggregateDimension(repository, _condition1ExtractionInformation, aggregateConfiguration);
            
            //name the count column
            aggregateConfiguration.CountSQL = "count(*) as ConditionFrequency";
            
            //create an AND container
            var container = new AggregateFilterContainer(repository, FilterContainerOperation.AND);
            
            //create a new filter and put it into the AND container
            //typically this is what happens when you import an existing normal filter and adjust it if you need to
            AggregateFilter aggregateFilter = new AggregateFilter(repository, "Common Conditions", container);
            aggregateFilter.WhereSQL = _commonConditionsFilter.WhereSQL;
            aggregateFilter.SaveToDatabase();

            //set the configurations root container to the AND container
            aggregateConfiguration.RootFilterContainer_ID = container.ID;
            aggregateConfiguration.IsExtractable = true;
            aggregateConfiguration.Description = "Relative frequency of the top 30 primary conditions (" + _condition1ExtractionInformation.SelectSQL +") in the dataset";

            //save the configuration
            aggregateConfiguration.SaveToDatabase();

            //tell user about the success
            notifier.OnCheckPerformed(new CheckEventArgs("Created 'Primary Conditions' aggregate", CheckResult.Success, null));
        }

        private void CreateRecordsOverTimeAggregate(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            var aggregateConfiguration = new AggregateConfiguration(repository, _catalogue, "Records over time");

            AggregateDimension dimension = new AggregateDimension(repository, _admissionDateExtractionInformation, aggregateConfiguration);
            AggregateContinuousDateAxis axis = new AggregateContinuousDateAxis(repository, dimension)
            {
                AxisIncrement = AxisIncrement.Year,
                StartDate = "'1901-01-01'",
                EndDate = "getdate()"
            };

            axis.SaveToDatabase();

            aggregateConfiguration.IsExtractable = true;
            aggregateConfiguration.CountSQL = "count(*) as NumberOfAdmissions";
            aggregateConfiguration.Description = "Frequency of hospital admissions (by year of admission)";
            aggregateConfiguration.SaveToDatabase();

            notifier.OnCheckPerformed(new CheckEventArgs("Created 'Records over time' aggregate", CheckResult.Success, null));
        }

        private bool ImportLookupTable(ICheckNotifier notifier)
        {
            bool carryOn = DeleteOldTableInfoCalled(TestHospitalAdmissions.LookupTableName, notifier);

            if (!carryOn)
                return false;


            TableInfoImporter importer;
            //if there are credentials use them when importing the next dataset too.
            if(_demographyCredentials != null)
                importer = new TableInfoImporter(RepositoryLocator.CatalogueRepository, _dataServer.Name, _dataServer.GetCurrentDatabase().GetRuntimeName(), TestHospitalAdmissions.LookupTableName, DatabaseType.MicrosoftSQLServer, username: _demographyCredentials.Username, password: _demographyCredentials.GetDecryptedPassword());
            else
                importer = new TableInfoImporter(RepositoryLocator.CatalogueRepository, _dataServer.Name, _dataServer.GetCurrentDatabase().GetRuntimeName(), TestHospitalAdmissions.LookupTableName, DatabaseType.MicrosoftSQLServer);
             
            TableInfo whoCaresTable;
            ColumnInfo[] whoCaresColumns;
            importer.DoImport(out whoCaresTable, out whoCaresColumns);

            return true;
        }

        private bool ImportTable(ICheckNotifier notifier)
        {
            var repository = RepositoryLocator.CatalogueRepository;
            bool carryOn = DeleteOldTableInfoCalled(TestHospitalAdmissions.HospitalAdmissionsTableName,notifier);

            if (!carryOn)
                return false;

            TableInfo tableCreated;
            ColumnInfo[] columnsCreated;

            TableInfoImporter importer;

            //if user is using username/password for this
            if(_demographyCredentials != null)
                importer = new TableInfoImporter(repository, _dataServer.Name, _dataServer.GetCurrentDatabase().GetRuntimeName(), TestHospitalAdmissions.HospitalAdmissionsTableName, DatabaseType.MicrosoftSQLServer, username: _demographyCredentials.Username, password: _demographyCredentials.GetDecryptedPassword());
            else
                importer = new TableInfoImporter(repository, _dataServer.Name, _dataServer.GetCurrentDatabase().GetRuntimeName(), TestHospitalAdmissions.HospitalAdmissionsTableName, DatabaseType.MicrosoftSQLServer);

            importer.DoImport(out tableCreated,out columnsCreated);

            //copy the same logging settings as the test catalogue
            _catalogue = new Catalogue(repository, TestHospitalAdmissions.HospitalAdmissionsTableName)
            {
                Description = TestHospitalAdmissions.DatasetDescription,
                LiveLoggingServer_ID = _demographyCatalogue.LiveLoggingServer_ID,
                LoggingDataTask = _demographyCatalogue.LoggingDataTask
            };

            _catalogue.SaveToDatabase();
            notifier.OnCheckPerformed(new CheckEventArgs("Created new Catalogue called " + _catalogue.Name, CheckResult.Success, null));

            int order = 0;
            foreach (ColumnInfo columnInfo in columnsCreated)
            {
                string colName = columnInfo.GetRuntimeName();

                CatalogueItem catalogueItem = new CatalogueItem(repository, _catalogue, colName);
                
                ExtractionInformation extractionInformation = new ExtractionInformation(repository, catalogueItem, columnInfo, columnInfo.ToString())
                {
                    Order = order
                };

                extractionInformation.SaveToDatabase();
                
                switch (colName)
                {
                    case "CHI":
                        catalogueItem.Description = TestPerson.CHIDescription;
                        extractionInformation.IsExtractionIdentifier = true;
                        extractionInformation.SaveToDatabase();
                        
                        break;
                    case "ANOCHI":
                        catalogueItem.Description = TestPerson.ANOCHIDescription;
                        extractionInformation.IsExtractionIdentifier = true;
                        extractionInformation.SaveToDatabase();
                        break;
                    case "AdmissionDate":
                        catalogueItem.Description = TestAdmission.AdmissionDateDescription;
                        _admissionDateExtractionInformation = extractionInformation;
                        //make this the time coverage field
                        _catalogue.TimeCoverage_ExtractionInformation_ID = _admissionDateExtractionInformation.ID;
                        _catalogue.SaveToDatabase();
                        break;
                    case "DischargeDate":
                        catalogueItem.Description = TestAdmission.DischargeDateDescription;
                        break;
                    case "Condition1":
                        _condition1ExtractionInformation = extractionInformation;
                        catalogueItem.Description = TestAdmission.Condition1Description;
                        break;
                    case "Condition2": 
                         catalogueItem.Description = TestAdmission.Condition2To4Description;
                         _condition2ExtractionInformation = extractionInformation;
                        break;
                    case "Condition3":
                        catalogueItem.Description = TestAdmission.Condition2To4Description;
                        _condition3ExtractionInformation = extractionInformation;
                        break;
                    case "Condition4":
                        catalogueItem.Description = TestAdmission.Condition2To4Description;
                        _condition4ExtractionInformation = extractionInformation;
                        break;
                    default :
                        throw new Exception("Could not find appropriate description for column " + colName); 

                }
                notifier.OnCheckPerformed(new CheckEventArgs("Created CatalogueItem called " + catalogueItem.Name, CheckResult.Success,null));
                catalogueItem.SaveToDatabase();
                order++;
            }

            return true;
        }

        private bool FindTableAndCleanupOldRuns(ICheckNotifier notifier)
        {

            IDataAccessPoint point;

            _dataServer = _demographyCatalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing,true, out point);
            _demographyCredentials = point.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

            _toImportTable = _dataServer.GetCurrentDatabase().DiscoverTables(false).SingleOrDefault(t => t.GetRuntimeName().Equals(TestHospitalAdmissions.HospitalAdmissionsTableName));

            if(_toImportTable == null)
                throw new Exception("Could not find table called " + TestHospitalAdmissions.HospitalAdmissionsTableName + " in database " + _dataServer);

            notifier.OnCheckPerformed(new CheckEventArgs("Found table called " + TestHospitalAdmissions.HospitalAdmissionsTableName,CheckResult.Success,null));


            //cleanup old Catalogues
            foreach (Catalogue c in RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name.Equals(TestHospitalAdmissions.HospitalAdmissionsTableName)))
                c.DeleteInDatabase();


            return true;
        }
    }
}
