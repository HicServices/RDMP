// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Processes all GovernancePeriod and Catalogues into a CSV report about which datasets are covered by which governance periods, which periods have expired (and there
    /// is no corresponding follow on GovernancePeriod) and which Catalogues are not covered by any governance.
    /// </summary>
    public class GovernanceReport:DocXHelper
    {
        private readonly IDetermineDatasetTimespan _timespanCalculator;
        private readonly CatalogueRepository _repository;
        
        public GovernanceReport(IDetermineDatasetTimespan timespanCalculator,CatalogueRepository repository)
        {
            _timespanCalculator = timespanCalculator;
            _repository = repository;
        }

        public void GenerateReport()
        {
            StringBuilder sb = new StringBuilder();
            
            //first line of file
            sb.AppendLine("Extractable Datasets");
            sb.AppendLine("Folder,Catalogue,Current Governance,Dataset Period,Description");
            
            Dictionary<GovernancePeriod, ICatalogue[]> govs = _repository.GetAllObjects<GovernancePeriod>().ToDictionary(period => period, period => period.GovernedCatalogues.ToArray());
            
            foreach (Catalogue catalogue in _repository.GetAllCataloguesWithAtLeastOneExtractableItem())
            {
                if (catalogue.IsDeprecated || catalogue.IsColdStorageDataset || catalogue.IsInternalDataset)
                    continue;

                //get active governances
                var activeGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && !kvp.Key.IsExpired()).Select(g=>g.Key).ToArray();
                var expiredGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && kvp.Key.IsExpired()).Select(g => g.Key).ToArray();

                string relevantGovernance = "\"";

                if (activeGovs.Any())
                    relevantGovernance += string.Join("," , activeGovs.Select(gov => gov.Name));
                else if (expiredGovs.Any())
                    relevantGovernance += "No Current Governance (Expired Governances: " + string.Join(",", expiredGovs.Select(gov => gov.Name)) + ")";
                else
                    relevantGovernance += "No Governance Required";

                relevantGovernance += "\"";

                //write the results out to Excel
                sb.Append(catalogue.Folder).Append(",");
                sb.Append(catalogue.Name).Append(",");
                sb.Append(relevantGovernance).Append(",");
                sb.Append(_timespanCalculator.GetHumanReadableTimepsanIfKnownOf(catalogue,true)).Append(",");
                sb.Append(ShortenDescription(catalogue.Description)).AppendLine();
            }

            sb.AppendLine();
            
            // next section header
            sb.AppendLine("Active Governance");
            
            OutputGovernanceList(govs,sb, false);

            //take a blank line
            sb.AppendLine();

            // next section header
            sb.AppendLine("Expired Governance");

            OutputGovernanceList(govs,sb, true);

            var f = GetUniqueFilenameInWorkArea("GovernanceReport", ".csv");

            File.WriteAllText(f.FullName, sb.ToString());
            ShowFile(f);
        }

        private string ShortenDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return description;

            var toReturn = description.Replace(Environment.NewLine, " ");

            if (toReturn.Length >= 100)
                return toReturn.Substring(0, 100) + "...";
            else
                return toReturn;
        }

        /// <summary>
        /// Pass false for active or true for expired
        /// </summary>
        /// <param name="govs"></param>
        /// <param name="expired"></param>
        private void OutputGovernanceList(Dictionary<GovernancePeriod, ICatalogue[]> govs, StringBuilder sb, bool expired)
        {
            //headers for this section
            sb.AppendLine("Governance Period Name,Catalogues,Approval Start,Approval End,Documents");
            
            foreach (KeyValuePair<GovernancePeriod, ICatalogue[]> kvp in govs)
            {
                //if governance period does not have any Catalogues associated with it skip it
                if (!kvp.Value.Any())
                    continue;

                //if it is expiry does not match the callers desired expiry state then skip it
                if (kvp.Key.IsExpired() != expired)
                    continue;

                sb.Append(kvp.Key.Name).Append(",");
                sb.Append("\""+string.Join(",", kvp.Value.Select(cata => cata.Name))).Append("\",");
                sb.Append(kvp.Key.StartDate).Append(",");
                sb.Append(kvp.Key.EndDate == null ? "Never Expires" : kvp.Key.EndDate.ToString()).Append(",");
                sb.Append("\""+ string.Join(",", kvp.Key.GovernanceDocuments.Select(doc => doc.GetFilenameOnly()))).AppendLine("\"");
            }
        }
    }
}
