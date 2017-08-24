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
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;

namespace DataExportLibrary.ExtractionTime
{
    public class WordDataReleaseFileGenerator : RequiresMicrosoftOffice
    {
        private readonly IRepository _repository;
        public IExtractionConfiguration Configuration { get; set; }
        protected ICumulativeExtractionResults[] ExtractionResults { get; set; }
        protected IExtractableCohort Cohort { get; set; }
        protected IProject Project { get; set; }
        
        private WordHelper _wordHelper;

        #region stuff for Word
        private object oTrue = true;
        private object oFalse = false;
        private Object oMissing = System.Reflection.Missing.Value;

        private Application _wrdApp;
        private _Document _wrdDoc;
        #endregion


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
            _wrdApp = new Microsoft.Office.Interop.Word.Application();

            //caller wants to save the document so dont display it onscreen
            if (!string.IsNullOrWhiteSpace(saveAsFilename))
            {
                _wrdApp.Visible = false;
                _wrdApp.DisplayAlerts = WdAlertLevel.wdAlertsNone;
            }
            else
            {
                _wrdApp.Visible = true;
            }
            //add blank new word
            _wrdDoc = _wrdApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            try
            {
                _wrdDoc.Select();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("RETRYLATER"))
                    Thread.Sleep(2000);

                _wrdDoc.Select();
            }
            
            //actually changes it to landscape :)
            _wrdDoc.PageSetup.TogglePortrait();
            
            _wordHelper = new WordHelper(_wrdApp);

            string disclaimer = new ConfigurationProperties(false, _repository)
                .TryGetValue(ConfigurationProperties.ExpectedProperties.ReleaseDocumentDisclaimer);

            _wordHelper.WriteLine(disclaimer,WdBuiltinStyle.wdStyleNormal);

            //change pen back to black
            _wordHelper.Write("", WdBuiltinStyle.wdStyleNormal);

            CreateTopTable1();

            var users = Project.DataUsers.ToArray();
            
            _wordHelper.GoToEndOfDocument();
            if(users.Any())
                _wordHelper.WriteLine("Data User(s):" + string.Join(",",users.Select(u=>u.ToString())),WdBuiltinStyle.wdStyleNormal);
            
            _wordHelper.GoToEndOfDocument();
            _wordHelper.WriteLine("", WdBuiltinStyle.wdStyleNormal);

            CreateCohortDetailsTable();

            _wordHelper.GoToEndOfDocument();
            _wordHelper.WriteLine("", WdBuiltinStyle.wdStyleNormal, WdColor.wdColorBlack);
            
            CreateFileSummary();

            if (!string.IsNullOrWhiteSpace(saveAsFilename))
            {
                _wrdDoc.SaveAs(saveAsFilename);
                _wrdDoc.Close();
                ((_Application)_wrdApp).Quit();
            }
        }

  

        private void CreateTopTable1()
        {

            object start = _wrdDoc.Content.End-1;
            object end = _wrdDoc.Content.End-1;

            Range tableLocation = _wrdDoc.Range(ref start, ref end);
            Table table = _wrdDoc.Tables.Add(tableLocation, 1, 5);
            table.set_Style("Table Grid");

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "Project:"+Environment.NewLine + Project.Name;
            table.Cell(tableLine, 2).Range.Text = "Master Issue:" +  Project.MasterTicket;
            table.Cell(tableLine, 3).Range.Text = "ReleaseIdentifier:" + SqlSyntaxHelper.GetRuntimeName(Cohort.GetReleaseIdentifier());
            table.Cell(tableLine, 4).Range.Text = "Configuration ID:" + Configuration.ID + Environment.NewLine + "Name:" + Configuration.Name;

            if (!Cohort.GetReleaseIdentifier().ToLower().Contains("prochi"))
                table.Cell(tableLine, 5).Range.Text = "Prefix:N/A";
            else
                table.Cell(tableLine, 5).Range.Text = "Prefix:"+Cohort.GetFirstProCHIPrefix();
            tableLine++;

        }

        private void CreateCohortDetailsTable()
        {
            object start = _wrdDoc.Content.End - 1;
            object end = _wrdDoc.Content.End - 1;

            Range tableLocation = _wrdDoc.Range(ref start, ref end);
            Table table = _wrdDoc.Tables.Add(tableLocation, 2, 6);
            table.set_Style("Table Grid");

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "Cohort ID (DataExportManager)";
            table.Cell(tableLine, 2).Range.Text = "Origin ID (External)";

            table.Cell(tableLine, 3).Range.Text = "Version";
            table.Cell(tableLine, 4).Range.Text = "Description";
            table.Cell(tableLine, 5).Range.Text = "dtCreated";
            table.Cell(tableLine, 6).Range.Text = "Unique Patient Counts";
            tableLine++;


            table.Cell(tableLine, 1).Range.Text = Cohort.ID.ToString();
            table.Cell(tableLine, 2).Range.Text = Cohort.OriginID.ToString();
            table.Cell(tableLine, 3).Range.Text = Cohort.GetExternalData().ExternalVersion.ToString();
            table.Cell(tableLine, 4).Range.Text = Cohort.ToString();//description fetched from CONSUS
            table.Cell(tableLine, 5).Range.Text = ExtractionResults.Max(r => r.DateOfExtraction).ToString();
            table.Cell(tableLine, 6).Range.Text = Cohort.CountDistinct.ToString();
            tableLine++;
        }

        private void CreateFileSummary()
        {
            object start = _wrdDoc.Content.End - 1;
            object end = _wrdDoc.Content.End - 1;

            Range tableLocation = _wrdDoc.Range(ref start, ref end);
            Table table = _wrdDoc.Tables.Add(tableLocation, ExtractionResults.Length + 1, 5);
            table.set_Style("Table Grid");

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "Data Requirement";
            table.Cell(tableLine, 2).Range.Text = "Notes";
            table.Cell(tableLine, 3).Range.Text = "Filename";
            table.Cell(tableLine, 4).Range.Text = "No. of records extracted";
            table.Cell(tableLine, 5).Range.Text = "Unique Patient Counts";
            tableLine++;

            foreach (CumulativeExtractionResults result in ExtractionResults)
            {

                string filename = "";

                if (!string.IsNullOrWhiteSpace(result.Filename))
                    filename = new FileInfo(result.Filename).Name;
                else
                    filename = "N/A";
                

                table.Cell(tableLine, 1).Range.Text = _repository.GetObjectByID<ExtractableDataSet>(result.ExtractableDataSet_ID).ToString();
                table.Cell(tableLine, 2).Range.Text = result.FiltersUsed;
                table.Cell(tableLine, 3).Range.Text = filename;
                table.Cell(tableLine, 4).Range.Text = result.RecordsExtracted.ToString();
                table.Cell(tableLine, 5).Range.Text = result.DistinctReleaseIdentifiersEncountered.ToString();
                tableLine++;
            }
           
        }

    }
}
