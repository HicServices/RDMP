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
using System.Runtime.CompilerServices;
using FAnsi;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Interactive;
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
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Validation;

namespace Tests.Common;

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
    /// Returns an <see cref="IBasicActivateItems"/> based on the <see cref="RepositoryLocator"/>
    /// (or <paramref name="locator"/> if specified) that throws if input is sought (e.g.
    /// <see cref="IBasicActivateItems.YesNo(DialogArgs)"/>)
    /// </summary>
    /// <returns></returns>
    protected IBasicActivateItems GetActivator(IRDMPPlatformRepositoryServiceLocator locator = null)
    {
        return new ConsoleInputManager(locator ?? RepositoryLocator, new ThrowImmediatelyCheckNotifier()) { DisallowInput = true};
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

    /// <summary>
    /// Creates a minimum viable object of Type T.  This includes the object and any dependencies e.g. a 
    /// <see cref="ColumnInfo"/> cannot exist without a <see cref="TableInfo"/>.  
    /// </summary>
    /// <typeparam name="T">Type of object you want to create</typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">If there is not yet an implementation for the given T.  Feel free to write one.</exception>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected T WhenIHaveA<T>() where T : DatabaseEntity
    {
        return WhenIHaveA<T>(Repository);
    }


    /// <summary>
    /// Creates a minimum viable object of Type T.  This includes the object and any dependencies e.g. a 
    /// <see cref="ColumnInfo"/> cannot exist without a <see cref="TableInfo"/>.  
    /// </summary>
    /// <typeparam name="T">Type of object you want to create</typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">If there is not yet an implementation for the given T.  Feel free to write one.</exception>
    public static T WhenIHaveA<T>(MemoryDataExportRepository repository) where T : DatabaseEntity
    {
        if (typeof(T) == typeof(Catalogue))
            return (T)(object)Save(new Catalogue(repository, "Mycata"));

            
        if (typeof(T) == typeof(ExtendedProperty))
        {
            return (T)(object)new ExtendedProperty(repository,Save(new Catalogue(repository, "Mycata")),"TestProp",0);
        }
                

        if (typeof(T) == typeof(CatalogueItem))
        {
            var cata = new Catalogue(repository, "Mycata");
            return (T)(object)new CatalogueItem(repository, cata, "MyCataItem");
        }

        if (typeof(T) == typeof(ExtractionInformation))
        {
            var col = WhenIHaveA<ColumnInfo>(repository);

            var cata = new Catalogue(repository, "Mycata");
            var ci = new CatalogueItem(repository, cata, "MyCataItem");
            var ei = new ExtractionInformation(repository, ci, col, "MyCataItem");
            return (T)(object)Save(ei);
        }

        if (typeof(T) == typeof(TableInfo))
        {
            var table = new TableInfo(repository, "My_Table"){DatabaseType = DatabaseType.MicrosoftSQLServer};
            return (T)(object)table;
        }

        if (typeof(T) == typeof(ColumnInfo))
        {
            var ti = WhenIHaveA<TableInfo>(repository);
            var col = new ColumnInfo(repository, "My_Col", "varchar(10)", ti);
            return (T)(object)col;
        }

        if (typeof(T) == typeof(AggregateConfiguration))
        {
            ExtractionInformation dateEi;
            ExtractionInformation otherEi;
            return (T)(object)WhenIHaveA<AggregateConfiguration>(repository,out dateEi, out otherEi);
        }

        if (typeof(T) == typeof(ExternalDatabaseServer))
        {
            return (T)(object)Save(new ExternalDatabaseServer(repository, "My Server",null));
        }

        if (typeof(T) == typeof(ANOTable))
        {
            ExternalDatabaseServer server;
            return (T)(object)WhenIHaveA<ANOTable>(repository, out server);
        }

        if (typeof(T) == typeof(LoadMetadata))
        {
            //creates the table, column, catalogue, catalogue item and extraction information
            var ei = WhenIHaveA<ExtractionInformation>(repository);
            var cata = ei.CatalogueItem.Catalogue;

            var ti = ei.ColumnInfo.TableInfo;
            ti.Server = "localhost";
            ti.Database = "mydb";
            ti.SaveToDatabase();

            var lmd = new LoadMetadata(repository, "MyLoad");
            cata.LoadMetadata_ID = lmd.ID;
            cata.SaveToDatabase();
            return (T)(object)lmd;
        }

        if (typeof (T) == typeof (AggregateTopX))
        {
            var agg = WhenIHaveA<AggregateConfiguration>(repository);
            return (T)(object) new AggregateTopX(repository, agg, 10);
        }

        if (typeof (T) == typeof (ConnectionStringKeyword))
        {
            return (T)(object)new ConnectionStringKeyword(repository, DatabaseType.MicrosoftSQLServer, "MultipleActiveResultSets", "true");
        }

        if (typeof (T) == typeof (DashboardLayout))
            return (T)(object)new DashboardLayout(repository, "My Layout");

        if (typeof(T) == typeof(DashboardControl))
        {
            var layout = WhenIHaveA<DashboardLayout>(repository);
            return (T)(object)Save(new DashboardControl(repository, layout, typeof(int), 0, 0, 100, 100, "") { ControlType = "GoodBadCataloguePieChart" });
        }

        if (typeof(T) == typeof(DashboardObjectUse))
        {
            var layout = WhenIHaveA<DashboardLayout>(repository);
            var control = Save(new DashboardControl(repository, layout, typeof(int), 0, 0, 100, 100, "") { ControlType = "GoodBadCataloguePieChart" });
            var use = new DashboardObjectUse(repository, control, WhenIHaveA<Catalogue>(repository));
            return (T)(object)use;
        }

        if (typeof(T) == typeof(ExtractionFilter))
        {
            var ei = WhenIHaveA<ExtractionInformation>(repository);
            return (T)(object)new ExtractionFilter(repository, "My Filter", ei);
        }

        if (typeof(T) == typeof(ExtractionFilterParameter))
        {
            var filter = WhenIHaveA<ExtractionFilter>(repository);
            filter.WhereSQL = "@myParam = 'T'";

            return (T)(object)new ExtractionFilterParameter(repository,"DECLARE @myParam varchar(10)",filter);
        }

        if (typeof(T) == typeof(ExtractionFilterParameterSetValue))
        {
            var parameter = WhenIHaveA<ExtractionFilterParameter>(repository);
            var set = new ExtractionFilterParameterSet(repository, parameter.ExtractionFilter, "Parameter Set");
            return (T)(object)new ExtractionFilterParameterSetValue(repository, set, parameter);
        }

        if (typeof (T) == typeof(ExtractionFilterParameterSet))
        {
            return (T)(object)WhenIHaveA<ExtractionFilterParameterSetValue>(repository).ExtractionFilterParameterSet;
        }

        if (typeof (T) == typeof (Favourite))
            return (T) (object) new Favourite(repository, WhenIHaveA<Catalogue>(repository));

        if (typeof (T) == typeof(ObjectExport))
        {
            ShareManager sm;
            return (T)(object)WhenIHaveA<ObjectExport>(repository, out sm);
        }
            
        if (typeof (T) == typeof(ObjectImport))
        {
            ShareManager sm;
            var export = WhenIHaveA<ObjectExport>(repository, out sm);
            return (T)(object)sm.GetImportAs(export.SharingUID, WhenIHaveA<Catalogue>(repository));
        }
            
        if (typeof (T) == typeof(WindowLayout))
            return (T)(object)new WindowLayout(repository,"My window arrangement","<html><body>ignore this</body></html>");

        if (typeof (T) == typeof(RemoteRDMP))
            return (T)(object)new RemoteRDMP(repository);
            
        if (typeof (T) == typeof(CohortIdentificationConfiguration))
            return (T)(object)new CohortIdentificationConfiguration(repository,"My cic");

        if (typeof (T) == typeof(JoinableCohortAggregateConfiguration))
        {
            var config = WhenIHaveCohortAggregateConfiguration(repository, "PatientIndexTable");
            var cic = WhenIHaveA<CohortIdentificationConfiguration>(repository);
            cic.EnsureNamingConvention(config);
            return (T)(object)new JoinableCohortAggregateConfiguration(repository,cic,config);
        }
            
        if (typeof (T) == typeof(JoinableCohortAggregateConfigurationUse))
        {
            var joinable = WhenIHaveA<JoinableCohortAggregateConfiguration>(repository);
            var config = WhenIHaveCohortAggregateConfiguration(repository, "Aggregate");
            return (T)(object)joinable.AddUser(config);
        }
            
        if (typeof (T) == typeof(Rdmp.Core.Curation.Data.Plugin))
            return (T)(object)new Rdmp.Core.Curation.Data.Plugin(repository,new FileInfo("bob.nupkg"),new Version(1,1,1),new Version(1,1,1));
            
        if (typeof (T) == typeof(LoadModuleAssembly))
        {
            var dll = Path.Combine(TestContext.CurrentContext.TestDirectory,"a.nupkg");
            File.WriteAllBytes(dll,new byte[] {0x11});

            return (T)(object)new LoadModuleAssembly(repository,new FileInfo(dll),WhenIHaveA<Rdmp.Core.Curation.Data.Plugin>(repository));
        }
            
        if (typeof (T) == typeof(AggregateContinuousDateAxis))
        {
            ExtractionInformation dateEi;
            ExtractionInformation otherEi;
            var config = WhenIHaveA<AggregateConfiguration>(repository, out dateEi,out otherEi);
                
            //remove the other Ei
            config.AggregateDimensions[0].DeleteInDatabase();
            //add the date one
            var dim = new AggregateDimension(repository, dateEi, config);

            return (T)(object)new AggregateContinuousDateAxis(repository,dim);
        }
            
        if (typeof (T) == typeof(AggregateDimension))
            return (T)(object) WhenIHaveA<AggregateConfiguration>(repository).AggregateDimensions[0];
            
        if (typeof (T) == typeof(AggregateFilterContainer))
        {
            var config = WhenIHaveA<AggregateConfiguration>(repository);
            var container = new AggregateFilterContainer(repository,FilterContainerOperation.AND);
            config.RootFilterContainer_ID = container.ID;
            config.SaveToDatabase();
            return (T) (object) container;
        }
        if (typeof (T) == typeof(AggregateFilter))
        {
            var container = WhenIHaveA<AggregateFilterContainer>(repository);
            return (T)(object)new AggregateFilter(repository,"My Filter",container);
        }
            
        if (typeof (T) == typeof(AggregateFilterParameter))
        {
            var filter = WhenIHaveA<AggregateFilter>(repository);
            filter.WhereSQL = "@MyP = 'mnnn apples'";
            filter.SaveToDatabase();

            return (T)filter.GetFilterFactory().CreateNewParameter(filter, "DECLARE @MyP as varchar(10)");
        }

        if (typeof (T) == typeof (LoadProgress))
            return (T) (object) new LoadProgress(repository, WhenIHaveA<LoadMetadata>(repository));

        if (typeof (T) == typeof(CacheProgress))
            return (T)(object)new CacheProgress(repository,WhenIHaveA<LoadProgress>(repository));

        if (typeof (T) == typeof(CacheFetchFailure))
            return (T)(object)new CacheFetchFailure(repository,WhenIHaveA<CacheProgress>(repository),DateTime.Now.Subtract(new TimeSpan(1,0,0,0)),DateTime.Now,new Exception("It didn't work"));
            
        if (typeof (T) == typeof(CohortAggregateContainer))
        {
            var cic = WhenIHaveA<CohortIdentificationConfiguration>(repository);
            cic.CreateRootContainerIfNotExists();
            return (T)(object)cic.RootCohortAggregateContainer;
        }
            
        if (typeof (T) == typeof(AnyTableSqlParameter))
        {
            var cic = WhenIHaveA<CohortIdentificationConfiguration>(repository);
            return (T)(object)new AnyTableSqlParameter(repository,cic,"DECLARE @myGlobal as varchar(10)");
        }

        if (typeof (T) == typeof(DataAccessCredentials))
            return (T)(object)new DataAccessCredentials(repository,"My credentials");
            
        if (typeof (T) == typeof(GovernancePeriod))
            return (T)(object)new GovernancePeriod(repository);
            
        if (typeof (T) == typeof(GovernanceDocument))
        {
            var fi = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, "myfile.txt"));
            return (T)(object)new GovernanceDocument(repository,WhenIHaveA<GovernancePeriod>(repository),fi);
        }

        if (typeof (T) == typeof(PermissionWindow))
            return (T)(object)new PermissionWindow(repository);
            

        if (typeof (T) == typeof(JoinInfo))
        {
            ColumnInfo col1;
            ColumnInfo col2;
            ColumnInfo col3;
            WhenIHaveTwoTables(repository, out col1,out col2,out col3);
                
            return (T)(object)new JoinInfo(repository,col1,col2,ExtractionJoinType.Left, null);
        }
        if (typeof (T) == typeof(Lookup))
        {
            ColumnInfo col1;
            ColumnInfo col2;
            ColumnInfo col3;
            WhenIHaveTwoTables(repository, out col1,out col2,out col3);
                
            return (T)(object)new Lookup(repository,col3,col1,col2,ExtractionJoinType.Left, null);
        }
        if (typeof (T) == typeof(LookupCompositeJoinInfo))
        {
            var lookup = WhenIHaveA<Lookup>(repository);

            var otherJoinFk = new ColumnInfo(repository,"otherJoinKeyForeign","int",lookup.ForeignKey.TableInfo);
            var otherJoinPk = new ColumnInfo(repository,"otherJoinKeyPrimary","int",lookup.PrimaryKey.TableInfo);
                
            return (T)(object)new LookupCompositeJoinInfo(repository,lookup,otherJoinFk,otherJoinPk);
        }
        if (typeof (T) == typeof(Pipeline))
            return (T)(object)new Pipeline(repository,"My Pipeline");

        if (typeof (T) == typeof(PipelineComponent))
            return (T)(object)new PipelineComponent(repository, WhenIHaveA<Pipeline>(repository), typeof(ColumnForbidder),0,"My Component");
            
        if (typeof (T) == typeof(PipelineComponentArgument))
        {
            var comp = WhenIHaveA<PipelineComponent>(repository);
            return (T)comp.CreateArgumentsForClassIfNotExists<ColumnForbidder>().First();
        }

        if (typeof (T) == typeof(PreLoadDiscardedColumn))
            return (T)(object)new PreLoadDiscardedColumn(repository,WhenIHaveA<TableInfo>(repository),"MyDiscardedColum");
                        
                       
        if (typeof (T) == typeof(ProcessTask))
            return (T)(object)new ProcessTask(repository,WhenIHaveA<LoadMetadata>(repository),LoadStage.AdjustRaw);

        if (typeof (T) == typeof(ProcessTaskArgument))
            return (T)(object)new ProcessTaskArgument(repository,WhenIHaveA<ProcessTask>(repository));

                
        if (typeof (T) == typeof(StandardRegex))
            return (T)(object)new StandardRegex(repository);

        if (typeof (T) == typeof(SupportingSQLTable))
            return (T)(object)new SupportingSQLTable(repository,WhenIHaveA<Catalogue>(repository),"Some Handy Query");

        if (typeof (T) == typeof(TicketingSystemConfiguration))
            return (T)(object)new TicketingSystemConfiguration(repository,"My Ticketing System");

        if (typeof (T) == typeof(SupportingDocument))
            return (T)(object)new SupportingDocument(repository,WhenIHaveA<Catalogue>(repository),"HelpFile.docx");

        if (typeof (T) == typeof(Project))
            return (T)(object)new Project(repository,"My Project");
            
        if (typeof (T) == typeof(ExtractionConfiguration))
            return (T)(object)new ExtractionConfiguration(repository,WhenIHaveA<Project>(repository));
            
        if (typeof (T) == typeof(ExtractableDataSet))
        {
            //To make an extractable dataset we need an extraction identifier (e.g. chi) that will be linked in the cohort
            var ei = WhenIHaveA<ExtractionInformation>(repository);
            ei.IsExtractionIdentifier = true;
            ei.SaveToDatabase();

            //And we need another column too just for sanity sakes (in the same table)
            var ci2 = new CatalogueItem(repository,ei.CatalogueItem.Catalogue,"ci2");
            var col2 = new ColumnInfo(repository, "My_Col2", "varchar(10)", ei.ColumnInfo.TableInfo);
            var ei2 = new ExtractionInformation(repository,ci2,col2,col2.GetFullyQualifiedName());

            return (T)(object)new ExtractableDataSet(repository,ei.CatalogueItem.Catalogue);
        }
            
        if (typeof (T) == typeof(CumulativeExtractionResults))
            return (T)(object)new CumulativeExtractionResults(repository,WhenIHaveA<ExtractionConfiguration>(repository),WhenIHaveA<ExtractableDataSet>(repository),"SELECT * FROM Anywhere");
            
        if (typeof (T) == typeof(SelectedDataSets))
        {
            var eds = WhenIHaveA<ExtractableDataSet>(repository);
            var config = WhenIHaveA<ExtractionConfiguration>(repository);

            foreach (var ei in eds.Catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            {
                var ec = new ExtractableColumn(repository, eds, config,ei,ei.Order,ei.SelectSQL);
            }

            return (T)(object)new SelectedDataSets(repository,config,eds, null);
        }
                

        if (typeof (T) == typeof(ReleaseLog))
        {
            var file = Path.Combine(TestContext.CurrentContext.TestDirectory,"myDataset.csv");
            File.WriteAllText(file,"omg rows");
                
            var sds = WhenIHaveA<SelectedDataSets>(repository);
            new CumulativeExtractionResults(repository,sds.ExtractionConfiguration,sds.ExtractableDataSet,"SELECT * FROM ANYWHERE");
            var potential = new FlatFileReleasePotential(new RepositoryProvider(repository), sds);

            return (T)(object)new ReleaseLog(repository,
                potential,
                new ReleaseEnvironmentPotential(sds.ExtractionConfiguration),
                false,
                new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                new FileInfo(file));
                        
        }

        if (typeof (T) == typeof(ExtractableDataSetPackage))
            return (T)(object)new ExtractableDataSetPackage(repository,"My Cool Package");
            
            
        if (typeof (T) == typeof(SupplementalExtractionResults))
        {
            return (T)(object)new SupplementalExtractionResults(repository,WhenIHaveA<CumulativeExtractionResults>(repository),"Select * from Lookup",WhenIHaveA<SupportingSQLTable>(repository));
        }

        if (typeof (T) == typeof(SelectedDataSetsForcedJoin))
            return (T)(object)new SelectedDataSetsForcedJoin(repository,WhenIHaveA<SelectedDataSets>(repository),WhenIHaveA<TableInfo>(repository));
            
        if (typeof (T) == typeof(ProjectCohortIdentificationConfigurationAssociation))
            return (T)(object)new ProjectCohortIdentificationConfigurationAssociation(repository,WhenIHaveA<Project>(repository),WhenIHaveA<CohortIdentificationConfiguration>(repository));
            
        if (typeof (T) == typeof(ExternalCohortTable))
            return Save((T)(object)new ExternalCohortTable(repository,"My cohorts",DatabaseType.MicrosoftSQLServer)
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
            return (T)(object)new GlobalExtractionFilterParameter(repository,WhenIHaveA<ExtractionConfiguration>(repository),"DECLARE @ExtractionGlobal as varchar(100)");


        if (typeof (T) == typeof(ExtractableColumn))
        {
            var ei = WhenIHaveA<ExtractionInformation>(repository);

            var eds = new ExtractableDataSet(repository, ei.CatalogueItem.Catalogue);
            var config  = WhenIHaveA<ExtractionConfiguration>(repository);
            config.AddDatasetToConfiguration(eds);

            return (T)(object) config.GetAllExtractableColumnsFor(eds).Single();
        }
            
        if (typeof (T) == typeof(FilterContainer))
        {
            var sds = WhenIHaveA<SelectedDataSets>(repository);
            var container = new FilterContainer(repository, FilterContainerOperation.AND);
            sds.RootFilterContainer_ID = container.ID;
            sds.SaveToDatabase();

            return (T)(object)container;
        }
                              
            
        if (typeof (T) == typeof(DeployedExtractionFilter))
        {
            var container = WhenIHaveA<FilterContainer>(repository);
            return (T)(object)new DeployedExtractionFilter(repository,"Fish = 'haddock'",container);
        }
        if (typeof (T) == typeof(DeployedExtractionFilterParameter))
        {
            var filter = WhenIHaveA<DeployedExtractionFilter>(repository);
            filter.WhereSQL = "@had = 'enough'";
            return (T)(object)filter.GetFilterFactory().CreateNewParameter(filter, "DECLARE @had as varchar(100)");
        }

        if(typeof(T)== typeof(ExtractionProgress))
        {
            var cata = new Catalogue(repository, "MyCata");
            var cataItem = new CatalogueItem(repository, cata, "MyCol");
            var table = new TableInfo(repository, "MyTable");
            var col = new ColumnInfo(repository, "mycol", "datetime", table);

            var ei = new ExtractionInformation(repository, cataItem, col, "mycol");
            cata.TimeCoverage_ExtractionInformation_ID = ei.ID;
            cata.SaveToDatabase();

            var eds = new ExtractableDataSet(repository, cata);
            var project = new Project(repository, "My Proj");
            var config = new ExtractionConfiguration(repository, project);
            var sds = new SelectedDataSets(repository, config, eds, null);

            return (T)(object)new ExtractionProgress(repository, sds);
        }


        if (typeof(T) == typeof(Commit))
        {
            return (T)(object)new Commit(repository, Guid.NewGuid(),"Breaking stuff");
        }

        if (typeof(T) == typeof(Memento))
        {
            var commit = WhenIHaveA<Commit>(repository);
            var cata = WhenIHaveA<Catalogue>(repository);

            return (T)(object)new Memento(repository, commit, MementoType.Add, cata, null, "placeholder");
        }


        throw new TestCaseNotWrittenYetException(typeof(T));
    }

    private static void WhenIHaveTwoTables(MemoryDataExportRepository repository,out ColumnInfo col1, out ColumnInfo col2, out ColumnInfo col3)
    {
        TableInfo ti1;
        TableInfo ti2;
        WhenIHaveTwoTables(repository, out ti1, out ti2, out col1, out col2, out col3);
    }

    private static void WhenIHaveTwoTables(MemoryDataExportRepository repository,out TableInfo ti1, out TableInfo ti2, out ColumnInfo col1, out ColumnInfo col2, out ColumnInfo col3)
    {
        ti1 = WhenIHaveA<TableInfo>(repository);
        ti1.Name = "ParentTable";
        ti1.Database = "MyDb";
        ti1.SaveToDatabase();
        col1 = new ColumnInfo(repository, "ParentCol", "varchar(10)", ti1);
         
        ti2 = WhenIHaveA<TableInfo>(repository);
        ti2.Name = "ChildTable";
        ti2.Database = "MyDb";
        col2 = new ColumnInfo(repository, "ChildCol", "varchar(10)", ti2);
        col3 = new ColumnInfo(repository, "Desc", "varchar(10)", ti2);
    }

    private static AggregateConfiguration WhenIHaveCohortAggregateConfiguration(MemoryDataExportRepository repository, string name)
    {
        var config = WhenIHaveA<AggregateConfiguration>(repository);
        config.Name = name;
        config.SaveToDatabase();

        var ei = config.AggregateDimensions[0].ExtractionInformation;
        ei.IsExtractionIdentifier = true;
        ei.SaveToDatabase();
        return config;
    }

    /// <inheritdoc cref="WhenIHaveA{T}()"/>
    protected static AggregateConfiguration WhenIHaveA<T>(MemoryDataExportRepository repository, out ExtractionInformation dateEi, out ExtractionInformation otherEi) where T : AggregateConfiguration
    {
        var ti = WhenIHaveA<TableInfo>(repository);
        var dateCol = new ColumnInfo(repository, "MyDateCol", "datetime2", ti);
        var otherCol = new ColumnInfo(repository, "MyOtherCol", "varchar(10)", ti);

        var cata = WhenIHaveA<Catalogue>(repository);
        var dateCi = new CatalogueItem(repository, cata, dateCol.Name);
        dateEi = new ExtractionInformation(repository, dateCi, dateCol, dateCol.Name);
        var otherCi = new CatalogueItem(repository, cata, otherCol.Name);
        otherEi = new ExtractionInformation(repository, otherCi, otherCol, otherCol.Name);

        var config = new AggregateConfiguration(repository, cata, "My graph");
        new AggregateDimension(repository, otherEi, config);
        return config;
    }

    /// <inheritdoc cref="WhenIHaveA{T}()"/>
    protected static ANOTable WhenIHaveA<T>(MemoryDataExportRepository repository, out ExternalDatabaseServer server) where T : ANOTable
    {
        server = new ExternalDatabaseServer(repository, "ANO Server", new ANOStorePatcher());
        var anoTable = new ANOTable(repository, server, "ANOFish", "F");
        return anoTable;
    }

    /// <inheritdoc cref="WhenIHaveA{T}()"/>
    protected static ObjectExport WhenIHaveA<T>(MemoryDataExportRepository repository, out ShareManager shareManager) where T : ObjectExport
    {
        shareManager = new ShareManager(new RepositoryProvider(repository));
        return shareManager.GetNewOrExistingExportFor(WhenIHaveA<Catalogue>(repository));
    }

    private static T Save<T>(T s) where T : ISaveable
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

        foreach (var property in memObj.GetType().GetProperties())
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

        for (var i = 0; i < memObjectsArr.Count(); i++)
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
            
        foreach (var t in types)
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