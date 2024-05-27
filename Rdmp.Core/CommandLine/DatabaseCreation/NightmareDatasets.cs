// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using SynthEHR;
using SynthEHR.Datasets;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.DatabaseCreation;

internal class NightmareDatasets : DataGenerator
{
    private IRDMPPlatformRepositoryServiceLocator _repos;
    private string _serverName;
    private string _databaseNameWrapped;
    private string _databaseNameRuntime;

    /// <summary>
    /// Defaults to 1, set to 2 to double the amount of objects generated.
    /// Set to 10 to produce 10 times the amount etc
    /// </summary>
    public int Factor = 1;

    public NightmareDatasets(IRDMPPlatformRepositoryServiceLocator repos, DiscoveredDatabase db) : base(new Random(123))
    {
        _repos = repos;
        var db1 = db;
        _serverName = db1.Server.Name;
        _databaseNameWrapped = db1.GetWrappedName();
        _databaseNameRuntime = db1.GetRuntimeName();
    }

    private BucketList<Catalogue> Catalogues = new ();
    private BucketList<ExtractableDataSet> ExtractableDatasets = new ();
    private BucketList<Project> Projects = new ();
    private BucketList<TableInfo> Tables = new ();
    private int TablesCount;

    private BucketList<ColumnInfo> Columns = new();
    private int ColumnsCount;

    /// <summary>
    /// <para>Generates a lot of metadata in the RDMP platform databases.  This is for testing
    /// system scalability.
    /// </para>
    /// <remarks>We use <see cref="DataGenerator.GetRandomGPCode(Random)"/> a lot, this is just because it is a nice
    /// short string of letter and numbers not because we are actually using GP codes</remarks>
    /// </summary>
    public void Create(ExternalCohortTable ect)
    {
        // how likely is a given ExtractionInformation to be each of these
        // categories
        var extractionCategories = new BucketList<ExtractionCategory>();
        extractionCategories.Add(100, ExtractionCategory.Core);
        extractionCategories.Add(10, ExtractionCategory.Internal);
        extractionCategories.Add(5, ExtractionCategory.Supplemental);
        extractionCategories.Add(10, ExtractionCategory.SpecialApprovalRequired);
        extractionCategories.Add(4, ExtractionCategory.Deprecated);

        var extractionDir = Path.GetTempPath();


        // Based on DLS figures see: https://github.com/HicServices/RDMP/issues/1224
        for (var i = 0; i < 500 * Factor; i++)
        {
            var cata = new Catalogue(_repos.CatalogueRepository, $"Catalogue {GetRandomGPCode(r)}")
            {
                Description = GetRandomSentence(r)
            };
            cata.SaveToDatabase();
            Catalogues.Add(1, cata);

            // half of datasets have linkage identifiers
            var hasExtractionIdentifier = false;
            var first = true;

            // 14497 CatalogueItem
            // 8922 ExtractionInformation
            // = 60%  of columns are extractable

            foreach (var col in CreateTable())
            {
                var ci = new CatalogueItem(_repos.CatalogueRepository, cata, col.Name);

                // = 60%  of columns are extractable
                if (r.Next(10) < 6)
                {
                    var ei = new ExtractionInformation(_repos.CatalogueRepository, ci, col, col.Name)
                    {
                        ExtractionCategory = extractionCategories.GetRandom(r)
                    };

                    if (first)
                    {
                        hasExtractionIdentifier = r.Next(2) == 0;

                        // make the first field the linkage identifier
                        // if we are doing that
                        if (hasExtractionIdentifier)
                        {
                            ei.IsExtractionIdentifier = true;
                            ei.ExtractionCategory = ExtractionCategory.Core;
                            ei.SaveToDatabase();
                        }
                    }

                    first = false;
                }
            }

            // half of the Catalogues have IsExtractionIdentifier
            // but let's make only 75% of those extractable datasets
            if (r.Next(5) > 0 && hasExtractionIdentifier)
            {
                var eds = new ExtractableDataSet(_repos.DataExportRepository, cata);
                ExtractableDatasets.Add(1, eds);
            }
        }

        // There are 500 tables associated with Catalogues
        // but also 250 tables that are not linked to any Catalogues
        for (var i = 0; i < 250 * Factor; i++) CreateTable();

        // open a connection to the cohort db for creating external cohorts
        using var con = ect.Discover().Server.GetManagedConnection();

        for (var i = 0; i < 200 * Factor; i++)
        {
            // each project
            var p = new Project(_repos.DataExportRepository, $"Project {i}")
            {
                ProjectNumber = r.Next(50) == 0 ? 5 : i, // it's ok for some projects to have the same number
                ExtractionDirectory = extractionDir
            };
            p.SaveToDatabase();
            Projects.Add(1, p);

            // has an average of 5 ExtractionConfigurations but could have 0 to 10
            var numberOfConfigs = GetGaussianInt(0, 10);

            for (var c = 0; c < numberOfConfigs; c++)
            {
                var config = new ExtractionConfiguration(_repos.DataExportRepository, p,
                    $"Extraction {GetRandomGPCode(r)}");
                if (r.Next(2) == 0)
                    config.RequestTicket = GetRandomGPCode(r); // some have request tickets
                if (r.Next(4) == 0)
                    config.ReleaseTicket = GetRandomGPCode(r); // some have release tickets
                config.SaveToDatabase();

                // average of 8 Catalogues per extraction
                var numberOfsds = GetGaussianInt(0, 16);
                for (var s = 0; s < numberOfsds; s++)
                {
                    var ds = ExtractableDatasets.GetRandom(r);
                    config.AddDatasetToConfiguration(ds, out var sds);

                    var request = new CohortCreationRequest(p,
                        new CohortDefinition(null, "Created by NightmareDatasets", 1, p.ProjectNumber.Value, ect),
                        _repos.DataExportRepository,
                        $"Nightmare dreamed on {DateTime.Now}");

                    request.PushToServer(con);

                    var cohort = new ExtractableCohort(_repos.DataExportRepository, ect,
                        request.NewCohortDefinition.ID.Value);
                    config.Cohort_ID = cohort.ID;
                    config.SaveToDatabase();

                    // FilterContainer     4370
                    // most selected datasets have filters

                    // we picked the same dataset twice, oh well
                    if (sds == null)
                        continue;

                    // just add a single root container
                    if (sds.RootFilterContainer_ID == null)
                    {
                        sds.CreateRootContainerIfNotExists();
                        AddExtractionFiltersTo(sds.RootFilterContainer);
                    }

                    // 	FilterContainerSubcontainers 	271

                    // 5% have subcontainers
                    if (r.Next(20) == 0)
                    {
                        var subContainer = new FilterContainer(_repos.DataExportRepository);
                        sds.RootFilterContainer.AddChild(subContainer);
                        AddExtractionFiltersTo(subContainer);
                    }
                }
            }
        }

        // 200 cics
        for (var i = 0; i < 200 * Factor; i++)
        {
            var cic = new CohortIdentificationConfiguration(_repos.CatalogueRepository,
                $"Cohort Query {GetRandomGPCode(r)}");

            // 25% of cics are associated with a specific project
            if (r.Next(4) == 0)
                _ = new ProjectCohortIdentificationConfigurationAssociation(_repos.DataExportRepository,
                    Projects.GetRandom(r), cic);
        }
    }

    private void AddExtractionFiltersTo(IContainer container)
    {
        var numberOfFilters = GetGaussianInt(0, 2);
        for (var f = 0; f < numberOfFilters; f++)
        {
            var filter = new DeployedExtractionFilter(_repos.DataExportRepository,
                $"Filter {Guid.NewGuid()}", null)
            {
                WhereSQL = "ColX > 0"
            };
            filter.SaveToDatabase();

            container.AddChild(filter);
        }
    }

    private IEnumerable<ColumnInfo> CreateTable()
    {
        // 762 tables
        // 18415 columns
        // = average of 24 columns per table
        var ti = new TableInfo(_repos.CatalogueRepository, $"[MyDb].[Table{TablesCount++}]");

        // let's not set the server name on 1 in 20 so we get all those
        // horrible null references out in the open
        if (r.Next(20) != 0)
            ti.Server = _serverName;

        // let's not set the database name on 1 in 20 so we get all those
        // horrible null references out in the open
        if (r.Next(20) != 0)
            ti.Database =
                r.Next(2) == 0 ? _databaseNameRuntime : _databaseNameWrapped; // some are "[mydb]" some will be "mydb"

        // 1 in 20 tables is a view
        ti.IsView = r.Next(20) == 0;

        ti.SaveToDatabase();

        Tables.Add(1, ti);

        var numberOfColumns = GetGaussianInt(1, 48);
        for (var j = 0; j < numberOfColumns; j++)
            yield return new ColumnInfo(_repos.CatalogueRepository, $"MyCol{ColumnsCount++}", "varchar(10)", ti);
    }

    // we are not actually interested in these methods, just want to use GetGaussian etc
    public override object[] GenerateTestDataRow(Person p) => throw new NotSupportedException();

    protected override string[] GetHeaders() => throw new NotSupportedException();
}