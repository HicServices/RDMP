// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using FAnsi;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.Databases;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataRelease;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.DataLoad.Modules.DataFlowOperations;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation;
using ReusableLibraryCode.Checks;

namespace Tests.Common
{
    /// <summary>
    /// Base class for all tests that want to create objects only in memory (and not in database like <see cref="DatabaseTests"/>)
    /// </summary>
    [Category("Unit")]
    public class UnitTests
    {
        protected MemoryDataExportRepository Repository = new MemoryDataExportRepository();
        protected IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }
        
        //These types do not have to be supported by the method WhenIHaveA
        protected HashSet<string> SkipTheseTypes = new HashSet<string>(new string[]
        {
            "TestColumn",
            "ExtractableCohort",
            "DQEGraphAnnotation",
            "Evaluation",
            "WindowLayout",
        });
        
        
        public UnitTests()
        {
            RepositoryLocator = new RepositoryProvider(Repository);
        }


        /// <summary>
        /// Override to do stuff before your first instance is constructed
        /// </summary>
        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            ImplementationManager.Load<MicrosoftSQLImplementation>();
            ImplementationManager.Load<MySqlImplementation>();
            ImplementationManager.Load<OracleImplementation>();
            ImplementationManager.Load<PostgreSqlImplementation>();
        }

        /// <summary>
        /// Loads FAnsi implementations for all supported DBMS platforms into memory
        /// </summary>
        [SetUp]
        protected virtual void SetUp()
        {
        }

        ///
        /// Sanity check the .Net assembly checker
        ///
        [Test]
        public static void TestIsDotNetAssembly()
        {
            Assert.True(SafeDirectoryCatalog.IsDotNetAssembly(Path.Combine(
                RuntimeEnvironment.GetRuntimeDirectory(),
                "System.Net.dll")));
        }

        /// <summary>
        /// Creates a minimum viable object of Type T.  This includes the object and any dependencies e.g. a 
        /// <see cref="ColumnInfo"/> cannot exist without a <see cref="TableInfo"/>.  
        /// </summary>
        /// <typeparam name="T">Type of object you want to create</typeparam>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">If there is not yet an implementation for the given T.  Feel free to write one.</exception>
        protected T WhenIHaveA<T>() where T : DatabaseEntity
        {
            if (typeof(T) == typeof(Catalogue))
                return (T)(object)Save(new Catalogue(Repository, "Mycata"));

            
            if (typeof(T) == typeof(ExtendedProperty))
            {
                return (T)(object)new ExtendedProperty(Repository,Save(new Catalogue(Repository, "Mycata")),"TestProp",0);
            }
                

            if (typeof(T) == typeof(CatalogueItem))
            {
                var cata = new Catalogue(Repository, "Mycata");
                return (T)(object)new CatalogueItem(Repository, cata, "MyCataItem");
            }

            if (typeof(T) == typeof(ExtractionInformation))
            {
                var col = WhenIHaveA<ColumnInfo>();

                var cata = new Catalogue(Repository, "Mycata");
                var ci = new CatalogueItem(Repository, cata, "MyCataItem");
                var ei = new ExtractionInformation(Repository, ci, col, "MyCataItem");
                return (T)(object)Save(ei);
            }

            if (typeof(T) == typeof(TableInfo))
            {
                var table = new TableInfo(Repository, "My_Table"){DatabaseType = DatabaseType.MicrosoftSQLServer};
                return (T)(object)table;
            }

            if (typeof(T) == typeof(ColumnInfo))
            {
                var ti = WhenIHaveA<TableInfo>();
                var col = new ColumnInfo(Repository, "My_Col", "varchar(10)", ti);
                return (T)(object)col;
            }

            if (typeof(T) == typeof(AggregateConfiguration))
            {
                ExtractionInformation dateEi;
                ExtractionInformation otherEi;
                return (T)(object)WhenIHaveA<AggregateConfiguration>(out dateEi, out otherEi);
            }

            if (typeof(T) == typeof(ExternalDatabaseServer))
            {
                return (T)(object)Save(new ExternalDatabaseServer(Repository, "My Server",null));
            }

            if (typeof(T) == typeof(ANOTable))
            {
                ExternalDatabaseServer server;
                return (T)(object)WhenIHaveA<ANOTable>(out server);
            }

            if (typeof(T) == typeof(LoadMetadata))
            {
                //creates the table, column, catalogue, catalogue item and extraction information
                var ei = WhenIHaveA<ExtractionInformation>();
                var cata = ei.CatalogueItem.Catalogue;

                var ti = ei.ColumnInfo.TableInfo;
                ti.Server = "localhost";
                ti.Database = "mydb";
                ti.SaveToDatabase();

                var lmd = new LoadMetadata(Repository, "MyLoad");
                cata.LoadMetadata_ID = lmd.ID;
                cata.SaveToDatabase();
                return (T)(object)lmd;
            }

            if (typeof (T) == typeof (AggregateTopX))
            {
                var agg = WhenIHaveA<AggregateConfiguration>();
                return (T)(object) new AggregateTopX(Repository, agg, 10);
            }

            if (typeof (T) == typeof (ConnectionStringKeyword))
            {
                return (T)(object)new ConnectionStringKeyword(Repository, DatabaseType.MicrosoftSQLServer, "MultipleActiveResultSets", "true");
            }

            if (typeof (T) == typeof (DashboardLayout))
                return (T)(object)new DashboardLayout(Repository, "My Layout");

            if (typeof(T) == typeof(DashboardControl))
            {
                var layout = WhenIHaveA<DashboardLayout>();
                return (T)(object)Save(new DashboardControl(Repository, layout, typeof(int), 0, 0, 100, 100, "") { ControlType = "GoodBadCataloguePieChart" });
            }

            if (typeof(T) == typeof(DashboardObjectUse))
            {
                var layout = WhenIHaveA<DashboardLayout>();
                var control = Save(new DashboardControl(Repository, layout, typeof(int), 0, 0, 100, 100, "") { ControlType = "GoodBadCataloguePieChart" });
                var use = new DashboardObjectUse(Repository, control, WhenIHaveA<Catalogue>());
                return (T)(object)use;
            }

            if (typeof(T) == typeof(ExtractionFilter))
            {
                var ei = WhenIHaveA<ExtractionInformation>();
                return (T)(object)new ExtractionFilter(Repository, "My Filter", ei);
            }

            if (typeof(T) == typeof(ExtractionFilterParameter))
            {
                var filter = WhenIHaveA<ExtractionFilter>();
                filter.WhereSQL = "@myParam = 'T'";

                return (T)(object)new ExtractionFilterParameter(Repository,"DECLARE @myParam varchar(10)",filter);
            }

            if (typeof(T) == typeof(ExtractionFilterParameterSetValue))
            {
                var parameter = WhenIHaveA<ExtractionFilterParameter>();
                var set = new ExtractionFilterParameterSet(Repository, parameter.ExtractionFilter, "Parameter Set");
                return (T)(object)new ExtractionFilterParameterSetValue(Repository, set, parameter);
            }

            if (typeof (T) == typeof(ExtractionFilterParameterSet))
            {
                return (T)(object)WhenIHaveA<ExtractionFilterParameterSetValue>().ExtractionFilterParameterSet;
            }

            if (typeof (T) == typeof (Favourite))
                return (T) (object) new Favourite(Repository, WhenIHaveA<Catalogue>());

            if (typeof (T) == typeof(ObjectExport))
            {
                ShareManager sm;
                return (T)(object)WhenIHaveA<ObjectExport>(out sm);
            }
            
            if (typeof (T) == typeof(ObjectImport))
            {
                ShareManager sm;
                ObjectExport export = WhenIHaveA<ObjectExport>(out sm);
                return (T)(object)sm.GetImportAs(export.SharingUID, WhenIHaveA<Catalogue>());
            }
            
            if (typeof (T) == typeof(WindowLayout))
                return (T)(object)new WindowLayout(Repository,"My window arrangement","<html><body>ignore this</body></html>");

            if (typeof (T) == typeof(RemoteRDMP))
                return (T)(object)new RemoteRDMP(Repository);
            
            if (typeof (T) == typeof(CohortIdentificationConfiguration))
                return (T)(object)new CohortIdentificationConfiguration(Repository,"My cic");

            if (typeof (T) == typeof(JoinableCohortAggregateConfiguration))
            {
                var config = WhenIHaveCohortAggregateConfiguration("PatientIndexTable");
                var cic = WhenIHaveA<CohortIdentificationConfiguration>();
                cic.EnsureNamingConvention(config);
                return (T)(object)new JoinableCohortAggregateConfiguration(Repository,cic,config);
            }
            
            if (typeof (T) == typeof(JoinableCohortAggregateConfigurationUse))
            {
                var joinable = WhenIHaveA<JoinableCohortAggregateConfiguration>();
                var config = WhenIHaveCohortAggregateConfiguration("Aggregate");
                return (T)(object)joinable.AddUser(config);
            }
            
            if (typeof (T) == typeof(Rdmp.Core.Curation.Data.Plugin))
                return (T)(object)new Rdmp.Core.Curation.Data.Plugin(Repository,new FileInfo("bob.nupkg"),new Version(1,1,1),new Version(1,1,1));
            
            if (typeof (T) == typeof(LoadModuleAssembly))
            {
                var dll = Path.Combine(TestContext.CurrentContext.TestDirectory,"a.nupkg");
                File.WriteAllBytes(dll,new byte[] {0x11});

                return (T)(object)new LoadModuleAssembly(Repository,new FileInfo(dll),WhenIHaveA<Rdmp.Core.Curation.Data.Plugin>());
            }
            
            if (typeof (T) == typeof(AggregateContinuousDateAxis))
            {
                ExtractionInformation dateEi;
                ExtractionInformation otherEi;
                var config = WhenIHaveA<AggregateConfiguration>(out dateEi,out otherEi);
                
                //remove the other Ei
                config.AggregateDimensions[0].DeleteInDatabase();
                //add the date one
                var dim = new AggregateDimension(Repository, dateEi, config);

                return (T)(object)new AggregateContinuousDateAxis(Repository,dim);
            }
            
            if (typeof (T) == typeof(AggregateDimension))
                return (T)(object) WhenIHaveA<AggregateConfiguration>().AggregateDimensions[0];
            
            if (typeof (T) == typeof(AggregateFilterContainer))
            {
                var config = WhenIHaveA<AggregateConfiguration>();
                var container = new AggregateFilterContainer(Repository,FilterContainerOperation.AND);
                config.RootFilterContainer_ID = container.ID;
                config.SaveToDatabase();
                return (T) (object) container;
            }
            if (typeof (T) == typeof(AggregateFilter))
            {
                var container = WhenIHaveA<AggregateFilterContainer>();
                return (T)(object)new AggregateFilter(Repository,"My Filter",container);
            }
            
            if (typeof (T) == typeof(AggregateFilterParameter))
            {
                var filter = WhenIHaveA<AggregateFilter>();
                filter.WhereSQL = "@MyP = 'mnnn apples'";
                filter.SaveToDatabase();

                return (T)filter.GetFilterFactory().CreateNewParameter(filter, "DECLARE @MyP as varchar(10)");
            }

            if (typeof (T) == typeof (LoadProgress))
                return (T) (object) new LoadProgress(Repository, WhenIHaveA<LoadMetadata>());

            if (typeof (T) == typeof(CacheProgress))
                return (T)(object)new CacheProgress(Repository,WhenIHaveA<LoadProgress>());

            if (typeof (T) == typeof(CacheFetchFailure))
                return (T)(object)new CacheFetchFailure(Repository,WhenIHaveA<CacheProgress>(),DateTime.Now.Subtract(new TimeSpan(1,0,0,0)),DateTime.Now,new Exception("It didn't work"));
            
            if (typeof (T) == typeof(CohortAggregateContainer))
            {
                var cic = WhenIHaveA<CohortIdentificationConfiguration>();
                cic.CreateRootContainerIfNotExists();
                return (T)(object)cic.RootCohortAggregateContainer;
            }
            
            if (typeof (T) == typeof(AnyTableSqlParameter))
            {
                var cic = WhenIHaveA<CohortIdentificationConfiguration>();
                return (T)(object)new AnyTableSqlParameter(Repository,cic,"DECLARE @myGlobal as varchar(10)");
            }

            if (typeof (T) == typeof(DataAccessCredentials))
                return (T)(object)new DataAccessCredentials(Repository,"My credentials");
            
            if (typeof (T) == typeof(GovernancePeriod))
                return (T)(object)new GovernancePeriod(Repository);
            
            if (typeof (T) == typeof(GovernanceDocument))
            {
                var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "myfile.txt"));
                return (T)(object)new GovernanceDocument(Repository,WhenIHaveA<GovernancePeriod>(),fi);
            }

            if (typeof (T) == typeof(PermissionWindow))
                return (T)(object)new PermissionWindow(Repository);
            

            if (typeof (T) == typeof(JoinInfo))
            {
                ColumnInfo col1;
                ColumnInfo col2;
                ColumnInfo col3;
                WhenIHaveTwoTables(out col1,out col2,out col3);
                
                return (T)(object)new JoinInfo(Repository,col1,col2,ExtractionJoinType.Left, null);
            }
            if (typeof (T) == typeof(Lookup))
            {
                ColumnInfo col1;
                ColumnInfo col2;
                ColumnInfo col3;
                WhenIHaveTwoTables(out col1,out col2,out col3);
                
                return (T)(object)new Lookup(Repository,col3,col1,col2,ExtractionJoinType.Left, null);
            }
            if (typeof (T) == typeof(LookupCompositeJoinInfo))
            {
                var lookup = WhenIHaveA<Lookup>();

                var otherJoinFk = new ColumnInfo(Repository,"otherJoinKeyForeign","int",lookup.ForeignKey.TableInfo);
                var otherJoinPk = new ColumnInfo(Repository,"otherJoinKeyPrimary","int",lookup.PrimaryKey.TableInfo);
                
                return (T)(object)new LookupCompositeJoinInfo(Repository,lookup,otherJoinFk,otherJoinPk);
            }
            if (typeof (T) == typeof(Pipeline))
                return (T)(object)new Pipeline(Repository,"My Pipeline");

            if (typeof (T) == typeof(PipelineComponent))
                return (T)(object)new PipelineComponent(Repository, WhenIHaveA<Pipeline>(), typeof(ColumnBlacklister),0,"My Component");
            
            if (typeof (T) == typeof(PipelineComponentArgument))
            {
                var comp = WhenIHaveA<PipelineComponent>();
                return (T)comp.CreateArgumentsForClassIfNotExists<ColumnBlacklister>().First();
            }

            if (typeof (T) == typeof(PreLoadDiscardedColumn))
                return (T)(object)new PreLoadDiscardedColumn(Repository,WhenIHaveA<TableInfo>(),"MyDiscardedColum");
                        
                       
            if (typeof (T) == typeof(ProcessTask))
                return (T)(object)new ProcessTask(Repository,WhenIHaveA<LoadMetadata>(),LoadStage.AdjustRaw);

            if (typeof (T) == typeof(ProcessTaskArgument))
                return (T)(object)new ProcessTaskArgument(Repository,WhenIHaveA<ProcessTask>());

                
            if (typeof (T) == typeof(StandardRegex))
                return (T)(object)new StandardRegex(Repository);

            if (typeof (T) == typeof(SupportingSQLTable))
                return (T)(object)new SupportingSQLTable(Repository,WhenIHaveA<Catalogue>(),"Some Handy Query");

            if (typeof (T) == typeof(TicketingSystemConfiguration))
                return (T)(object)new TicketingSystemConfiguration(Repository,"My Ticketing System");

            if (typeof (T) == typeof(SupportingDocument))
                return (T)(object)new SupportingDocument(Repository,WhenIHaveA<Catalogue>(),"HelpFile.docx");

            if (typeof (T) == typeof(Project))
                return (T)(object)new Project(Repository,"My Project");
            
            if (typeof (T) == typeof(ExtractionConfiguration))
                return (T)(object)new ExtractionConfiguration(Repository,WhenIHaveA<Project>());
            
            if (typeof (T) == typeof(ExtractableDataSet))
            {
                //To make an extractable dataset we need an extraction identifier (e.g. chi) that will be linked in the cohort
                var ei = WhenIHaveA<ExtractionInformation>();
                ei.IsExtractionIdentifier = true;
                ei.SaveToDatabase();

                //And we need another column too just for sanity sakes (in the same table)
                var ci2 = new CatalogueItem(Repository,ei.CatalogueItem.Catalogue,"ci2");
                var col2 = new ColumnInfo(Repository, "My_Col2", "varchar(10)", ei.ColumnInfo.TableInfo);
                var ei2 = new ExtractionInformation(Repository,ci2,col2,col2.GetFullyQualifiedName());

                return (T)(object)new ExtractableDataSet(Repository,ei.CatalogueItem.Catalogue);
            }
            
            if (typeof (T) == typeof(CumulativeExtractionResults))
                return (T)(object)new CumulativeExtractionResults(Repository,WhenIHaveA<ExtractionConfiguration>(),WhenIHaveA<ExtractableDataSet>(),"SELECT * FROM Anywhere");
            
            if (typeof (T) == typeof(SelectedDataSets))
            {
                var eds = WhenIHaveA<ExtractableDataSet>();
                var config = WhenIHaveA<ExtractionConfiguration>();

                foreach (var ei in eds.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
                {
                    var ec = new ExtractableColumn(Repository, eds, config,ei,ei.Order,ei.SelectSQL);
                }

                return (T)(object)new SelectedDataSets(Repository,config,eds, null);
            }
                

            if (typeof (T) == typeof(ReleaseLog))
            {
                var file = Path.Combine(TestContext.CurrentContext.TestDirectory,"myDataset.csv");
                File.WriteAllText(file,"omg rows");
                
                var sds = WhenIHaveA<SelectedDataSets>();
                new CumulativeExtractionResults(Repository,sds.ExtractionConfiguration,sds.ExtractableDataSet,"SELECT * FROM ANYWHERE");
                var potential = new FlatFileReleasePotential(new RepositoryProvider(Repository), sds);

                return (T)(object)new ReleaseLog(Repository,
                    potential,
                    new ReleaseEnvironmentPotential(sds.ExtractionConfiguration),
                    false,
                    new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                    new FileInfo(file));
                        
            }

            if (typeof (T) == typeof(ExtractableDataSetPackage))
                return (T)(object)new ExtractableDataSetPackage(Repository,"My Cool Package");
            
            
            if (typeof (T) == typeof(SupplementalExtractionResults))
            {
                return (T)(object)new SupplementalExtractionResults(Repository,WhenIHaveA<CumulativeExtractionResults>(),"Select * from Lookup",WhenIHaveA<SupportingSQLTable>());
            }

            if (typeof (T) == typeof(SelectedDataSetsForcedJoin))
                return (T)(object)new SelectedDataSetsForcedJoin(Repository,WhenIHaveA<SelectedDataSets>(),WhenIHaveA<TableInfo>());
            
            if (typeof (T) == typeof(ProjectCohortIdentificationConfigurationAssociation))
                return (T)(object)new ProjectCohortIdentificationConfigurationAssociation(Repository,WhenIHaveA<Project>(),WhenIHaveA<CohortIdentificationConfiguration>());
            
            if (typeof (T) == typeof(ExternalCohortTable))
                return Save((T)(object)new ExternalCohortTable(Repository,"My cohorts",DatabaseType.MicrosoftSQLServer)
                {
                    Database="MyCohortsDb",
                    DefinitionTableForeignKeyField = "c_id",
                    PrivateIdentifierField = "priv",
                    ReleaseIdentifierField = "rel",
                    TableName = "Cohorts",
                    DefinitionTableName = "InventoryTable",
                    Server = "localhost\\sqlexpress"
                });

            if (typeof (T) == typeof(ExtractableCohort))
                throw new NotSupportedException("You should inherit from TestsRequiringACohort instead, cohorts have to exist to be constructed");

            if (typeof (T) == typeof(GlobalExtractionFilterParameter))
                return (T)(object)new GlobalExtractionFilterParameter(Repository,WhenIHaveA<ExtractionConfiguration>(),"DECLARE @ExtractionGlobal as varchar(100)");


            if (typeof (T) == typeof(ExtractableColumn))
            {
                var ei = WhenIHaveA<ExtractionInformation>();

                var eds = new ExtractableDataSet(Repository, ei.CatalogueItem.Catalogue);
                var config  = WhenIHaveA<ExtractionConfiguration>();
                config.AddDatasetToConfiguration(eds);

                return (T)(object) config.GetAllExtractableColumnsFor(eds).Single();
            }
            
            if (typeof (T) == typeof(FilterContainer))
            {
                var sds = WhenIHaveA<SelectedDataSets>();
                var container = new FilterContainer(Repository, FilterContainerOperation.AND);
                sds.RootFilterContainer_ID = container.ID;
                sds.SaveToDatabase();

                return (T)(object)container;
            }
                              
            
            if (typeof (T) == typeof(DeployedExtractionFilter))
            {
                var container = WhenIHaveA<FilterContainer>();
                return (T)(object)new DeployedExtractionFilter(Repository,"Fish = 'haddock'",container);
            }
            if (typeof (T) == typeof(DeployedExtractionFilterParameter))
            {
                var filter = WhenIHaveA<DeployedExtractionFilter>();
                filter.WhereSQL = "@had = 'enough'";
                return (T)(object)filter.GetFilterFactory().CreateNewParameter(filter, "DECLARE @had as varchar(100)");
            }

            if(typeof(T)== typeof(ExtractionProgress))
            {
                var cata = new Catalogue(Repository, "MyCata");
                var cataItem = new CatalogueItem(Repository, cata, "MyCol");
                var table = new TableInfo(Repository, "MyTable");
                var col = new ColumnInfo(Repository, "mycol", "datetime", table);

                var ei = new ExtractionInformation(Repository, cataItem, col, "mycol");
                cata.TimeCoverage_ExtractionInformation_ID = ei.ID;
                cata.SaveToDatabase();

                var eds = new ExtractableDataSet(Repository, cata);
                var project = new Project(Repository, "My Proj");
                var config = new ExtractionConfiguration(Repository, project);
                var sds = new SelectedDataSets(Repository, config, eds, null);

                return (T)(object)new ExtractionProgress(Repository, sds);
            }
            
            throw new TestCaseNotWrittenYetException(typeof(T));
        }

        private void WhenIHaveTwoTables(out ColumnInfo col1, out ColumnInfo col2, out ColumnInfo col3)
        {
            TableInfo ti1;
            TableInfo ti2;
            WhenIHaveTwoTables(out ti1, out ti2, out col1, out col2, out col3);
        }

        private void WhenIHaveTwoTables(out TableInfo ti1, out TableInfo ti2, out ColumnInfo col1, out ColumnInfo col2, out ColumnInfo col3)
        {
            ti1 = WhenIHaveA<TableInfo>();
            ti1.Name = "ParentTable";
            ti1.SaveToDatabase();
            col1 = new ColumnInfo(Repository, "ParentCol", "varchar(10)", ti1);
         
            ti2 = WhenIHaveA<TableInfo>();
            ti2.Name = "Child Table";
            col2 = new ColumnInfo(Repository, "ChildCol", "varchar(10)", ti2);
            col3 = new ColumnInfo(Repository, "Desc", "varchar(10)", ti2);
        }

        private AggregateConfiguration WhenIHaveCohortAggregateConfiguration(string name)
        {
            var config = WhenIHaveA<AggregateConfiguration>();
            config.Name = name;
            config.SaveToDatabase();

            var ei = config.AggregateDimensions[0].ExtractionInformation;
            ei.IsExtractionIdentifier = true;
            ei.SaveToDatabase();
            return config;
        }

        /// <inheritdoc cref="WhenIHaveA{T}()"/>
        protected AggregateConfiguration WhenIHaveA<T>(out ExtractionInformation dateEi, out ExtractionInformation otherEi) where T : AggregateConfiguration
        {
            var ti = WhenIHaveA<TableInfo>();
            var dateCol = new ColumnInfo(Repository, "MyDateCol", "datetime2", ti);
            var otherCol = new ColumnInfo(Repository, "MyOtherCol", "varchar(10)", ti);

            var cata = WhenIHaveA<Catalogue>();
            var dateCi = new CatalogueItem(Repository, cata, dateCol.Name);
            dateEi = new ExtractionInformation(Repository, dateCi, dateCol, dateCol.Name);
            var otherCi = new CatalogueItem(Repository, cata, otherCol.Name);
            otherEi = new ExtractionInformation(Repository, otherCi, otherCol, otherCol.Name);

            var config = new AggregateConfiguration(Repository, cata, "My graph");
            new AggregateDimension(Repository, otherEi, config);
            return config;
        }

        /// <inheritdoc cref="WhenIHaveA{T}()"/>
        protected ANOTable WhenIHaveA<T>(out ExternalDatabaseServer server) where T : ANOTable
        {
            server = new ExternalDatabaseServer(Repository, "ANO Server", new ANOStorePatcher());
            var anoTable = new ANOTable(Repository, server, "ANOFish", "F");
            return anoTable;
        }

        /// <inheritdoc cref="WhenIHaveA{T}()"/>
        protected ObjectExport WhenIHaveA<T>(out ShareManager shareManager) where T : ObjectExport
        {
            shareManager = new ShareManager(new RepositoryProvider(Repository));
            return shareManager.GetNewOrExistingExportFor(WhenIHaveA<Catalogue>());
        }

        private T Save<T>(T s) where T : ISaveable
        {
            s.SaveToDatabase();
            return s;
        }
        
        protected MEF MEF;

        /// <summary>
        /// Call if your test needs to access classes via MEF.  Loads all dlls in the test directory.
        /// 
        /// <para>This must be called before you 'launch' your ui</para>
        /// </summary>
        protected void SetupMEF()
        {
            MEF = new MEF();
            MEF.Setup(new SafeDirectoryCatalog(new IgnoreAllErrorsCheckNotifier(),TestContext.CurrentContext.TestDirectory));
            Repository.CatalogueRepository.MEF = MEF;

            Validator.RefreshExtraTypes(MEF.SafeDirectoryCatalog,new ThrowImmediatelyCheckNotifier());
        }

        //Fields that can be safely ignored when comparing an object created in memory with one created into the database.
        private static readonly string[] IgnorePropertiesWhenDiffing = new[] {"ID","Repository","CatalogueRepository","SoftwareVersion"};
        public static Dictionary<PropertyInfo,HashSet<object>> _alreadyChecked = new Dictionary<PropertyInfo, HashSet<object>>();

        /// <summary>
        /// Asserts that the two objects are basically the same except for IDs/Repositories.  This includes checking all public properties
        /// that are not in the <see cref="IgnorePropertiesWhenDiffing"/> list.  Date fields will be validated as equal if they are within
        /// 10 seconds of each other (<see cref="AreAboutTheSameTime"/>).
        /// </summary>
        /// <param name="memObj"></param>
        /// <param name="dbObj"></param>
        /// <param name="firstIteration"></param>
        public static void AssertAreEqual(IMapsDirectlyToDatabaseTable memObj, IMapsDirectlyToDatabaseTable dbObj,bool firstIteration = true)
        {
            if(firstIteration)
                _alreadyChecked.Clear();

            foreach (PropertyInfo property in memObj.GetType().GetProperties())
            {
                if (IgnorePropertiesWhenDiffing.Contains(property.Name) || property.Name.EndsWith("_ID"))
                    continue;

                if (!_alreadyChecked.ContainsKey(property))
                    _alreadyChecked.Add(property,new HashSet<object>());

                //if we have already checked this property
                if(_alreadyChecked[property].Contains(memObj))
                    return; //don't check it again

                _alreadyChecked[property].Add(memObj);

                object memValue = null;
                object dbValue = null;
                try
                {
                    memValue = property.GetValue(memObj);
                }
                catch (Exception e)
                {
                    Assert.Fail("{0} Property {1} could not be read from Memory:\r\n{2}", memObj.GetType().Name, property.Name,e);
                }
                try
                {
                    dbValue = property.GetValue(dbObj);
                }
                catch (Exception e)
                {
                    Assert.Fail("{0} Property {1} could not be read from Database:\r\n{2}", dbObj.GetType().Name, property.Name, e);
                }

                if(memValue is IMapsDirectlyToDatabaseTable)
                {
                    AssertAreEqual((IMapsDirectlyToDatabaseTable)memValue, (IMapsDirectlyToDatabaseTable)dbValue,false);
                    return;
                }
                if (memValue is IEnumerable<IMapsDirectlyToDatabaseTable>)
                {
                    AssertAreEqual((IEnumerable<IMapsDirectlyToDatabaseTable>)memValue, (IEnumerable<IMapsDirectlyToDatabaseTable>)dbValue,false);
                    return;
                }

                if (memValue is DateTime && dbValue is DateTime)
                    if (!AreAboutTheSameTime((DateTime) memValue, (DateTime) dbValue))
                        Assert.Fail("Dates differed, {0} Property {1} differed Memory={2} and Db={3}",memObj.GetType().Name, property.Name, memValue, dbValue);
                    else
                        return;

                //treat empty strings as the same as 
                memValue = memValue as string == string.Empty ? null : memValue;
                dbValue = dbValue as string == string.Empty ? null : dbValue;

                //all other properties should be legit
                Assert.AreEqual(memValue, dbValue, "{0} Property {1} differed Memory={2} and Db={3}", memObj.GetType().Name,property.Name, memValue, dbValue);
            }
        }
        public static void AssertAreEqual(IEnumerable<IMapsDirectlyToDatabaseTable> memObjects, IEnumerable<IMapsDirectlyToDatabaseTable> dbObjects,bool firstIteration=true)
        {
            var memObjectsArr = memObjects.OrderBy(o => o.ID).ToArray();
            var dbObjectsArr = dbObjects.OrderBy(o => o.ID).ToArray();

            Assert.AreEqual(memObjectsArr.Count(), dbObjectsArr.Count());

            for (int i = 0; i < memObjectsArr.Count(); i++)
                UnitTests.AssertAreEqual(memObjectsArr[i], dbObjectsArr[i],firstIteration);
        }

        /// <summary>
        /// The number of seconds that have to differ between two DateTime objects in method <see cref="AreAboutTheSameTime"/> before
        /// they are considered not the same time
        /// </summary>
        const double TimeThresholdInSeconds = 60;

        private static bool AreAboutTheSameTime(DateTime memValue, DateTime dbValue)
        {
            return Math.Abs(memValue.Subtract(dbValue).TotalSeconds) < TimeThresholdInSeconds;
        }


        /// <summary>
        /// Returns instances of all Types supported by <see cref="WhenIHaveA{T}()"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DatabaseEntity> WhenIHaveAll()
        {
            var types = typeof(Catalogue).Assembly.GetTypes()
                .Where(t => t != null && typeof (DatabaseEntity).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToArray();

            var methodWhenIHaveA = GetWhenIHaveAMethod();
            
            foreach (Type t in types)
            {
                //ignore these types too
                if (SkipTheseTypes.Contains(t.Name) || t.Name.StartsWith("Spontaneous") ||
                    typeof (SpontaneousObject).IsAssignableFrom(t))
                    continue;

                //ensure that the method supports the Type
                var genericWhenIHaveA = methodWhenIHaveA.MakeGenericMethod(t);
                yield return (DatabaseEntity) genericWhenIHaveA.Invoke(this, null);
            }
        }

        /// <summary>
        /// Returns a properly initialized object of Type <paramref name="t"/> which must be a <see cref="DatabaseEntity"/> that
        /// is supported by <see cref="UnitTests"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IMapsDirectlyToDatabaseTable WhenIHaveA(Type t)
        {
            var methodWhenIHaveA = GetWhenIHaveAMethod();
            //ensure that the method supports the Type
            var genericWhenIHaveA = methodWhenIHaveA.MakeGenericMethod(t);
            return (DatabaseEntity) genericWhenIHaveA.Invoke(this, null);
        }

        private MethodInfo GetWhenIHaveAMethod()
        {
            var methods = typeof (UnitTests).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return methods.Single(m => m.Name.Equals(nameof(WhenIHaveA)) && !m.GetParameters().Any());
        }

    }
}