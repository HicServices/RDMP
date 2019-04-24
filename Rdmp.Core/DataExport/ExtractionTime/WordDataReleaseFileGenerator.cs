// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Reports;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.Core.CatalogueLibrary.Repositories.Managers;
using Rdmp.Core.DataExport.Data.DataTables;
using Xceed.Words.NET;

namespace Rdmp.Core.DataExport.ExtractionTime
{
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

        public WordDataReleaseFileGenerator(IExtractionConfiguration configuration, IDataExportRepository repository)
        {
            _repository = repository;
            Configuration = configuration;
            Project = configuration.Project;

            if (Configuration.Cohort_ID == null)
                throw new NullReferenceException("Configuration has no Cohort");

            Cohort = _repository.GetObjectByID<ExtractableCohort>((int) Configuration.Cohort_ID);

            ExtractionResults = 
                Configuration.CumulativeExtractionResults
                .OrderBy(
                    c => _repository.GetObjectByID<ExtractableDataSet>(c.ExtractableDataSet_ID).ToString()
                ).ToArray();
        }

        public void GenerateWordFile(string saveAsFilename)
        {

            FileInfo f;

            if (string.IsNullOrWhiteSpace(saveAsFilename))
                f = GetUniqueFilenameInWorkArea("ReleaseDocument");
            else
                f = new FileInfo(saveAsFilename);

            // Create an instance of Word  and make it visible.=
            using (DocX document = DocX.Create(f.FullName))
            {
                
                //actually changes it to landscape :)
                document.PageLayout.Orientation = Orientation.Landscape;
                
                string disclaimer = _repository.DataExportPropertyManager.GetValue(DataExportProperty.ReleaseDocumentDisclaimer);

                if(disclaimer != null)
                    InsertParagraph(document,disclaimer);

                CreateTopTable1(document);

                InsertParagraph(document, Environment.NewLine);

                CreateCohortDetailsTable(document);

                InsertParagraph(document,Environment.NewLine);

                CreateFileSummary(document);

                document.Save();
                
                //interactive mode, user didn't ask us to save to a specific location so we created it in temp and so we can now show them where that file is
                if (string.IsNullOrWhiteSpace(saveAsFilename))
                    ShowFile(f);
            }

        }

        private void CreateTopTable1(DocX document)
        {
            Table table = InsertTable(document, 1, 5, TableDesign.TableGrid);

            SetTableCell(table,0, 0, "Project:"+Environment.NewLine + Project.Name);
            SetTableCell(table,0, 1, "Master Issue:" +  Project.MasterTicket);
            SetTableCell(table,0, 2, "ReleaseIdentifier:" + Cohort.GetReleaseIdentifier(true));
            SetTableCell(table,0, 3, "Configuration ID:" + Configuration.ID + Environment.NewLine + "Name:" + Configuration.Name);

            if (!Cohort.GetReleaseIdentifier().ToLower().Contains("prochi"))
                SetTableCell(table,0, 4,"Prefix:N/A");
            else
                SetTableCell(table,0, 4, "Prefix:"+ GetFirstProCHIPrefix());
            
        }

        /// <summary>
        /// Returns the first 3 digits of the first release identifier in the cohort (this is very hic specific).
        /// </summary>
        /// <returns></returns>
        private string GetFirstProCHIPrefix()
        {
            var ect = Cohort.ExternalCohortTable;

            var db = ect.Discover();
            using (var con = db.Server.GetConnection())
            {
                con.Open();

                string sql = "SELECT  TOP 1 LEFT(" + Cohort.GetReleaseIdentifier() + ",3) FROM " + ect.TableName + " WHERE " + Cohort.WhereSQL();

                return (string)db.Server.GetCommand(sql, con).ExecuteScalar();
            }
        }

        private void CreateCohortDetailsTable(DocX document)
        {
            Table table = InsertTable(document, 2, 6, TableDesign.TableGrid);
            
            int tableLine = 0;

            SetTableCell(table,tableLine, 0,"Cohort ID (DataExportManager)");
            SetTableCell(table,tableLine, 1,"Origin ID (External)");
            SetTableCell(table,tableLine, 2, "Version");
            SetTableCell(table,tableLine, 3, "Description");
            SetTableCell(table,tableLine, 4, "dtCreated");
            SetTableCell(table,tableLine, 5, "Unique Patient Counts");
            tableLine++;
            
            SetTableCell(table,tableLine, 0, Cohort.ID.ToString());
            SetTableCell(table,tableLine, 1, Cohort.OriginID.ToString());
            SetTableCell(table,tableLine, 2, Cohort.GetExternalData().ExternalVersion.ToString());
            SetTableCell(table,tableLine, 3, Cohort.ToString());//description fetched from CONSUS
            SetTableCell(table,tableLine, 4, ExtractionResults.Max(r => r.DateOfExtraction).ToString());
            SetTableCell(table,tableLine, 5, Cohort.CountDistinct.ToString());
            tableLine++;
        }

        private void CreateFileSummary(DocX document)
        {
            Table table = InsertTable(document, ExtractionResults.Length + 1, 5, TableDesign.TableGrid);
            
            int tableLine = 0;

            SetTableCell(table,tableLine, 0, "Data Requirement");
            SetTableCell(table,tableLine, 1, "Notes");
            SetTableCell(table,tableLine, 2, "Filename");
            SetTableCell(table,tableLine, 3, "No. of records extracted");
            SetTableCell(table,tableLine, 4, "Unique Patient Counts");
            tableLine++;

            foreach (var result in ExtractionResults)
            {
                string filename = "";
                
                if (IsValidFilename(result.DestinationDescription))
                    filename = new FileInfo(result.DestinationDescription).Name;
                else
                    filename = result.DestinationDescription;

                SetTableCell(table,tableLine, 0,_repository.GetObjectByID<ExtractableDataSet>(result.ExtractableDataSet_ID).ToString());
                SetTableCell(table,tableLine, 1,result.FiltersUsed);
                SetTableCell(table,tableLine, 2,filename);
                SetTableCell(table,tableLine, 3,result.RecordsExtracted.ToString());
                SetTableCell(table,tableLine, 4,result.DistinctReleaseIdentifiersEncountered.ToString());
                tableLine++;
            }
           
        }

        private bool IsValidFilename(string candidateFilename)
        {
            return !string.IsNullOrEmpty(candidateFilename) &&
                   candidateFilename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                   !File.Exists(candidateFilename);
        }
    }
}
