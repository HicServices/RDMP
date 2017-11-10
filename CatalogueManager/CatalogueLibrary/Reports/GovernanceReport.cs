using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;
using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Reports
{
    public class GovernanceReport:RequiresMicrosoftOffice
    {
        /*
        private readonly IDetermineDatasetTimespan _timespanCalculator;
        private readonly CatalogueRepository _repository;
        private Microsoft.Office.Interop.Excel.Application xlApp;
        private object _missing = false;

        public GovernanceReport(IDetermineDatasetTimespan timespanCalculator,CatalogueRepository repository)
        {
            _timespanCalculator = timespanCalculator;
            _repository = repository;
        }

        public void GenerateReport()
        {
            xlApp = new Microsoft.Office.Interop.Excel.Application();

            xlApp.Visible = false;
            xlApp.UserControl = false;

            Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet ws = (Worksheet) wb.Worksheets[1];


            if (ws == null)
            {
                Console.WriteLine("Worksheet could not be created. Check that your office installation and project references are correct.");
            }

            //excel indexes cells from 1
            int currentRow = 1;

            //first line of file
            ws.Cells[currentRow, 1] = "Extractable Datasets";


            ws.Cells[currentRow, 1] = "Folder"; 
            ws.Cells[currentRow, 2] = "Catalogue";
            ws.Cells[currentRow, 3] = "Current Governance";
            ws.Cells[currentRow, 4] = "Dataset Period";
            ws.Cells[currentRow, 5] = "Description";
            currentRow++;


            Dictionary<GovernancePeriod, Catalogue[]> govs = _repository.GetAllObjects<GovernancePeriod>().ToDictionary(period => period, period => period.GovernedCatalogues.ToArray());


            foreach (Catalogue catalogue in _repository.GetAllCataloguesWithAtLeastOneExtractableItem())
            {
                if (catalogue.IsDeprecated || catalogue.IsColdStorageDataset || catalogue.IsInternalDataset)
                    continue;

                //get active governances
                var activeGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && !kvp.Key.IsExpired()).Select(g=>g.Key).ToArray();
                var expiredGovs = govs.Where(kvp => kvp.Value.Contains(catalogue) && kvp.Key.IsExpired()).Select(g => g.Key).ToArray();

                string relevantGovernance = "";

                if (activeGovs.Any())
                    relevantGovernance = string.Join("," , activeGovs.Select(gov => gov.Name));
                else if (expiredGovs.Any())
                    relevantGovernance = "No Current Governance (Expired Governances: " + string.Join(",", expiredGovs.Select(gov => gov.Name)) + ")";
                else
                    relevantGovernance = "No Governance Required";

                //write the results out to Excel
                ws.Cells[currentRow, 1] = catalogue.Folder;
                ws.Cells[currentRow, 2] = catalogue.Name;
                ws.Cells[currentRow, 3] = relevantGovernance;
                ws.Cells[currentRow, 4] = _timespanCalculator.GetHumanReadableTimepsanIfKnownOf(catalogue,true);
                ws.Cells[currentRow, 5] = ShortenDescription(catalogue.Description);

                //next line
                currentRow++;
            }

            //take a blank line
            currentRow++;
            
            // next section header
            ws.Cells[currentRow, 1] = "Active Governance";
            currentRow++;

            OutputGovernanceList(govs,ws, ref currentRow, false);

            //take a blank line
            currentRow++;

            // next section header
            ws.Cells[currentRow, 1] = "Expired Governance";
            currentRow++;

            OutputGovernanceList(govs,ws, ref currentRow, true);

            xlApp.Visible = true;
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
        /// <param name="ws"></param>
        /// <param name="currentRow"></param>
        /// <param name="expired"></param>
        private void OutputGovernanceList(Dictionary<GovernancePeriod, Catalogue[]> govs, Worksheet ws, ref int currentRow, bool expired)
        {
            //headers for this section
            ws.Cells[currentRow, 1] = "Governance Period Name";
            ws.Cells[currentRow, 2] = "Catalogues";
            ws.Cells[currentRow, 3] = "Approval Start";
            ws.Cells[currentRow, 4] = "Approval End";
            ws.Cells[currentRow, 5] = "Documents";
            currentRow++;
            
            foreach (KeyValuePair<GovernancePeriod, Catalogue[]> kvp in govs)
            {
                //if governance period does not have any Catalogues associated with it skip it
                if (!kvp.Value.Any())
                    continue;

                //if it is expiry does not match the callers desired expiry state then skip it
                if (kvp.Key.IsExpired() != expired)
                    continue;

                ws.Cells[currentRow, 1] = kvp.Key.Name;
                ws.Cells[currentRow, 2] = string.Join(",", kvp.Value.Select(cata => cata.Name));
                ws.Cells[currentRow, 3] = kvp.Key.StartDate;
                ws.Cells[currentRow, 4] = kvp.Key.EndDate == null ? "Never Expires" : kvp.Key.EndDate.ToString();
                ws.Cells[currentRow, 5] = string.Join(",", kvp.Key.GovernanceDocuments.Select(doc => doc.GetFilenameOnly()));
                currentRow++;
            }
        }*/
    }
}
