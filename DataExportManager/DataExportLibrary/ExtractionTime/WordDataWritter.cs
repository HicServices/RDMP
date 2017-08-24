using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ADOX;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;
using Chart = Microsoft.Office.Interop.Word.Chart;
using IFilter = CatalogueLibrary.Data.IFilter;
using Path = System.IO.Path;
using Range = Microsoft.Office.Interop.Word.Range;
using Table = Microsoft.Office.Interop.Word.Table;
using XlAxisGroup = Microsoft.Office.Interop.Word.XlAxisGroup;
using XlChartType = Microsoft.Office.Core.XlChartType;
using XlDataLabelsType = Microsoft.Office.Interop.Word.XlDataLabelsType;
using _Application = Microsoft.Office.Interop.Word._Application;

namespace DataExportLibrary.ExtractionTime
{
    public class WordDataWritter : RequiresMicrosoftOffice
    {
        /// <summary>
        /// Set this to true if you want microsoft word to be visible while it is running Interop commands (will be very confusing for users so never ship this with true)
        /// </summary>
        public bool DEBUG_WORD = false;

        public ExtractionPipelineHost Executer { get; set; }

        #region stuff for Word
        object oTrue = true;
        object oFalse = false;
        Object oMissing = System.Reflection.Missing.Value;

        Microsoft.Office.Interop.Word.Application wrdApp;
        Microsoft.Office.Interop.Word._Document wrdDoc;
        #endregion

        public List<Exception> ExceptionsGeneratingWordFile = new List<Exception>();

        public WordDataWritter(ExtractionPipelineHost executer)
        {
            if(executer == null)
                throw new NullReferenceException("Cannot write meta data without the accompanying ExtractionPipelineHost");


            if (executer.Source.WasCancelled)
                throw new NullReferenceException("Cannot write meta data since ExtractionPipelineHost reports that it was Cancelled");
            
            Executer = executer;

            _destination = Executer.Destination as ExecuteDatasetExtractionFlatFileDestination;

            if(_destination == null)
                throw new NotSupportedException(GetType().FullName + " only supports destinations which are " + typeof(ExecuteDatasetExtractionFlatFileDestination).FullName);
        }
        
        WordHelper wordHelper;
        private string tableStyle = "Table Grid"; 

        private static object oLockOnWordUsage = new object();
        private ExecuteDatasetExtractionFlatFileDestination _destination;

        /// <summary>
        /// Generates a new meta data word file in the extraction directory and populates it with information about the extraction.
        /// It returns the open document as an object so that you can supplement it e.g. with catalogue information
        /// </summary>
        /// <returns></returns>
        public void GenerateWordFile()
        {
            lock (oLockOnWordUsage)
            {
                // Create an instance of Word  and make it visible.=
                wrdApp = new Microsoft.Office.Interop.Word.Application();
            
                //normally we hide word and suppress popups but it might be that word is being broken in which case we would want to watch it as it outputs stuff
                if (!DEBUG_WORD)
                {
                    wrdApp.Visible = false;
                    wrdApp.DisplayAlerts = WdAlertLevel.wdAlertsNone;
                }
                else
                {
                    wrdApp.Visible = true;
                }
                //add blank new word
                wrdDoc = wrdApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                try
                {
                    wrdDoc.Select();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("RETRYLATER"))
                        Thread.Sleep(2000);

                    wrdDoc.Select();
                }


           
                wordHelper = new WordHelper(wrdApp);


                //leave room for contents to be inserted (inserted at the end of this method but at the beginning of the word file - obviously)
                wordHelper.StartNewPageInDocument();

                //file data
                wordHelper.WriteLine("File Data",WdBuiltinStyle.wdStyleHeading1);

                object start = wrdApp.Selection.End;
                object end = wrdApp.Selection.End;
            
                Range tableLocation = wrdDoc.Range(ref start, ref end);
                Table table = wrdDoc.Tables.Add(tableLocation, 10, 2);
                table.set_Style(tableStyle);

                int tableLine = 1;

                table.Cell(tableLine, 1).Range.Text = "File Name";
                table.Cell(tableLine, 2).Range.Text = new FileInfo(_destination.OutputFile).Name;
                tableLine++;


                table.Cell(tableLine, 1).Range.Text = "Cohort Size (Distinct)";
                table.Cell(tableLine, 2).Range.Text = Executer.Source.Request.ExtractableCohort.CountDistinct.ToString();
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Cohorts Found In Dataset";
                table.Cell(tableLine, 2).Range.Text = Executer.Source.UniqueReleaseIdentifiersEncountered.Count.ToString();
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Dataset Line Count";
                table.Cell(tableLine, 2).Range.Text = Executer.Destination.TableLoadInfo.Inserts.ToString();
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "MD5";
                table.Cell(tableLine, 2).Range.Text = UsefulStuff.MD5File(_destination.OutputFile);
                tableLine++;

                FileInfo f = new FileInfo(_destination.OutputFile);
                table.Cell(tableLine, 1).Range.Text = "File Size";
                table.Cell(tableLine, 2).Range.Text = f.Length + "bytes ("+(f.Length/1024)+ "KB)";
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Extraction Date";
                table.Cell(tableLine, 2).Range.Text = Executer.Destination.TableLoadInfo.EndTime.ToString();
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Table Load ID (for HIC)";
                table.Cell(tableLine, 2).Range.Text = Executer.Destination.TableLoadInfo.ID.ToString();
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Separator";
                table.Cell(tableLine, 2).Range.Text = Executer.Source.Request.Configuration.Separator + "\t("+_destination.SeparatorsStrippedOut + " values stripped from data)";
                tableLine++;

                table.Cell(tableLine, 1).Range.Text = "Date Format";
                table.Cell(tableLine, 2).Range.Text = _destination.DateFormat;
                tableLine++;

                if (Executer.Source.ExtractionTimeValidator != null && Executer.Source.Request.IncludeValidation)
                {
                    wordHelper.StartNewPageInDocument();

                    //validation
                    wordHelper.WriteLine("Validation Information", WdBuiltinStyle.wdStyleHeading1);

                    CreateValidationRulesTable();
                    wordHelper.StartNewPageInDocument();


                    CreateValidationResultsTable();

                    //wordHelper.WriteLine("Total Invalid Rows:" + ExecuteThatRan.ExtractionTimeValidator.Results.CountOfRowsInvalidated, WdBuiltinStyle.wdStyleNormal);
                }

            
                //if a count of date times seen exists for this extraction create a graph of the counts seen
                if (Executer.Source.ExtractionTimeTimeCoverageAggregator != null && Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets.Any())
                {
                    try
                    {
                        wordHelper.StartNewPageInDocument();
                        wordHelper.WriteLine("Dataset Timespan", WdBuiltinStyle.wdStyleHeading1);
              
                            CreateTimespanGraph(Executer.Source.ExtractionTimeTimeCoverageAggregator);
                  
                    }
                    catch (Exception e)
                    {
                        ExceptionsGeneratingWordFile.Add(e);
                    }
                
                }
            
                wordHelper.StartNewPageInDocument();

                AddAllCatalogueItemMetaData();
           
                //technical data
                wordHelper.StartNewPageInDocument();
            
                AddFiltersAndParameters();
            
                AddIssuesForCatalogue(tableStyle);

                //insert table of contents
                Range myRange = wrdDoc.Range(0, 0);
                wrdDoc.TablesOfContents.Add(myRange);


                Thread.Sleep(1000);
            

                //insert title - after some whitespace
                wordHelper.GoToStartOfDocument();
                wordHelper.WriteLine();    
                wordHelper.GoToStartOfDocument();
                wordHelper.WriteLine(Executer.Source.Request.DatasetBundle.DataSet + " Meta Data", WdBuiltinStyle.wdStyleTitle);


                object outputFilename = Path.Combine(_destination.DirectoryPopulated.FullName, Executer.Source.Request.DatasetBundle.DataSet + ".docx");

                wrdDoc.TrackRevisions = true;
                wrdDoc.SaveAs(ref outputFilename);
                wrdDoc.Close(ref oFalse);
                ((_Application)wrdApp).Quit(ref oFalse);
            }
        }

        private void AddIssuesForCatalogue(string tableStyle)
        {
            var cata = Executer.Source.Request.Catalogue;
            WordCatalogueExtractor catalogueMetaData = new WordCatalogueExtractor(cata, wrdApp, wrdDoc);

            catalogueMetaData.AddIssuesForCatalogue(tableStyle);
        }

        private void AddFiltersAndParameters()
        {
            var request = Executer.Source.Request;
            FilterContainer fc = (FilterContainer)request.Configuration.GetFilterContainerFor(request.DatasetBundle.DataSet);

            if (fc != null)
            {
                List<IFilter> filtersUsed;
                filtersUsed = fc.GetAllFiltersIncludingInSubContainersRecursively();

                WriteOutFilters(filtersUsed);
                
                WriteOutParameters(filtersUsed);
            }
        }

        private void WriteOutParameters(List<IFilter> filtersUsed)
        {
            wordHelper.GoToEndOfDocument();
            wordHelper.WriteLine("Parameters", WdBuiltinStyle.wdStyleHeading1);
            wordHelper.GoToEndOfDocument();

            int linesRequred = filtersUsed.Aggregate(0, (s, f) => s + f.GetAllParameters().Count());

            var globalParameters = Executer.Source.Request.Configuration.GlobalExtractionFilterParameters.ToArray();
            linesRequred += globalParameters.Length;

            Table tableOfParameters = wordHelper.CreateTable(linesRequred + 1, 3, wrdDoc);
            tableOfParameters.Cell(1, 1).Range.Text = "Name";
            tableOfParameters.Cell(1, 2).Range.Text = "Comment";
            tableOfParameters.Cell(1, 3).Range.Text = "Value";
            int currentLine = 2;

            foreach (IFilter filter in filtersUsed)
                foreach (ISqlParameter parameter in filter.GetAllParameters())
                {
                    //i+2 because first row is for headers and indexing in word starts at 1 not 0
                    tableOfParameters.Cell(currentLine, 1).Range.Text = parameter.ParameterName;
                    tableOfParameters.Cell(currentLine, 2).Range.Text = parameter.Comment;
                    tableOfParameters.Cell(currentLine, 3).Range.Text = parameter.Value;
                    currentLine++;
                }
            foreach (ISqlParameter globalParameter in globalParameters)
            {
                tableOfParameters.Cell(currentLine, 1).Range.Text = globalParameter.ParameterName;
                tableOfParameters.Cell(currentLine, 2).Range.Text = globalParameter.Comment;
                tableOfParameters.Cell(currentLine, 3).Range.Text = globalParameter.Value;
                currentLine++;
            }

            tableOfParameters.set_Style(tableStyle);
        }

        private void  WriteOutFilters(List<IFilter> filtersUsed)
        {
            wordHelper.GoToEndOfDocument();
            wordHelper.WriteLine("Filters", WdBuiltinStyle.wdStyleHeading1);
            wordHelper.GoToEndOfDocument();

        
            Table tableOfFilters = wordHelper.CreateTable(filtersUsed.Count + 1, 3, wrdDoc);

            tableOfFilters.Cell(1, 1).Range.Text = "Name";
            tableOfFilters.Cell(1, 2).Range.Text = "Description";
            tableOfFilters.Cell(1, 3).Range.Text = "SQL";

            for (int i = 0; i < filtersUsed.Count; i++)
            {
                //i+2 because first row is for headers and indexing in word starts at 1 not 0
                tableOfFilters.Cell(i + 2, 1).Range.Text = filtersUsed[i].Name;
                tableOfFilters.Cell(i + 2, 2).Range.Text = filtersUsed[i].Description;
                tableOfFilters.Cell(i + 2, 3).Range.Text = filtersUsed[i].WhereSQL;
            }

            tableOfFilters.set_Style(tableStyle); 

        }

        private void AddAllCatalogueItemMetaData()
        {
            var cata = Executer.Source.Request.Catalogue;
            WordCatalogueExtractor catalogueMetaData = new WordCatalogueExtractor(cata,wrdApp,wrdDoc);
            
            var supplementalData = new Dictionary<CatalogueItem, Tuple<string, string>[]>();


            foreach (ExtractTimeTransformationObserved value in Executer.Source.ExtractTimeTransformationsObserved.Values)
            {
                List<Tuple<string,string>> supplementalValuesForThisOne = new List<Tuple<string, string>>();
             
                //Jim no longer wants these to appear in metadata
                /*
                if (value.FoundAtExtractTime)
                    supplementalValuesForThisOne.Add(new Tuple<string, string>("Runtime Name:", value.RuntimeName));
                else
                    supplementalValuesForThisOne.Add(new Tuple<string, string>("Runtime Name:", "Not found"));
                */

                supplementalValuesForThisOne.Add(new Tuple<string, string>("Datatype (SQL):",value.DataTypeInCatalogue));
                

                if(value.FoundAtExtractTime)
                    if(value.DataTypeObservedInRuntimeBuffer == null)
                        supplementalValuesForThisOne.Add(new Tuple<string, string>("Datatype:", "Value was always NULL"));
                    else
                        supplementalValuesForThisOne.Add(new Tuple<string, string>("Datatype:", value.DataTypeObservedInRuntimeBuffer.ToString()));
                

                //add it with supplemental values
                if(value.CatalogueItem != null)
                    supplementalData.Add(value.CatalogueItem ,supplementalValuesForThisOne.ToArray());
                
            }

            catalogueMetaData.AddMetaDataForColumns(supplementalData.Keys.ToArray(), tableStyle,supplementalData);
        }

        private void CreateTimespanGraph(ExtractionTimeTimeCoverageAggregator toGraph)
        {
            Chart wdChart = wrdDoc.InlineShapes.AddChart(Microsoft.Office.Core.XlChartType.xl3DColumn, ref oMissing).Chart;
            ChartData wdChartData = wdChart.ChartData;
            Workbook dataWorkbook = (Workbook)wdChartData.Workbook;
            dataWorkbook.Application.Visible = DEBUG_WORD;
            Worksheet dataSheet = (Worksheet)dataWorkbook.Worksheets[1];

            //set title before putting any data in
            wdChart.ApplyLayout(1);
            wdChart.ChartTitle.Text = "Dataset Timespan";
            wdChart.ChartTitle.Font.Italic = true;
            wdChart.ChartTitle.Font.Size = 18;
            wdChart.ChartTitle.Font.Color = Color.Black.ToArgb();

            //fill in sheet data
            dataSheet.Cells.Range["A1", oMissing].FormulaR1C1 = "Date";
            dataSheet.Cells.Range["B1", oMissing].FormulaR1C1 = "Record Count";
            dataSheet.Cells.Range["C1", oMissing].FormulaR1C1 = "Distinct Patient Count";

            //get rid of default microsoft crap
            dataSheet.Cells.Range["D1", oMissing].FormulaR1C1 = "";
            dataSheet.Cells.Range["D2", oMissing].FormulaR1C1 = "";
            dataSheet.Cells.Range["D3", oMissing].FormulaR1C1 = "";
            dataSheet.Cells.Range["D4", oMissing].FormulaR1C1 = "";
            dataSheet.Cells.Range["D5", oMissing].FormulaR1C1 = "";
       
            //get first of the month
            int futureRecordsCount = 0;
            int recordsPriorToDatasetStartCount = 0;

            int currentRow = 2;

            var cata = Executer.Source.Request.Catalogue;
            DateTime startDateCutoff;

            if (cata.DatasetStartDate == null)
                startDateCutoff = DateTime.MinValue;
            else
                startDateCutoff = (DateTime)cata.DatasetStartDate;

            foreach (DateTime key in Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets.Keys.OrderBy(d => d))
            {
                if (key < startDateCutoff)
                {
                    recordsPriorToDatasetStartCount += Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets[key].CountOfTimesSeen;
                    continue;
                }
                if (key > DateTime.Now)
                {
                    futureRecordsCount += Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets[key].CountOfTimesSeen;
                    continue;
                }

                dataSheet.Cells.Range["A" + currentRow, oMissing].FormulaR1C1 = key.ToString("yyyy-MM-dd");
                ExtractionTimeTimeCoverageAggregatorBucket currentBucket = Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets[key];
                dataSheet.Cells.Range["B" + currentRow, oMissing].FormulaR1C1 = currentBucket.CountOfTimesSeen;
                dataSheet.Cells.Range["C" + currentRow, oMissing].FormulaR1C1 = currentBucket.CountOfDistinctIdentifiers;

                currentRow++;
            }

            //set max size of sheet
            Microsoft.Office.Interop.Excel.Range tRange = dataSheet.Cells.get_Range("A1", "C" + (currentRow-1));

            ListObject tbl1 = dataSheet.ListObjects["Table1"];
            tbl1.Resize(tRange);

            wdChart.ApplyDataLabels(XlDataLabelsType.xlDataLabelsShowNone, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
            wdChart.ChartType = XlChartType.xlLine;
            Microsoft.Office.Interop.Word.Axis xAxis = (Microsoft.Office.Interop.Word.Axis)wdChart.Axes(Microsoft.Office.Interop.Word.XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
            xAxis.BaseUnit = Microsoft.Office.Interop.Word.XlTimeUnit.xlMonths;
            
            
            wdChart.Refresh();

             if (Executer.Destination.TableLoadInfo.Inserts == 0)
                return;
            
            float futureRecordsCountAsFraction = ((float)futureRecordsCount) / ((float)Executer.Destination.TableLoadInfo.Inserts) * 100.0f;
            float nullCountAsFraction = ((float)toGraph.countOfNullsSeen) / ((float)Executer.Destination.TableLoadInfo.Inserts) * 100.0f;
            float brokenDatesAsFraction = ((float)toGraph.countOfBrokenDates) / ((float)Executer.Destination.TableLoadInfo.Inserts) * 100.0f;
            float recordsPriorToDatasetStartCountAsFraction = ((float)recordsPriorToDatasetStartCount) / ((float)Executer.Destination.TableLoadInfo.Inserts) * 100.0f; ;
            
            wordHelper.WriteLine("Nulls:" + toGraph.countOfNullsSeen + "(" + nullCountAsFraction + "%)", WdBuiltinStyle.wdStyleNormal);
            wordHelper.WriteLine("Invalid Date formats:" + toGraph.countOfBrokenDates + "(" + brokenDatesAsFraction + "%)", WdBuiltinStyle.wdStyleNormal);
            wordHelper.WriteLine("Future Dates:" + futureRecordsCount +"("+ futureRecordsCountAsFraction + "%)",WdBuiltinStyle.wdStyleNormal);
            wordHelper.WriteLine("Timespan Field: " + cata.TimeCoverage_ExtractionInformation.GetRuntimeName(), WdBuiltinStyle.wdStyleNormal);

            if (cata.DatasetStartDate != null)
                wordHelper.WriteLine("Dates before dataset start date(" + cata.DatasetStartDate.Value.ToString(_destination.DateFormat) + "):" + recordsPriorToDatasetStartCount + "(" + recordsPriorToDatasetStartCountAsFraction + "%)", WdBuiltinStyle.wdStyleNormal);
           
            dataWorkbook.Application.Quit();
        
        }

        private void CreateValidationRulesTable()
        {
            object start = wrdApp.Selection.End;
            object end = wrdApp.Selection.End;



            Validator validator = Executer.Source.ExtractionTimeValidator.Validator;

            int rowCount = validator.ItemValidators.Count +
                           Executer.Source.ExtractionTimeValidator.IgnoredBecauseColumnHashed.Count + 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);
            Table table = wrdDoc.Tables.Add(tableLocation, rowCount, 2);
            table.set_Style(tableStyle);

            int tableLine = 1;
            //output table header
            table.Cell(tableLine, 1).Range.Text = "Column";
            table.Cell(tableLine, 2).Range.Text = "Validation";
            tableLine++;

            //output list of validations we carried out
            foreach (ItemValidator iv in validator.ItemValidators)
            {
                table.Cell(tableLine, 1).Range.Text = iv.TargetProperty;

                string definition = "";

                if (iv.PrimaryConstraint != null)
                {
                    definition += "Primary Constraint:" + Environment.NewLine;
                    definition += iv.PrimaryConstraint.GetHumanReadableDescriptionOfValidation() +
                                  " (Validation rule failure has Consequence that cell data is:" +
                                  iv.PrimaryConstraint.Consequence + ")" + Environment.NewLine;
                }

                if (iv.SecondaryConstraints.Any())
                {
                    definition += "Secondary Constraint(s):" + Environment.NewLine;
                    definition = iv.SecondaryConstraints.Aggregate(definition,
                                                                   (def, e) =>
                                                                   def + e.GetHumanReadableDescriptionOfValidation() +
                                                                   " (Validation rule failure has Consequence that cell data is:" +
                                                                   e.Consequence + ")" + Environment.NewLine);
                }

                if (string.IsNullOrWhiteSpace(definition))
                    definition = "No validation configured";

                table.Cell(tableLine, 2).Range.Text = definition;
                tableLine++;
            }

            //ouput list of validations we skipped
            foreach (ItemValidator iv in Executer.Source.ExtractionTimeValidator.IgnoredBecauseColumnHashed)
            {
                table.Cell(tableLine, 1).Range.Text = iv.TargetProperty;
                table.Cell(tableLine, 2).Range.Text = "No validations carried out because column is Hashed/Annonymised";
                tableLine++;
            }
        }

        private void CreateValidationResultsTable()
        {
            object start = wrdApp.Selection.End;
            object end = wrdApp.Selection.End;

            VerboseValidationResults results = Executer.Source.ExtractionTimeValidator.Results;

            Range tableLocation = wrdDoc.Range(ref start, ref end);
            Table table = wrdDoc.Tables.Add(tableLocation, results.DictionaryOfFailure.Count+1,5);
            table.set_Style(tableStyle);

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "";
            table.Cell(tableLine, 2).Range.Text = Consequence.Missing.ToString();
            table.Cell(tableLine, 4).Range.Text = Consequence.Wrong.ToString();
            table.Cell(tableLine, 5).Range.Text = Consequence.InvalidatesRow.ToString();

            tableLine++;

            foreach (KeyValuePair<string, Dictionary<Consequence, int>> keyValuePair in results.DictionaryOfFailure)
            {
                string colname = keyValuePair.Key;

                table.Cell(tableLine, 1).Range.Text = colname;
                table.Cell(tableLine, 2).Range.Text = keyValuePair.Value[Consequence.Missing].ToString();
                table.Cell(tableLine, 4).Range.Text = keyValuePair.Value[Consequence.Wrong].ToString();
                table.Cell(tableLine, 5).Range.Text = keyValuePair.Value[Consequence.InvalidatesRow].ToString();
                tableLine++;
            }
        }
    }
}
