// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using ReusableLibraryCode;
using Xceed.Words.NET;
using IFilter = CatalogueLibrary.Data.IFilter;

namespace DataExportLibrary.ExtractionTime
{
    /// <summary>
    /// Generates a Microsoft Word docx file containing information about a researchers extract including the file generated, the number of rows, distinct patients, the
    /// filters used in the extraction query, any parameters etc.  Optionally includes a validation table which counts the number of rows extracted that passed ValidationXML 
    /// </summary>
    public class WordDataWriter : RequiresMicrosoftOffice
    {
        public ExtractionPipelineUseCase Executer { get; set; }

        public List<Exception> ExceptionsGeneratingWordFile = new List<Exception>();

        public WordDataWriter(ExtractionPipelineUseCase executer)
        {
            if(executer == null)
                throw new NullReferenceException("Cannot write meta data without the accompanying ExtractionPipelineHost");

            if (executer.Source.WasCancelled)
                throw new NullReferenceException("Cannot write meta data since ExtractionPipelineHost reports that it was Cancelled");
            
            Executer = executer;

            _destination = Executer.Destination;

            if(_destination == null)
                throw new NotSupportedException(GetType().FullName + " only supports destinations which are " + typeof(ExecuteDatasetExtractionFlatFileDestination).FullName);
        }
        
        private static object oLockOnWordUsage = new object();
        private IExecuteDatasetExtractionDestination _destination;

        /// <summary>
        /// Generates a new meta data word file in the extraction directory and populates it with information about the extraction.
        /// It returns the open document as an object so that you can supplement it e.g. with catalogue information
        /// </summary>
        /// <returns></returns>
        public void GenerateWordFile()
        {
            lock (oLockOnWordUsage)
            {
                string outputFilename = Path.Combine(_destination.DirectoryPopulated.FullName, _destination.GetFilename() + ".docx");

                using (DocX document = DocX.Create(outputFilename))
                {
                    InsertHeader(document,Executer.Source.Request.DatasetBundle.DataSet + " Meta Data");

                    document.InsertTableOfContents("Contents", new TableOfContentsSwitches());

                    InsertHeader(document,"File Data");

                    int rowCount;
                    if (_destination.GeneratesFiles)
                        rowCount = 10;
                    else
                        rowCount = 5;

                    var t = InsertTable(document, rowCount, 2, TableDesign.TableGrid);
                    t.Design = TableDesign.LightList;
                    int rownum = 0;
                    if (_destination.GeneratesFiles)
                    {
                        t.Rows[rownum].Cells[0].Paragraphs.First().Append("File Name");
                        t.Rows[rownum].Cells[1].Paragraphs.First().Append(new FileInfo(_destination.OutputFile).Name);
                        rownum++;
                    }

                    t.Rows[rownum].Cells[0].Paragraphs.First().Append("Cohort Size (Distinct)");
                    t.Rows[rownum].Cells[1].Paragraphs.First().Append(Executer.Source.Request.ExtractableCohort.CountDistinct.ToString());
                    rownum++;

                    t.Rows[rownum].Cells[0].Paragraphs.First().Append("Cohorts Found In Dataset");
                    t.Rows[rownum].Cells[1].Paragraphs.First().Append(Executer.Source.UniqueReleaseIdentifiersEncountered.Count.ToString());
                    rownum++;

                    t.Rows[rownum].Cells[0].Paragraphs.First().Append("Dataset Line Count");
                    t.Rows[rownum].Cells[1].Paragraphs.First().Append(Executer.Destination.TableLoadInfo.Inserts.ToString());
                    rownum++;

                    if (_destination.GeneratesFiles)
                    {
                        t.Rows[rownum].Cells[0].Paragraphs.First().Append("MD5");
                        t.Rows[rownum].Cells[1].Paragraphs.First().Append(UsefulStuff.MD5File(_destination.OutputFile));
                        rownum++;
                    
                        FileInfo f = new FileInfo(_destination.OutputFile);
                        t.Rows[rownum].Cells[0].Paragraphs.First().Append("File Size");
                        t.Rows[rownum].Cells[1].Paragraphs.First().Append(f.Length + "bytes (" + (f.Length / 1024) + "KB)");
                        rownum++;
                    }

                    t.Rows[rownum].Cells[0].Paragraphs.First().Append("Extraction Date");
                    t.Rows[rownum].Cells[1].Paragraphs.First().Append(Executer.Destination.TableLoadInfo.EndTime.ToString());
                    rownum++;

                    t.Rows[rownum].Cells[0].Paragraphs.First().Append("Table Load ID (for HIC)");
                    t.Rows[rownum].Cells[1].Paragraphs.First().Append(Executer.Destination.TableLoadInfo.ID.ToString());
                    rownum++;

                    if (_destination.GeneratesFiles)
                    {
                        t.Rows[rownum].Cells[0].Paragraphs.First().Append("Separator");
                        t.Rows[rownum].Cells[1].Paragraphs.First()
                            .Append(Executer.Source.Request.Configuration.Separator + "\t(" +
                                    _destination.SeparatorsStrippedOut + " values stripped from data)");
                        rownum++;

                        t.Rows[rownum].Cells[0].Paragraphs.First().Append("Date Format");
                        t.Rows[rownum].Cells[1].Paragraphs.First().Append(_destination.DateFormat);
                        rownum++;
                    }

                    if (Executer.Source.ExtractionTimeValidator != null && Executer.Source.Request.IncludeValidation)
                    {
                        document.InsertSectionPageBreak();

                        InsertHeader(document,"Validation Information");

                        CreateValidationRulesTable(document);

                        document.InsertSectionPageBreak();

                        CreateValidationResultsTable(document);
                    }

                    //if a count of date times seen exists for this extraction create a graph of the counts seen
                    if (Executer.Source.ExtractionTimeTimeCoverageAggregator != null && Executer.Source.ExtractionTimeTimeCoverageAggregator.Buckets.Any())
                    {
                        try
                        {
                            document.InsertSectionPageBreak();
                            InsertHeader(document, "Dataset Timespan");
                            
                            CreateTimespanGraph(Executer.Source.ExtractionTimeTimeCoverageAggregator);

                        }
                        catch (Exception e)
                        {
                            ExceptionsGeneratingWordFile.Add(e);
                        }

                    }

                    document.InsertSectionPageBreak();

                    AddAllCatalogueItemMetaData(document);

                    //technical data
                    document.InsertSectionPageBreak();

                    AddFiltersAndParameters(document);

                    AddIssuesForCatalogue(document);
                    
                    document.Save();
                }
            }
        }

        private void AddIssuesForCatalogue(DocX document)
        {
            var cata = Executer.Source.Request.Catalogue;
            WordCatalogueExtractor catalogueMetaData = new WordCatalogueExtractor(cata, document);

            catalogueMetaData.AddIssuesForCatalogue();
        }

        private void AddFiltersAndParameters(DocX document)
        {
            var request = Executer.Source.Request;
            FilterContainer fc = (FilterContainer)request.Configuration.GetFilterContainerFor(request.DatasetBundle.DataSet);

            if (fc != null)
            {
                List<IFilter> filtersUsed;
                filtersUsed = fc.GetAllFiltersIncludingInSubContainersRecursively();

                WriteOutFilters(document,filtersUsed);

                WriteOutParameters(document,filtersUsed);
            }
        }

        private void WriteOutParameters(DocX document, List<IFilter> filtersUsed)
        {
            InsertHeader(document,"Parameters");

            int linesRequred = filtersUsed.Aggregate(0, (s, f) => s + f.GetAllParameters().Count());

            var globalParameters = Executer.Source.Request.Configuration.GlobalExtractionFilterParameters.ToArray();
            linesRequred += globalParameters.Length;

            Table t = InsertTable(document,linesRequred + 1, 3);
            SetTableCell(t,0,0,"Name");
            SetTableCell(t, 0, 1, "Comment");
            SetTableCell(t, 0, 2, "Value");

            int currentLine = 1;

            foreach (IFilter filter in filtersUsed)
                foreach (ISqlParameter parameter in filter.GetAllParameters())
                {
                    SetTableCell(t,currentLine, 0, parameter.ParameterName);
                    SetTableCell(t,currentLine, 1, parameter.Comment);
                    SetTableCell(t,currentLine, 2, parameter.Value);
                    currentLine++;
                }

            foreach (ISqlParameter globalParameter in globalParameters)
            {
                SetTableCell(t,currentLine, 0, globalParameter.ParameterName);
                SetTableCell(t,currentLine, 1, globalParameter.Comment);
                SetTableCell(t,currentLine, 2, globalParameter.Value);
                currentLine++;
            }
        }

        private void  WriteOutFilters(DocX document, List<IFilter> filtersUsed)
        {
            InsertHeader(document,"Filters");

            Table t = InsertTable(document,filtersUsed.Count + 1, 3);

            SetTableCell(t,0, 0, "Name");
            SetTableCell(t,0, 1, "Description");
            SetTableCell(t,0, 2, "SQL");

            for (int i = 0; i < filtersUsed.Count; i++)
            {
                //i+2 becauset, first row is for headers and indexing in word starts at 1 not 0
                SetTableCell(t,i + 1, 0, filtersUsed[i].Name);
                SetTableCell(t,i + 1, 1, filtersUsed[i].Description);
                SetTableCell(t,i + 1, 2, filtersUsed[i].WhereSQL);
            }
        }

        private void AddAllCatalogueItemMetaData(DocX document)
        {
            var cata = Executer.Source.Request.Catalogue;
            WordCatalogueExtractor catalogueMetaData = new WordCatalogueExtractor(cata,document);
            
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

            catalogueMetaData.AddMetaDataForColumns(supplementalData.Keys.ToArray(),supplementalData);
        }

        private void CreateTimespanGraph(ExtractionTimeTimeCoverageAggregator toGraph)
        {/*
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
        */
        }

        private void CreateValidationRulesTable(DocX document)
        {
            Validator validator = Executer.Source.ExtractionTimeValidator.Validator;

            int rowCount = validator.ItemValidators.Count +
                           Executer.Source.ExtractionTimeValidator.IgnoredBecauseColumnHashed.Count + 1;

            Table t = InsertTable(document,rowCount, 2);

            int tableLine = 0;
            //output table header
            SetTableCell(t,tableLine, 0,"Column");
            SetTableCell(t,tableLine, 1, "Validation");
            tableLine++;

            //output list of validations we carried out
            foreach (ItemValidator iv in validator.ItemValidators)
            {
                SetTableCell(t,tableLine, 0, iv.TargetProperty);

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

                SetTableCell(t, tableLine,1, definition);
                tableLine++;
            }

            //ouput list of validations we skipped
            foreach (ItemValidator iv in Executer.Source.ExtractionTimeValidator.IgnoredBecauseColumnHashed)
            {
                SetTableCell(t, tableLine, 0 , iv.TargetProperty);
                SetTableCell(t, tableLine, 1, "No validations carried out because column is Hashed/Annonymised");
                tableLine++;
            }
        }

        private void CreateValidationResultsTable(DocX document)
        {
            VerboseValidationResults results = Executer.Source.ExtractionTimeValidator.Results;

            Table t = InsertTable(document,results.DictionaryOfFailure.Count + 1, 4);
            
            int tableLine = 0;
            
            SetTableCell(t,tableLine, 0, "");
            SetTableCell(t,tableLine, 1, Consequence.Missing.ToString());
            SetTableCell(t,tableLine, 2, Consequence.Wrong.ToString());
            SetTableCell(t,tableLine, 3, Consequence.InvalidatesRow.ToString());

            tableLine++;

            foreach (KeyValuePair<string, Dictionary<Consequence, int>> keyValuePair in results.DictionaryOfFailure)
            {
                string colname = keyValuePair.Key;

                SetTableCell(t,tableLine, 0,colname);
                SetTableCell(t,tableLine, 1,keyValuePair.Value[Consequence.Missing].ToString());
                SetTableCell(t,tableLine, 2,keyValuePair.Value[Consequence.Wrong].ToString());
                SetTableCell(t,tableLine, 3,keyValuePair.Value[Consequence.InvalidatesRow].ToString());
                tableLine++;
            }
        }
    }
}
