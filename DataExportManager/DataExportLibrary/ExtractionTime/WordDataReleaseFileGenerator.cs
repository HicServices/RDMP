using System;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogueLibrary;
using CatalogueLibrary.Reports;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using Microsoft.Office.Interop.Excel;
using ReusableLibraryCode;
using Xceed.Words.NET;

namespace DataExportLibrary.ExtractionTime
{
    public class WordDataReleaseFileGenerator : RequiresMicrosoftOffice
    {
        private readonly IRepository _repository;
        public IExtractionConfiguration Configuration { get; set; }
        protected ICumulativeExtractionResults[] ExtractionResults { get; set; }
        protected IExtractableCohort Cohort { get; set; }
        protected IProject Project { get; set; }
        
        public WordDataReleaseFileGenerator(IExtractionConfiguration configuration, IRepository repository)
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
            // Create an instance of Word  and make it visible.=
            using (DocX document = DocX.Create(saveAsFilename??"ReleaseDocument.docx"))
            {
                
                //actually changes it to landscape :)
                document.PageLayout.Orientation = Orientation.Portrait;
                
                string disclaimer = new ConfigurationProperties(false, _repository)
                    .TryGetValue(ConfigurationProperties.ExpectedProperties.ReleaseDocumentDisclaimer);

                InsertParagraph(document,disclaimer);

                CreateTopTable1(document);

                var users = Project.DataUsers.ToArray();

                if (users.Any())
                    InsertParagraph(document,"Data User(s):" + string.Join(",", users.Select(u => u.ToString())));

                CreateCohortDetailsTable(document);

                CreateFileSummary(document);

                if (!string.IsNullOrWhiteSpace(saveAsFilename))
                    document.Save();
            }

        }

        private void CreateTopTable1(DocX document)
        {
            Table table = InsertTable(document,1, 5);

            int tableLine = 0;

            SetTableCell(table,tableLine, 0, "Project:"+Environment.NewLine + Project.Name);
            SetTableCell(table,tableLine, 1, "Master Issue:" +  Project.MasterTicket);
            SetTableCell(table,tableLine, 2, "ReleaseIdentifier:" + SqlSyntaxHelper.GetRuntimeName(Cohort.GetReleaseIdentifier()));
            SetTableCell(table,tableLine, 3, "Configuration ID:" + Configuration.ID + Environment.NewLine + "Name:" + Configuration.Name);

            if (!Cohort.GetReleaseIdentifier().ToLower().Contains("prochi"))
                SetTableCell(table,tableLine, 4,"Prefix:N/A");
            else
                SetTableCell(table,tableLine, 4, "Prefix:"+Cohort.GetFirstProCHIPrefix());
            tableLine++;
        }

        private void CreateCohortDetailsTable(DocX document)
        {
            Table table = InsertTable(document, 2, 6);
            
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
            Table table = InsertTable(document,ExtractionResults.Length + 1, 5);
            
            int tableLine = 0;

            SetTableCell(table,tableLine, 0, "Data Requirement");
            SetTableCell(table,tableLine, 1, "Notes");
            SetTableCell(table,tableLine, 2, "Filename");
            SetTableCell(table,tableLine, 3, "No. of records extracted");
            SetTableCell(table,tableLine, 4, "Unique Patient Counts");
            tableLine++;

            foreach (CumulativeExtractionResults result in ExtractionResults)
            {

                string filename = "";

                if (!string.IsNullOrWhiteSpace(result.Filename))
                    filename = new FileInfo(result.Filename).Name;
                else
                    filename = "N/A";


                SetTableCell(table,tableLine, 0,_repository.GetObjectByID<ExtractableDataSet>(result.ExtractableDataSet_ID).ToString());
                SetTableCell(table,tableLine, 1,result.FiltersUsed);
                SetTableCell(table,tableLine, 2,filename);
                SetTableCell(table,tableLine, 3,result.RecordsExtracted.ToString());
                SetTableCell(table,tableLine, 4,result.DistinctReleaseIdentifiersEncountered.ToString());
                tableLine++;
            }
           
        }

    }
}
