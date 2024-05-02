// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
///     Command representing a desire to extract a given dataset in an ExtractionConfiguration through an extraction
///     pipeline.  This includes bundled content
///     (Lookup tables, SupportingDocuments etc).  Also includes optional settings (e.g. IncludeValidation) etc.  You can
///     realise the request by running the
///     QueryBuilder SQL.
/// </summary>
public class ExtractDatasetCommand : ExtractCommand, IExtractDatasetCommand
{
    public ISelectedDataSets SelectedDataSets { get; set; }

    private IExtractableDatasetBundle _datasetBundle;
    private readonly List<IColumn> _origColumnsToExtract;

    public IExtractableCohort ExtractableCohort { get; set; }

    public IExtractableDatasetBundle DatasetBundle
    {
        get => _datasetBundle;
        set
        {
            _datasetBundle = value;

            Catalogue = value == null
                ? null
                : DataExportRepository.CatalogueRepository.GetObjectByID<Catalogue>(value.DataSet.Catalogue_ID);
        }
    }

    public IDataExportRepository DataExportRepository { get; set; }

    public List<IColumn> ColumnsToExtract { get; set; }
    public IHICProjectSalt Salt { get; set; }
    public bool IncludeValidation { get; set; }

    public IExtractionDirectory Directory { get; set; }
    public ICatalogue Catalogue { get; private set; }

    public ISqlQueryBuilder QueryBuilder { get; set; }
    public ICumulativeExtractionResults CumulativeExtractionResults { get; set; }
    public List<ReleaseIdentifierSubstitution> ReleaseIdentifierSubstitutions { get; private set; }
    public List<IExtractionResults> ExtractionResults { get; private set; }
    public int TopX { get; set; }

    /// <inheritdoc />
    public DateTime? BatchStart { get; set; }

    /// <inheritdoc />
    public DateTime? BatchEnd { get; set; }

    public ExtractDatasetCommand(IExtractionConfiguration configuration, IExtractableCohort extractableCohort,
        IExtractableDatasetBundle datasetBundle, List<IColumn> columnsToExtract, IHICProjectSalt salt,
        IExtractionDirectory directory, bool includeValidation = false, bool includeLookups = false) : this(
        configuration, datasetBundle.DataSet)
    {
        DataExportRepository = configuration.DataExportRepository;
        ExtractableCohort = extractableCohort;
        DatasetBundle = datasetBundle;
        ColumnsToExtract = columnsToExtract;

        // create a copy of the columns so we can support Reset()
        _origColumnsToExtract = ColumnsToExtract.ToList();

        Salt = salt;
        Directory = directory;
        IncludeValidation = includeValidation;
        TopX = -1;
    }


    /// <summary>
    ///     This version has less arguments because it goes back to the database and queries the configuration and explores who
    ///     the cohort is etc, it will result in more database
    ///     queries than the more explicit constructor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="datasetBundle"></param>
    /// <param name="includeValidation"></param>
    /// <param name="includeLookups"></param>
    public ExtractDatasetCommand(IExtractionConfiguration configuration, IExtractableDatasetBundle datasetBundle,
        bool includeValidation = false, bool includeLookups = false) : this(configuration, datasetBundle.DataSet)
    {
        DataExportRepository = configuration.DataExportRepository;
        //ExtractableCohort = ExtractableCohort.GetExtractableCohortByID((int) configuration.Cohort_ID);
        ExtractableCohort = configuration.GetExtractableCohort();
        DatasetBundle = datasetBundle;
        ColumnsToExtract = new List<IColumn>(Configuration.GetAllExtractableColumnsFor(datasetBundle.DataSet));

        // create a copy of the columns so we can support Reset()
        _origColumnsToExtract = ColumnsToExtract.ToList();

        Salt = new HICProjectSalt(Project);
        Directory = new ExtractionDirectory(Project.ExtractionDirectory, configuration);
        IncludeValidation = includeValidation;
        TopX = -1;
    }

    public static readonly ExtractDatasetCommand EmptyCommand = new();


    private ExtractDatasetCommand(IExtractionConfiguration configuration, IExtractableDataSet dataset) : base(
        configuration)
    {
        var selectedDataSets = configuration.SelectedDataSets.Where(ds => ds.ExtractableDataSet_ID == dataset.ID)
            .ToArray();

        if (selectedDataSets.Length != 1)
            throw new Exception(
                $"Could not find 1 ISelectedDataSets for ExtractionConfiguration '{configuration}' | Dataset '{dataset}'");

        SelectedDataSets = selectedDataSets[0];

        ExtractionResults = new List<IExtractionResults>();
    }

    private ExtractDatasetCommand() : base(null)
    {
    }

    /// <summary>
    ///     Resets the state of the command to when it was first constructed
    /// </summary>
    public void Reset()
    {
        ColumnsToExtract = _origColumnsToExtract.ToList();
        QueryBuilder = null;
    }

    public void GenerateQueryBuilder()
    {
        var host = new ExtractionQueryBuilder(DataExportRepository);
        QueryBuilder = host.GetSQLCommandForFullExtractionSet(this, out var substitutions);
        ReleaseIdentifierSubstitutions = substitutions;
    }

    public override string ToString()
    {
        return this == EmptyCommand ? "EmptyCommand" : DatasetBundle.DataSet.ToString();
    }

    public override DirectoryInfo GetExtractionDirectory()
    {
        return this == EmptyCommand
            ? new DirectoryInfo(Path.GetTempPath())
            : Directory.GetDirectoryForDataset(DatasetBundle.DataSet);
    }

    public override string DescribeExtractionImplementation()
    {
        return QueryBuilder.SQL;
    }

    /// <inheritdoc />
    public DiscoveredServer GetDistinctLiveDatabaseServer()
    {
        IDataAccessPoint[] points = QueryBuilder?.TablesUsedInQuery != null
            ? QueryBuilder.TablesUsedInQuery.ToArray()
            : //get it from the request if it has been built
            Catalogue.GetTableInfoList(false); //or from the Catalogue directly if the query hasn't been built

        var singleServer = new DataAccessPointCollection(true, DataAccessContext.DataExport);
        singleServer.AddRange(points);

        return singleServer.GetDistinctServer();
    }
}