// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BadMedicine;
using BadMedicine.Datasets;
using FAnsi;
using FAnsi.Discovery;
using FAnsi.Discovery.ConnectionStringDefaults;
using Microsoft.Data.SqlClient;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;

namespace Rdmp.Core.CommandLine.DatabaseCreation;

/// <summary>
/// Handles the creation of example RDMP datasets and metadata object (catalogues, cohorts , projects etc).
/// </summary>
public partial class ExampleDatasetsCreation
{
    private IRDMPPlatformRepositoryServiceLocator _repos;
    private IBasicActivateItems _activator;
    public const int NumberOfPeople = 5000;
    public const int NumberOfRowsPerDataset = 10000;

    public ExampleDatasetsCreation(IBasicActivateItems activator, IRDMPPlatformRepositoryServiceLocator repos)
    {
        _repos = repos;
        _activator = activator;
    }

    internal void Create(DiscoveredDatabase db, ICheckNotifier notifier, PlatformDatabaseCreationOptions options)
    {
        if (db.Exists())
            if (options.DropDatabases)
                db.Drop();
            else
                throw new Exception(
                    $"Database {db.GetRuntimeName()} already exists and allowDrop option was not specified");

        if (db.Server.Builder is SqlConnectionStringBuilder b)
        {
            var keywords = _repos.CatalogueRepository
                .GetAllObjects<ConnectionStringKeyword>()
                .Where(k => k.DatabaseType == DatabaseType.MicrosoftSQLServer)
                .ToArray();

            AddKeywordIfSpecified(b.TrustServerCertificate, nameof(b.TrustServerCertificate), keywords);
        }

        notifier.OnCheckPerformed(new CheckEventArgs($"About to create {db.GetRuntimeName()}", CheckResult.Success));
        //create a new database for the datasets
        db.Create();

        notifier.OnCheckPerformed(
            new CheckEventArgs($"Successfully created {db.GetRuntimeName()}", CheckResult.Success));

        //fixed seed so everyone gets the same datasets
        var r = new Random(options.Seed);

        notifier.OnCheckPerformed(new CheckEventArgs("Generating people", CheckResult.Success));
        //people
        var people = new PersonCollection();
        people.GeneratePeople(options.NumberOfPeople, r);

        //datasets
        var biochem = ImportCatalogue(Create<Biochemistry>(db, people, r, notifier, options.NumberOfRowsPerDataset,
            "chi", "Healthboard", "SampleDate", "TestCode"));
        var demography = ImportCatalogue(Create<Demography>(db, people, r, notifier, options.NumberOfRowsPerDataset,
            "chi", "dtCreated", "hb_extract"));
        var prescribing = ImportCatalogue(Create<Prescribing>(db, people, r, notifier, options.NumberOfRowsPerDataset,
            "chi", "PrescribedDate", "Name")); //<- this is slooo!
        var admissions = ImportCatalogue(Create<HospitalAdmissions>(db, people, r, notifier,
            options.NumberOfRowsPerDataset, "chi", "AdmissionDate"));

        //Create but do not import the CarotidArteryScan dataset so that users can test out referencing a brand new table
        Create<CarotidArteryScan>(db, people, r, notifier, options.NumberOfRowsPerDataset, "RECORD_NUMBER");

        //the following should not be extractable
        ForExtractionInformations(demography,
            e => e.DeleteInDatabase(),
            "chi_num_of_curr_record",
            "surname",
            "forename",
            "current_address_L1",
            "current_address_L2",
            "current_address_L3",
            "current_address_L4",
            "birth_surname",
            "previous_surname",
            "midname",
            "alt_forename",
            "other_initials",
            "previous_address_L1",
            "previous_address_L2",
            "previous_address_L3",
            "previous_address_L4",
            "previous_postcode",
            "date_address_changed",
            "adr",
            "previous_gp_accept_date",
            "hic_dataLoadRunID");

        //the following should be special approval only
        ForExtractionInformations(demography,
            e =>
            {
                e.ExtractionCategory = ExtractionCategory.SpecialApprovalRequired;
                e.SaveToDatabase();
            },
            "current_postcode",
            "current_gp",
            "previous_gp",
            "date_of_birth");


        CreateAdmissionsViews(db);
        var vConditions = ImportCatalogue(db.ExpectTable("vConditions", null, TableType.View));
        var vOperations = ImportCatalogue(db.ExpectTable("vOperations", null, TableType.View));

        CreateGraph(biochem, "Test Codes", "TestCode", false, null);
        CreateGraph(biochem, "Test Codes By Date", "SampleDate", true, "TestCode");

        CreateFilter(biochem, "Creatinine", "TestCode", "TestCode like '%CRE%'",
            @"Serum creatinine is a blood measurement.  It is an indicator of renal health.");
        CreateFilter(biochem, "Test Code", "TestCode", "TestCode like @code", "Filters any test code set");

        CreateExtractionInformation(demography, "Age", "date_of_birth",
            "FLOOR(DATEDIFF(DAY, date_of_birth, GETDATE()) / 365.25) As Age");
        var fAge = CreateFilter(demography, "Older at least x years", "Age",
            "FLOOR(DATEDIFF(DAY, date_of_birth, GETDATE()) / 365.25) >= @age",
            "Patients age is greater than or equal to the provided @age");
        SetParameter(fAge, "@age", "int", "16");

        CreateGraph(demography, "Patient Ages", "Age", false, null);

        CreateGraph(prescribing, "Approved Name", "ApprovedName", false, null);
        CreateGraph(prescribing, "Approved Name Over Time", "PrescribedDate", true, "ApprovedName");

        CreateGraph(prescribing, "Bnf", "FormattedBnfCode", false, null);
        CreateGraph(prescribing, "Bnf Over Time", "PrescribedDate", true, "FormattedBnfCode");

        CreateFilter(
            CreateGraph(vConditions, "Conditions frequency", "Field", false, "Condition"),
            "Common Conditions Only",
            @"(Condition in 
(select top 40 Condition from vConditions c
 WHERE Condition <> 'NULL' AND Condition <> 'Nul' 
 group by Condition order by count(*) desc))");

        CreateFilter(
            CreateGraph(vOperations, "Operation frequency", "Field", false, "Operation"),
            "Common Operation Only",
            @"(Operation in 
(select top 40 Operation from vOperations c
 WHERE Operation <> 'NULL' AND Operation <> 'Nul' 
 group by Operation order by count(*) desc))");

        //group these all into the same folder
        admissions.Folder = @"\admissions";
        admissions.SaveToDatabase();
        vConditions.Folder = @"\admissions";
        vConditions.SaveToDatabase();
        vOperations.Folder = @"\admissions";
        vOperations.SaveToDatabase();


        var cmdCreateCohortTable = new ExecuteCommandCreateNewCohortStore(_activator, db, false, "chi", "varchar(10)");
        cmdCreateCohortTable.Execute();
        var externalCohortTable = cmdCreateCohortTable.Created;

        //Find the pipeline for committing cohorts
        var cohortCreationPipeline =
            _repos.CatalogueRepository.GetAllObjects<Pipeline>().FirstOrDefault(p =>
                p?.Source?.Class == typeof(CohortIdentificationConfigurationSource).FullName) ??
            throw new Exception("Could not find a cohort committing pipeline");

        //A cohort creation query
        var f = CreateFilter(vConditions, "Lung Cancer Condition", "Condition", "Condition like 'C349'",
            "ICD-10-CM Diagnosis Code C34.9 Malignant neoplasm of unspecified part of bronchus or lung");

        var cic = CreateCohortIdentificationConfiguration((ExtractionFilter)f);

        var cohort = CommitCohortToNewProject(cic, externalCohortTable, cohortCreationPipeline, "Lung Cancer Project",
            "P1 Lung Cancer Patients", 123, out var project);

        var cohortTable = cohort.ExternalCohortTable.DiscoverCohortTable();
        using (var con = cohortTable.Database.Server.GetConnection())
        {
            con.Open();
            //delete half the records (so we can simulate cohort refresh)
            using var cmd = cohortTable.Database.Server.GetCommand(
                $"DELETE TOP (10) PERCENT from {cohortTable.GetFullyQualifiedName()}", con);
            cmd.ExecuteNonQuery();
        }

        var ec1 = CreateExtractionConfiguration(project, cohort, "First Extraction (2016 - project 123)", true,
            notifier, biochem, prescribing, demography);
        var ec2 = CreateExtractionConfiguration(project, cohort, "Project 123 - 2017 Refresh", true, notifier, biochem,
            prescribing, demography, admissions);
        var ec3 = CreateExtractionConfiguration(project, cohort, "Project 123 - 2018 Refresh", true, notifier, biochem,
            prescribing, demography, admissions);

        ReleaseAllConfigurations(notifier, ec1, ec2, ec3);


        if (options.Nightmare)
        {
            var nightmare = new NightmareDatasets(_repos, db)
            {
                Factor = options.NightmareFactor
            };
            nightmare.Create(externalCohortTable);
        }
    }

    private void AddKeywordIfSpecified(bool isEnabled, string name, ConnectionStringKeyword[] alreadyDeclared)
    {
        if (isEnabled && !alreadyDeclared.Any(k => k.Name.Equals(name)))
        {
            var keyword = new ConnectionStringKeyword(_activator.RepositoryLocator.CatalogueRepository,
                DatabaseType.MicrosoftSQLServer, name, "true");

            //pass it into the system wide static keyword collection for use with all databases of this type all the time
            DiscoveredServerHelper.AddConnectionStringKeyword(keyword.DatabaseType, keyword.Name, keyword.Value,
                ConnectionStringKeywordPriority.SystemDefaultMedium);
        }
    }

    private void ReleaseAllConfigurations(ICheckNotifier notifier,
        params ExtractionConfiguration[] extractionConfigurations)
    {
        var releasePipeline = _repos.CatalogueRepository.GetAllObjects<Pipeline>()
            .FirstOrDefault(p => p?.Destination?.Class == typeof(BasicDataReleaseDestination).FullName);

        try
        {
            //cleanup any old releases
            var project = extractionConfigurations.Select(ec => ec.Project).Distinct().Single();

            var folderProvider = new ReleaseFolderProvider();
            var dir = folderProvider.GetFromProjectFolder(project);
            if (dir.Exists)
                dir.Delete(true);
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Could not detect/delete release folder for extractions",
                CheckResult.Warning, ex));
            return;
        }


        if (releasePipeline != null)
            try
            {
                var optsRelease = new ReleaseOptions
                {
                    Configurations = string.Join(",",
                        extractionConfigurations.Select(ec => ec.ID.ToString()).Distinct().ToArray()),
                    Pipeline = releasePipeline.ID.ToString()
                };

                var runnerRelease = new ReleaseRunner(optsRelease);
                runnerRelease.Run(_repos, ThrowImmediatelyDataLoadEventListener.Quiet, notifier,
                    new GracefulCancellationToken());
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not Release ExtractionConfiguration (never mind)",
                    CheckResult.Warning, ex));
            }
    }

    private static void ForExtractionInformations(ICatalogue catalogue, Action<ExtractionInformation> action,
        params string[] extractionInformations)
    {
        foreach (var e in extractionInformations.Select(s => GetExtractionInformation(catalogue, s)))
            action(e);
    }

    private static void SetParameter(IFilter filter, string paramterToSet, string dataType, string value)
    {
        var p = filter.GetAllParameters().Single(fp => fp.ParameterName == paramterToSet);
        p.ParameterSQL = $"DECLARE {paramterToSet} AS {dataType}";
        p.Value = value;
        p.SaveToDatabase();
    }

    private ExtractionInformation CreateExtractionInformation(ICatalogue catalogue, string name, string columnInfoName,
        string selectSQL)
    {
        var col =
            catalogue.GetTableInfoList(false).SelectMany(t => t.ColumnInfos)
                .SingleOrDefault(c => c.GetRuntimeName() == columnInfoName) ??
            throw new Exception($"Could not find ColumnInfo called '{columnInfoName}' in Catalogue {catalogue}");
        var ci = new CatalogueItem(_repos.CatalogueRepository, catalogue, name)
        {
            ColumnInfo_ID = col.ID
        };
        ci.SaveToDatabase();

        return new ExtractionInformation(_repos.CatalogueRepository, ci, col, selectSQL);
    }

    private ExtractionConfiguration CreateExtractionConfiguration(Project project, ExtractableCohort cohort,
        string name, bool isReleased, ICheckNotifier notifier, params ICatalogue[] catalogues)
    {
        var extractionConfiguration = new ExtractionConfiguration(_repos.DataExportRepository, project)
        {
            Name = name,
            Cohort_ID = cohort.ID
        };
        extractionConfiguration.SaveToDatabase();

        foreach (var c in catalogues)
        {
            //Get its extractableness
            var eds = _repos.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(c).SingleOrDefault()
                      ?? new ExtractableDataSet(_repos.DataExportRepository, c); //or make it extractable

            extractionConfiguration.AddDatasetToConfiguration(eds);
        }

        var extractionPipeline = _repos.CatalogueRepository.GetAllObjects<Pipeline>().FirstOrDefault(p =>
            p?.Destination?.Class == typeof(ExecuteDatasetExtractionFlatFileDestination).FullName);

        if (isReleased && extractionConfiguration != null)
        {
            var optsExtract = new ExtractionOptions
            {
                Pipeline = extractionPipeline.ID.ToString(),
                ExtractionConfiguration = extractionConfiguration.ID.ToString()
            };
            var runnerExtract = new ExtractionRunner(_activator, optsExtract);
            try
            {
                runnerExtract.Run(_repos, ThrowImmediatelyDataLoadEventListener.Quiet, notifier,
                    new GracefulCancellationToken());
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Could not run ExtractionConfiguration (never mind)",
                    CheckResult.Warning, ex));
            }

            extractionConfiguration.IsReleased = true;
            extractionConfiguration.SaveToDatabase();
        }

        return extractionConfiguration;
    }

    private ExtractableCohort CommitCohortToNewProject(CohortIdentificationConfiguration cic,
        ExternalCohortTable externalCohortTable, IPipeline cohortCreationPipeline, string projectName,
        string cohortName, int projectNumber, out Project project)
    {
        //create a new data extraction Project
        project = new Project(_repos.DataExportRepository, projectName)
        {
            ProjectNumber = projectNumber,
            ExtractionDirectory = Path.GetTempPath()
        };
        project.SaveToDatabase();

        //create a cohort
        var auditLogBuilder = new ExtractableCohortAuditLogBuilder();

        var request = new CohortCreationRequest(project,
            new CohortDefinition(null, cohortName, 1, projectNumber, externalCohortTable), _repos.DataExportRepository,
            ExtractableCohortAuditLogBuilder.GetDescription(cic))
        {
            CohortIdentificationConfiguration = cic
        };

        var engine = request.GetEngine(cohortCreationPipeline, ThrowImmediatelyDataLoadEventListener.Quiet);

        engine.ExecutePipeline(new GracefulCancellationToken());

        return request.CohortCreatedIfAny;
    }

    private CohortIdentificationConfiguration CreateCohortIdentificationConfiguration(ExtractionFilter inclusionFilter1)
    {
        //Create the top level configuration object
        var cic = new CohortIdentificationConfiguration(_repos.CatalogueRepository, "Tayside Lung Cancer Cohort");

        //create a UNION container for Inclusion Criteria
        var container = new CohortAggregateContainer(_repos.CatalogueRepository, SetOperation.UNION)
        {
            Name = "Inclusion Criteria"
        };
        container.SaveToDatabase();

        cic.RootCohortAggregateContainer_ID = container.ID;
        cic.SaveToDatabase();

        //Create a new cohort set to the 'Inclusion Criteria' based on the filters Catalogue
        var cata = inclusionFilter1.ExtractionInformation.CatalogueItem.Catalogue;
        var ac = cic.CreateNewEmptyConfigurationForCatalogue(cata,
            (a, b) => throw new Exception("Problem encountered with chi column(s)"), false);
        container.AddChild(ac, 0);

        //Add the filter to the WHERE logic of the cohort set
        var whereContainer = new AggregateFilterContainer(_repos.CatalogueRepository, FilterContainerOperation.OR);

        ac.Name = $"People with {inclusionFilter1.Name}";
        ac.RootFilterContainer_ID = whereContainer.ID;
        cic.EnsureNamingConvention(ac); //this will put cicx at the front and cause implicit SaveToDatabase

        var filterImporter = new FilterImporter(new AggregateFilterFactory(_repos.CatalogueRepository), null);
        var cloneFilter = filterImporter.ImportFilter(whereContainer, inclusionFilter1, null);

        whereContainer.AddChild(cloneFilter);

        return cic;
    }

    private IFilter CreateFilter(AggregateConfiguration graph, string name, string whereSql)
    {
        AggregateFilterContainer container;
        if (graph.RootFilterContainer_ID == null)
        {
            container = new AggregateFilterContainer(_repos.CatalogueRepository, FilterContainerOperation.AND);
            graph.RootFilterContainer_ID = container.ID;
            graph.SaveToDatabase();
        }
        else
        {
            container = (AggregateFilterContainer)graph.RootFilterContainer;
        }

        var filter = new AggregateFilter(_repos.CatalogueRepository, name, container)
        {
            WhereSQL = whereSql
        };
        filter.SaveToDatabase();

        return filter;
    }

    private static void CreateAdmissionsViews(DiscoveredDatabase db)
    {
        using var con = db.Server.GetConnection();
        con.Open();
        using (var cmd = db.Server.GetCommand(
                   """
                   create view vConditions as

                   SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,Condition,Field
                   FROM
                   (
                     SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,MainCondition,OtherCondition1,OtherCondition2,OtherCondition3
                     FROM HospitalAdmissions
                   ) AS cp
                   UNPIVOT
                   (
                     Condition FOR Field IN (MainCondition,OtherCondition1,OtherCondition2,OtherCondition3)
                   ) AS up;
                   """, con))
        {
            cmd.ExecuteNonQuery();
        }


        using (var cmd = db.Server.GetCommand(
                   """
                   create view vOperations as

                   SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,Operation,Field
                   FROM
                   (
                     SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,MainOperation,OtherOperation1,OtherOperation2,OtherOperation3
                     FROM HospitalAdmissions
                   ) AS cp
                   UNPIVOT
                   (
                     Operation FOR Field IN (MainOperation,OtherOperation1,OtherOperation2,OtherOperation3)
                   ) AS up;
                   """, con))
        {
            cmd.ExecuteNonQuery();
        }
    }

    private IFilter CreateFilter(ICatalogue cata, string name, string parentExtractionInformation, string whereSql,
        string desc)
    {
        var filter = new ExtractionFilter(_repos.CatalogueRepository, name,
            GetExtractionInformation(cata, parentExtractionInformation))
        {
            WhereSQL = whereSql,
            Description = desc
        };
        filter.SaveToDatabase();

        var parameterCreator = new ParameterCreator(filter.GetFilterFactory(), null, null);
        parameterCreator.CreateAll(filter, null);

        return filter;
    }

    /// <summary>
    /// Creates a new AggregateGraph for the given dataset (<paramref name="cata"/>)
    /// </summary>
    /// <param name="cata"></param>
    /// <param name="name">The name to give the graph</param>
    /// <param name="dimension1">The first dimension e.g. pass only one dimension to create a bar chart</param>
    /// <param name="isAxis">True if <paramref name="dimension1"/> should be created as a axis (creates a line chart)</param>
    /// <param name="dimension2">Optional second dimension to create (this will be the pivot column)</param>
    private AggregateConfiguration CreateGraph(ICatalogue cata, string name, string dimension1, bool isAxis,
        string dimension2)
    {
        var ac = new AggregateConfiguration(_repos.CatalogueRepository, cata, name)
        {
            CountSQL = "count(*) as NumberOfRecords"
        };
        ac.SaveToDatabase();
        ac.IsExtractable = true;

        var mainDimension = ac.AddDimension(GetExtractionInformation(cata, dimension1));
        var otherDimension = string.IsNullOrWhiteSpace(dimension2)
            ? null
            : ac.AddDimension(GetExtractionInformation(cata, dimension2));

        if (isAxis)
        {
            var axis = new AggregateContinuousDateAxis(_repos.CatalogueRepository, mainDimension)
            {
                StartDate = "'1970-01-01'",
                AxisIncrement = FAnsi.Discovery.QuerySyntax.Aggregation.AxisIncrement.Year
            };
            axis.SaveToDatabase();
        }

        if (otherDimension != null)
        {
            ac.PivotOnDimensionID = otherDimension.ID;
            ac.SaveToDatabase();
        }

        return ac;
    }

    private static ExtractionInformation GetExtractionInformation(ICatalogue cata, string name)
    {
        try
        {
            return cata.GetAllExtractionInformation(ExtractionCategory.Any).Single(ei =>
                ei.GetRuntimeName().Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        catch
        {
            throw new Exception($"Could not find an ExtractionInformation called '{name}' in dataset {cata.Name}");
        }
    }

    private static DiscoveredTable Create<T>(DiscoveredDatabase db, PersonCollection people, Random r,
        ICheckNotifier notifier, int numberOfRecords, params string[] primaryKey) where T : IDataGenerator
    {
        var dataset = typeof(T).Name;
        notifier.OnCheckPerformed(new CheckEventArgs($"Generating {numberOfRecords} records for {dataset}",
            CheckResult.Success));

        //half a million biochemistry results
        var biochem = DataGeneratorFactory.Create(typeof(T), r);
        var dt = biochem.GetDataTable(people, numberOfRecords);

        //prune "nulls"
        foreach (DataRow dr in dt.Rows)
            for (var i = 0; i < dt.Columns.Count; i++)
                if (string.Equals(dr[i] as string, "NULL", StringComparison.CurrentCultureIgnoreCase))
                    dr[i] = DBNull.Value;


        notifier.OnCheckPerformed(new CheckEventArgs($"Uploading {dataset}", CheckResult.Success));
        var tbl = db.CreateTable(dataset, dt, GetExplicitColumnDefinitions<T>());

        if (primaryKey.Length != 0)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Creating Primary Key {dataset}", CheckResult.Success));
            var cols = primaryKey.Select(s => tbl.DiscoverColumn(s)).ToArray();
            tbl.CreatePrimaryKey(5000, cols);
        }

        return tbl;
    }

    private static DatabaseColumnRequest[] GetExplicitColumnDefinitions<T>() where T : IDataGenerator
    {
        return typeof(T) == typeof(HospitalAdmissions)
            ? new[]
            {
                new DatabaseColumnRequest("MainOperation", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("MainOperationB", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation1", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation1B", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation2", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation2B", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation3", new DatabaseTypeRequest(typeof(string), 4)),
                new DatabaseColumnRequest("OtherOperation3B", new DatabaseTypeRequest(typeof(string), 4))
            }
            : null;
    }

    private ITableInfo ImportTableInfo(DiscoveredTable tbl)
    {
        var importer = new TableInfoImporter(_repos.CatalogueRepository, tbl);
        importer.DoImport(out var ti, out _);

        return ti;
    }

    private ICatalogue ImportCatalogue(DiscoveredTable tbl) => ImportCatalogue(ImportTableInfo(tbl));

    private ICatalogue ImportCatalogue(ITableInfo ti)
    {
        var forwardEngineer = new ForwardEngineerCatalogue(ti, ti.ColumnInfos);
        forwardEngineer.ExecuteForwardEngineering(out var cata, out _, out var eis);

        //get descriptions of the columns from BadMedicine
        cata.Description = Trim(Descriptions.Get(cata.Name));
        if (cata.Description != null)
        {
            cata.SaveToDatabase();

            foreach (var ci in cata.CatalogueItems)
            {
                var ciDescription = Trim(Descriptions.Get(cata.Name, ci.Name));
                if (ciDescription != null)
                {
                    ci.Description = ciDescription;
                    ci.SaveToDatabase();
                }
            }
        }

        var chi = eis.SingleOrDefault(e => e.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
        if (chi != null)
        {
            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            new ExtractableDataSet(_repos.DataExportRepository, cata);
        }

        return cata;
    }

    private static string Trim(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;

        //replace 2+ tabs and spaces with single spaces
        return SpaceTabs().Replace(s, " ").Trim();
    }

    [GeneratedRegex("[ \\t]{2,}")]
    private static partial Regex SpaceTabs();
}