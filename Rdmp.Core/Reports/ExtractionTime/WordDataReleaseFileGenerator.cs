// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NPOI.XWPF.UserModel;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Reports.ExtractionTime;

/// <summary>
/// Generates a Microsoft Word DocX file containing information about all the datasets extracted (and released) as part of a Data Release.  This includes
/// row counts and unique patient counts as well as the number of patients in the original cohort (not all patients will appear in all datasets).  Also
/// included are the tickets for the project, the cohort ID number etc
/// </summary>
public class WordDataReleaseFileGenerator : DocXHelper
{
    private readonly IDataExportRepository _repository;
    public IExtractionConfiguration Configuration { get; set; }
    protected ICumulativeExtractionResults[] ExtractionResults { get; set; }
    protected IExtractableCohort Cohort { get; set; }
    protected IProject Project { get; set; }

    private const int CohortCountTimeoutInSeconds = 600; // 10 minutes

    public WordDataReleaseFileGenerator(IExtractionConfiguration configuration, IDataExportRepository repository)
    {
        _repository = repository;
        Configuration = configuration;
        Project = configuration.Project;

        if (Configuration.Cohort_ID == null)
            throw new NullReferenceException("Configuration has no Cohort");

        Cohort = _repository.GetObjectByID<ExtractableCohort>((int)Configuration.Cohort_ID);

        ExtractionResults =
            Configuration.CumulativeExtractionResults
                .OrderBy(
                    c => _repository.GetObjectByID<ExtractableDataSet>(c.ExtractableDataSet_ID).ToString()
                ).ToArray();
    }

    public void GenerateWordFile(string saveAsFilename)
    {
        var f = string.IsNullOrWhiteSpace(saveAsFilename)
            ? GetUniqueFilenameInWorkArea("ReleaseDocument")
            : new FileInfo(saveAsFilename);

        // Create an instance of Word  and make it visible.=
        using var document = GetNewDocFile(f);
        //actually changes it to landscape :)
        SetLandscape(document);

        InsertHeader(document, $"Project:{Project.Name}", 1);
        InsertHeader(document, Configuration.Name, 2);

        var disclaimer = _repository.DataExportPropertyManager.GetValue(DataExportProperty.ReleaseDocumentDisclaimer);

        if (disclaimer != null)
            InsertParagraph(document, disclaimer);

        CreateTopTable1(document);

        InsertParagraph(document, Environment.NewLine);

        CreateCohortDetailsTable(document);

        InsertParagraph(document, Environment.NewLine);

        CreateFileSummary(document);

        //interactive mode, user didn't ask us to save to a specific location so we created it in temp and so we can now show them where that file is
        if (string.IsNullOrWhiteSpace(saveAsFilename))
            ShowFile(f);
    }

    private void CreateTopTable1(XWPFDocument document)
    {
        var hasTicket = !string.IsNullOrWhiteSpace(Project.MasterTicket);
        var hasProchi = Cohort.GetReleaseIdentifier().ToLower().Contains("prochi");

        var currentRow = 0;
        var requiredRows = 1;

        if (hasProchi)
            requiredRows++;
        if (hasTicket)
            requiredRows++;

        var table = InsertTable(document, requiredRows, 2);

        if (hasTicket)
        {
            SetTableCell(table, currentRow, 0, "Master Issue");
            SetTableCell(table, currentRow, 1, Project.MasterTicket);
            currentRow++;
        }

        SetTableCell(table, currentRow, 0, "ReleaseIdentifier");
        SetTableCell(table, currentRow, 1, Cohort.GetReleaseIdentifier(true));
        currentRow++;

        if (hasProchi)
        {
            SetTableCell(table, currentRow, 0, "Prefix");
            SetTableCell(table, currentRow, 1, GetFirstProCHIPrefix());
        }
    }

    /// <summary>
    /// Returns the first 3 digits of the first release identifier in the cohort (this is very hic specific).
    /// </summary>
    /// <returns></returns>
    private string GetFirstProCHIPrefix()
    {
        var ect = Cohort.ExternalCohortTable;

        var db = ect.Discover();
        using var con = db.Server.GetConnection();
        con.Open();

        var sql =
            $"SELECT  TOP 1 LEFT({Cohort.GetReleaseIdentifier()},3) FROM {ect.TableName} WHERE {Cohort.WhereSQL()}";

        using var cmd = db.Server.GetCommand(sql, con);
        cmd.CommandTimeout = CohortCountTimeoutInSeconds;
        return (string)cmd.ExecuteScalar();
    }

    private void CreateCohortDetailsTable(XWPFDocument document)
    {
        var table = InsertTable(document, 2, 4);

        var tableLine = 0;

        SetTableCell(table, tableLine, 0, "Version");
        SetTableCell(table, tableLine, 1, "Description");
        SetTableCell(table, tableLine, 2, "Date Extracted");
        SetTableCell(table, tableLine, 3, "Unique Individuals");
        tableLine++;

        SetTableCell(table, tableLine, 0,
            Cohort.GetExternalData(CohortCountTimeoutInSeconds).ExternalVersion.ToString());
        SetTableCell(table, tableLine, 1,
            $"{Cohort} (ID={Cohort.ID}, OriginID={Cohort.OriginID})"); //description fetched from remote table

        var lastExtracted = ExtractionResults.Any()
            ? ExtractionResults.Max(r => r.DateOfExtraction).ToString(CultureInfo.CurrentCulture)
            : "Never";
        SetTableCell(table, tableLine, 2, lastExtracted);
        SetTableCell(table, tableLine, 3,
            Cohort.GetCountDistinctFromDatabase(CohortCountTimeoutInSeconds).ToString("N0"));
    }

    [NotNull]
    private string getDOI([NotNull] Curation.Data.Datasets.Dataset ds)
    {
        return !string.IsNullOrWhiteSpace(ds.DigitalObjectIdentifier) ? $" (DOI: {ds.DigitalObjectIdentifier})" : "";
    }

    private void CreateFileSummary(XWPFDocument document)
    {
        var table = InsertTable(document, ExtractionResults.Length + 1, 6);

        var tableLine = 0;

        SetTableCell(table, tableLine, 0, "Data Requirement");
        SetTableCell(table, tableLine, 1, "Notes");
        SetTableCell(table, tableLine, 2, "Filename");
        SetTableCell(table, tableLine, 3, "Records");
        SetTableCell(table, tableLine, 4, "Unique Individuals");
        SetTableCell(table, tableLine, 5, "Datasets");
        tableLine++;

        foreach (var result in ExtractionResults)
        {
            var filename = GetFileName(result);
            var extractableDataset = _repository.GetObjectByID<ExtractableDataSet>(result.ExtractableDataSet_ID);
            SetTableCell(table, tableLine, 0,
              extractableDataset.ToString());
            var linkedDatasets = extractableDataset.Catalogue.CatalogueItems.Select(static c => c.ColumnInfo).Where(ci => ci.Dataset_ID != null).Distinct().Select(ci => ci.Dataset_ID);
            var datasets = _repository.CatalogueRepository.GetAllObjects<Curation.Data.Datasets.Dataset>().Where(d => linkedDatasets.Contains(d.ID)).ToList();
            var datasetString = string.Join("",datasets.Select(ds=> $"{ds.Name} {getDOI(ds)}, {Environment.NewLine}"));
            SetTableCell(table, tableLine, 1, result.FiltersUsed);
            SetTableCell(table, tableLine, 2, filename);
            SetTableCell(table, tableLine, 3, result.RecordsExtracted.ToString("N0"));
            SetTableCell(table, tableLine, 4, result.DistinctReleaseIdentifiersEncountered.ToString("N0"));
            SetTableCell(table, tableLine, 5, datasetString);
            tableLine++;
        }
    }

    private string GetFileName(ICumulativeExtractionResults result)
    {
        if (string.IsNullOrWhiteSpace(result?.DestinationDescription))
            return "";

        if (string.IsNullOrWhiteSpace(Project.ExtractionDirectory))
            return result.DestinationDescription;

        //can we express it relative
        if (result.DestinationDescription.StartsWith(Project.ExtractionDirectory,
                StringComparison.CurrentCultureIgnoreCase))
        {
            var relative = result.DestinationDescription[Project.ExtractionDirectory.Length..].Replace('\\', '/');

            return $"./{relative.Trim('/')}";
        }

        return result.DestinationDescription;
    }
}