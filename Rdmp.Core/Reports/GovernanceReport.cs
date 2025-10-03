// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Reports;

/// <summary>
/// Processes all GovernancePeriod and Catalogues into a CSV report about which datasets are covered by which governance periods, which periods have expired (and there
/// is no corresponding follow on GovernancePeriod) and which Catalogues are not covered by any governance.
/// </summary>
public class GovernanceReport : DocXHelper
{
    private readonly IDetermineDatasetTimespan _timespanCalculator;
    private readonly ICatalogueRepository _repository;

    private readonly CsvConfiguration _csvConfig = new(CultureInfo.CurrentCulture)
    {
        Delimiter = ","
    };

    public GovernanceReport(IDetermineDatasetTimespan timespanCalculator, ICatalogueRepository repository)
    {
        _timespanCalculator = timespanCalculator;
        _repository = repository;
    }

    public void GenerateReport()
    {
        var f = GetUniqueFilenameInWorkArea("GovernanceReport", ".csv");

        using (var s = new StreamWriter(f.FullName))
        {
            using var writer = new CsvWriter(s, _csvConfig);
            writer.WriteField("Extractable Datasets");
            writer.NextRecord();

            writer.WriteField("Folder");
            writer.WriteField("Catalogue");
            writer.WriteField("Current Governance");
            writer.WriteField("Dataset Period");
            writer.WriteField("Description");
            writer.NextRecord();


            var govs = _repository.GetAllObjects<GovernancePeriod>()
                .ToDictionary(period => period, period => period.GovernedCatalogues.ToArray());

            foreach (var catalogue in _repository.GetAllObjects<Catalogue>()
                         .Where(c => c.CatalogueItems.Any(ci => ci.ExtractionInformation != null))
                         .OrderBy(c => c.Name))
            {
                if (catalogue.IsDeprecated || catalogue.IsInternalDataset)
                    continue;

                //get active governances
                var activeGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && !kvp.Key.IsExpired())
                    .Select(g => g.Key).ToArray();
                var expiredGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && kvp.Key.IsExpired())
                    .Select(g => g.Key).ToArray();

                string relevantGovernance = null;

                if (activeGovs.Any())
                    relevantGovernance = string.Join(",", activeGovs.Select(gov => gov.Name));
                else if (expiredGovs.Any())
                    relevantGovernance =
                        $"No Current Governance (Expired Governances: {string.Join(",", expiredGovs.Select(gov => gov.Name))})";
                else
                    relevantGovernance = "No Governance Required";


                //write the results out to Excel
                writer.WriteField(catalogue.Folder);
                writer.WriteField(catalogue.Name);
                writer.WriteField(relevantGovernance);
                writer.WriteField(_timespanCalculator.GetHumanReadableTimespanIfKnownOf(catalogue, true, out _));
                writer.WriteField(ShortenDescription(catalogue.Description));

                writer.NextRecord();
            }

            writer.NextRecord();

            // next section header
            writer.WriteField("Active Governance");

            OutputGovernanceList(govs, writer, false);

            writer.NextRecord();
            // next section header
            writer.WriteField("Expired Governance");

            OutputGovernanceList(govs, writer, true);
        }

        ShowFile(f);
    }

    private static string ShortenDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return description;

        description = description.Replace("\r\n", " ");
        description = description.Replace("\n", " ");

        return description.Length >= 100 ? $"{description[..100]}..." : description;
    }

    /// <summary>
    /// Pass false for active or true for expired
    /// </summary>
    /// <param name="govs"></param>
    /// <param name="writer"></param>
    /// <param name="expired"></param>
    private static void OutputGovernanceList(Dictionary<GovernancePeriod, ICatalogue[]> govs, CsvWriter writer,
        bool expired)
    {
        //headers for this section
        writer.WriteField("Governance");
        writer.WriteField("Period Name");
        writer.WriteField("Catalogues");
        writer.WriteField("Approval Start");
        writer.WriteField("Approval End");
        writer.WriteField("Documents");
        writer.NextRecord();

        foreach (var kvp in govs)
        {
            //if governance period does not have any Catalogues associated with it skip it
            if (!kvp.Value.Any())
                continue;

            //if it is expiry does not match the callers desired expiry state then skip it
            if (kvp.Key.IsExpired() != expired)
                continue;

            writer.WriteField(kvp.Key.Name);
            writer.WriteField(string.Join(",", kvp.Value.Select(cata => cata.Name)));
            writer.WriteField(kvp.Key.StartDate);
            writer.WriteField(kvp.Key.EndDate == null ? "Never Expires" : kvp.Key.EndDate.ToString());
            writer.WriteField(string.Join(",", kvp.Key.GovernanceDocuments.Select(doc => doc.GetFilenameOnly())));

            writer.NextRecord();
        }
    }
}