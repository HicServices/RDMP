using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using DataTable = System.Data.DataTable;

namespace CatalogueLibrary.Reports
{
    public delegate Image[] RequestCatalogueImagesHandler(Catalogue catalogue);

    public class MetadataReport
    {
        public IDetermineDatasetTimespan TimespanCalculator { get; set; }
        private readonly CatalogueRepository _repository;
        private readonly int _timeout;
        private readonly bool _includeRowCounts;
        private readonly bool _includeDistinctRowCounts;
        private readonly bool _skipImages;
        private readonly Catalogue[] _catalogues;

        HashSet<TableInfo> LookupsEncounteredToAppearInAppendix = new HashSet<TableInfo>();

        public float PageWidthInPixels { get; private set; }
        public int MaxLookupRows { get; set; }

        #region stuff for Word
        object oTrue = true;
        object oFalse = false;
        Object oMissing = System.Reflection.Missing.Value;

        Microsoft.Office.Interop.Word.Application wrdApp;
        Microsoft.Office.Interop.Word._Document wrdDoc;
        #endregion
        
        /// <summary>
        /// Set this to true if you want microsoft word to be visible while it is running Interop commands (will be very confusing for users so never ship this with true)
        /// </summary>
        public bool DEBUG_WORD = false;

        public event CatalogueProgressHandler CatalogueCompleted;
        public event RequestCatalogueImagesHandler RequestCatalogueImages;
        
        private const int TextFontSize = 7;


        public MetadataReport(CatalogueRepository repository,IEnumerable<Catalogue> catalogues, int timeout, bool includeRowCounts, bool includeDistinctRowCounts, bool skipImages,IDetermineDatasetTimespan timespanCalculator)
        {
            TimespanCalculator = timespanCalculator;
            _repository = repository;
            _timeout = timeout;
            _includeRowCounts = includeRowCounts;
            _includeDistinctRowCounts = includeDistinctRowCounts;
            _skipImages = skipImages;
            _catalogues = catalogues.ToArray();
        }

        Thread thread;

        public void GenerateWordFileAsync(ICheckNotifier warningsAndErrorsHandler)
        {

            thread = new Thread(() => GenerateWordFile(warningsAndErrorsHandler));
            thread.Start();
        }

        private void GenerateWordFile(ICheckNotifier warningsAndErrorsHandler)
        {
            try
            {

                int version = OfficeVersionFinder.GetMajorVersion(OfficeVersionFinder.OfficeComponent.Word);

                if (version == 0)
                    warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Microsoft Word not found, is it installed?",
                        CheckResult.Fail, null));
                else
                    warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Found Microsoft Word " + version + " installed",
                        CheckResult.Success, null));

                // Create an instance of Word  and make it visible.=
                wrdApp = new Application();

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
                
                
                wrdApp.Options.EnableMisusedWordsDictionary = false;
                wrdApp.Options.CheckSpellingAsYouType = false;
                wrdApp.Options.CheckGrammarWithSpelling = false;
                wrdApp.Options.CheckGrammarAsYouType = false;
                wrdApp.Options.ContextualSpeller = false;
                wrdApp.Options.SuggestSpellingCorrections = false;

                
                const int marginSize = 20;
                try
                {
                    wrdDoc.PageSetup.LeftMargin = marginSize;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("The message filter indicated that the application is busy"))
                        warningsAndErrorsHandler.OnCheckPerformed(
                            new CheckEventArgs(
                                "Word is trying to display a dialog (probably asking you about file associations or somethihg), you must manually launch Microsoft Word - resolve any popup dialogs and tick any boxes marked 'never warn me about this again'.  Once Word launches properly (without throwing up dialog boxes) this report will work",
                                CheckResult.Fail));
                    else
                        throw;
                }
                wrdDoc.PageSetup.RightMargin = marginSize;
                wrdDoc.PageSetup.TopMargin = marginSize;
                wrdDoc.PageSetup.BottomMargin = marginSize;
            
                PageWidthInPixels = wrdApp.PointsToPixels(wrdDoc.PageSetup.PageWidth, ref oFalse);

                try
                {
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
                
                    WordHelper wordHelper = new WordHelper(wrdApp);

                    int completed = 0;

                    
                    foreach (Catalogue c in _catalogues)
                    {
                        CatalogueCompleted(completed++, _catalogues.Length, c);

                        int recordCount = -1;
                        int distinctRecordCount = -1;
                        string identifierName = null;

                        bool gotRecordCount = false;
                        try
                        {
                            if (_includeRowCounts)
                            {
                                GetRecordCount(c, out recordCount, out distinctRecordCount, out identifierName);
                                gotRecordCount = true;
                            }
                        }
                        catch (Exception e)
                        {
                            warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Error processing record count for Catalogue " + c.Name,
                                CheckResult.Fail, e));
                        }
                        
                        wordHelper.WriteLine(c.Name, WdBuiltinStyle.wdStyleHeading1);

                        if (TimespanCalculator != null)
                        {

                            string timespan = TimespanCalculator.GetHumanReadableTimepsanIfKnownOf(c, true);
                            if(!string.IsNullOrWhiteSpace(timespan))
                                wordHelper.WriteLine(timespan, TextFontSize);
                        }
                        
                        wordHelper.WriteLine(c.Description,TextFontSize);

                        if (gotRecordCount)
                        {
                            wordHelper.WriteLine("Record Count", WdBuiltinStyle.wdStyleHeading4);
                            CreateCountTable(recordCount, distinctRecordCount, identifierName);
                        }

                        wordHelper.GoToEndOfDocument();

                        if(!_skipImages)
                        {
                            Image[] onRequestCatalogueImages = RequestCatalogueImages(c);

                            if (onRequestCatalogueImages.Any())
                            {
                                wordHelper.WriteLine("Aggregates", WdBuiltinStyle.wdStyleHeading4);
                                AddImages(onRequestCatalogueImages, wordHelper);
                            }
                            
                        }
                    
                        wordHelper.WriteLine("Columns", WdBuiltinStyle.wdStyleHeading4);
                        CreateDescriptionsTable(c);
                        
                        wrdDoc.Content.NoProofing = 1;
                        wordHelper.GoToEndOfDocument();
                    
                        //if this is not the last Catalogue create a new page
                        if(completed != _catalogues.Length)
                        {
                            wordHelper.StartNewPageInDocument();
                            wordHelper.GoToEndOfDocument();
                        }

                        CatalogueCompleted(completed, _catalogues.Length, c);

                        
                    }

                    if (LookupsEncounteredToAppearInAppendix.Any())
                        CreateLookupAppendix(wordHelper, warningsAndErrorsHandler);

                }
                catch (ThreadInterruptedException)
                {
                    //user hit abort   
                }
                finally
                {
                    wrdApp.Visible = true;
                }

            }
            catch (Exception e)
            {
                warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Entire process failed, see Exception for details",CheckResult.Fail, e));
            }
        }

        private void CreateLookupAppendix(WordHelper wordHelper, ICheckNotifier warningsAndErrorsHandler)
        {
            wordHelper.GoToEndOfDocument();
            wordHelper.StartNewPageInDocument();
            wordHelper.WriteLine("Appendix 1 - Lookup Tables", WdBuiltinStyle.wdStyleTitle);
            
            //foreach lookup
            foreach (TableInfo lookupTable in LookupsEncounteredToAppearInAppendix)
            {
                DataTable dt = null;

                try    
                {
                   dt = GetLookupTableInfoContentsFromDatabase(lookupTable);
                }
                catch (Exception e)
                {
                    warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Failed to get the contents of loookup " + lookupTable.Name, CheckResult.Fail, e));
                }
                
                if(dt == null)
                    continue;

                //if it has too many columns
                if (dt.Columns.Count > 5)
                {
                    warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs("Lookup table " + lookupTable.Name + " has more than 5 columns so will not be processed", CheckResult.Warning, null));
                    continue;
                }

                //write name of lookup
                wordHelper.WriteLine(lookupTable.Name, WdBuiltinStyle.wdStyleHeading1);

                object start = wrdDoc.Content.End - 1;
                object end = wrdDoc.Content.End - 1;

                Range tableLocation = wrdDoc.Range(ref start, ref end);

                Table table = wrdDoc.Tables.Add(tableLocation, Math.Min(dt.Rows.Count + 1, MaxLookupRows+2), dt.Columns.Count);

                table.set_Style("Table Grid");
                table.Range.Font.Size = TextFontSize;

                int tableLine = 1;

                //write the headers to the table
                for (int i = 0; i < dt.Columns.Count; i++)
                    table.Cell(tableLine, i + 1).Range.Text = dt.Columns[i].ColumnName;

                //move to next line
                tableLine++;

                int maxLineCountDowner = MaxLookupRows+2;//2, 1 for the headers and 1 for the ... row


                //see if it has any lookups
                foreach (DataRow row in dt.Rows)
                { 
                    for (int i = 0; i < dt.Columns.Count; i++)
                        table
                            .Cell(tableLine, i + 1).Range.Text = Convert.ToString(row[i]);

                    //move to next line
                    tableLine++;
                    maxLineCountDowner--;

                    if (maxLineCountDowner == 0)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                            table.Cell(tableLine, i + 1).Range.Text = "...";
                        break;
                    }
                }

                table.AllowAutoFit = true;
                table.Columns.AutoFit();
                warningsAndErrorsHandler.OnCheckPerformed(new CheckEventArgs(
                    "Wrote out lookup table " + lookupTable.Name + " successfully", CheckResult.Success, null));

                wordHelper.GoToEndOfDocument();
             
            }
        }

        private DataTable GetLookupTableInfoContentsFromDatabase(TableInfo lookupTable)
        {
            //get the contents of the lookup
            DbConnection con = DataAccessPortal.GetInstance().ExpectServer(lookupTable,DataAccessContext.InternalDataProcessing).GetConnection();
            con.Open();
            try
            {
                var cmd = DatabaseCommandHelper.GetCommand("Select * from " + lookupTable.Name, con);
                var da = DatabaseCommandHelper.GetDataAdapter(cmd);
                
                var dt = new System.Data.DataTable();
                da.Fill(dt);

                return dt;
            }
            finally
            {
                con.Close();
            }

        }

        private void AddImages(Image[] onRequestCatalogueImages, WordHelper wordHelper)
        {
            foreach (Image image in onRequestCatalogueImages)
                wordHelper.WriteImage(image, wrdDoc);
        }

        private void CreateDescriptionsTable(Catalogue c)
        {
            var extractionInformations = c.GetAllExtractionInformation(ExtractionCategory.Any).ToList();
            extractionInformations.Sort();

            object start = wrdDoc.Content.End - 1;
            object end = wrdDoc.Content.End - 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);

            Table table = wrdDoc.Tables.Add(tableLocation, extractionInformations.Count + 1, 3);

            table.set_Style("Table Grid");
            table.Range.Font.Size = TextFontSize;
            table.AllowAutoFit = true;

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "Column";
            table.Cell(tableLine, 2).Range.Text = "Datatype";
            table.Cell(tableLine, 3).Range.Text = "Description";

            tableLine++;


            foreach (ExtractionInformation information in extractionInformations)
            {
                table.Cell(tableLine, 1).Range.Text = information.GetRuntimeName();
                table.Cell(tableLine, 2).Range.Text = information.ColumnInfo.Data_type;
                string description = information.CatalogueItem.Description;

                
                //a field should only ever be a foreign key to one Lookup table
                var lookups = information.ColumnInfo.GetAllLookupForColumnInfoWhereItIsA(LookupType.ForeignKey);

                //if it has any lookups
                if (lookups.Any())
                {
                    var pkTableId = lookups.Select(l => l.PrimaryKey.TableInfo_ID).Distinct().SingleOrDefault();

                    var lookupTable = _repository.GetObjectByID<TableInfo>(pkTableId);

                    if (!LookupsEncounteredToAppearInAppendix.Contains(lookupTable))
                        LookupsEncounteredToAppearInAppendix.Add(lookupTable);

                    description += "References Lookup Table " + lookupTable.GetRuntimeName();

                }

                table.Cell(tableLine, 3).Range.Text = description;

                tableLine++;
            }
            
            table.Columns.AutoFit();
        }

        private void CreateCountTable(int recordCount,int distinctCount, string identifierName)
        {

            object start = wrdDoc.Content.End - 1;
            object end = wrdDoc.Content.End - 1;

            Range tableLocation = wrdDoc.Range(ref start, ref end);

            Table table = wrdDoc.Tables.Add(tableLocation, 2, identifierName != null && _includeDistinctRowCounts? 2:1);

            table.set_Style("Table Grid");
            table.Range.Font.Size = TextFontSize;
            table.AllowAutoFit = true;

            int tableLine = 1;

            table.Cell(tableLine, 1).Range.Text = "Records";

            //only add column values if there is an IsExtractionIdentifier returned 
            if (identifierName != null && _includeDistinctRowCounts)
                table.Cell(tableLine, 2).Range.Text = "Distinct " + identifierName;

            tableLine++;

            
            table.Cell(tableLine, 1).Range.Text = recordCount.ToString("N0");

            //only add column values if there is an IsExtractionIdentifier returned 
            if (identifierName != null &&  _includeDistinctRowCounts)
                table.Cell(tableLine, 2).Range.Text = distinctCount.ToString("N0");
        }

        private void GetRecordCount(Catalogue c, out int count, out int distinct, out string identifierName)
        {
            //one of the fields will be marked IsExtractionIdentifier (e.g. CHI column)
            ExtractionInformation[] bestExtractionInformation = c.GetAllExtractionInformation(ExtractionCategory.Any).Where(e => e.IsExtractionIdentifier).ToArray();

            TableInfo tableToQuery = null;

            //there is no extraction identifier or we are not doing distincts
            if (!bestExtractionInformation.Any())
            {
                //there is no extraction identifier, let's see what tables there are that we can query
                var tableInfos = 
                    c.GetAllExtractionInformation(ExtractionCategory.Any)
                    .Select(ei => ei.ColumnInfo.TableInfo_ID)
                    .Distinct()
                    .Select(_repository.GetObjectByID<TableInfo>)
                    .ToArray();
                
                //there is only one table that we can query
                if (tableInfos.Count() == 1)
                    tableToQuery = tableInfos.Single();//query that one
                else
                if (tableInfos.Count(t => t.IsPrimaryExtractionTable) == 1)//there are multiple tables but there is only one IsPrimaryExtractionTable
                    tableToQuery = tableInfos.Single(t => t.IsPrimaryExtractionTable);
                else
                    throw new Exception("Did not know which table to query out of " + string.Join(",", tableInfos.Select(t => t.GetRuntimeName())) + " you can resolve this by marking one (AND ONLY ONE) of these tables as IsPrimaryExtractionTable=true");//there are multiple tables and multiple or no IsPrimaryExtractionTable

            }
            else
                tableToQuery = bestExtractionInformation[0].ColumnInfo.TableInfo;//there is an extraction identifier so use it's table to query

            bool hasExtractionIdentifier = bestExtractionInformation.Any();

            var server = c.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, true);
            using (var con = server.GetConnection())
            {

                con.Open();

                if (tableToQuery.Name.Contains("@"))
                    throw new Exception("Table '" + tableToQuery.Name + "' looks like a table valued function so cannot be processed");

                string sql = "SELECT " + Environment.NewLine;
                sql += "count(*) as recordCount";

                //if it has extraction information and we want a distinct count
                if (hasExtractionIdentifier && _includeDistinctRowCounts)
                    sql += ",\r\ncount(distinct " + bestExtractionInformation[0].SelectSQL + ") as recordCountDistinct" + Environment.NewLine;
            
                sql += " from " + Environment.NewLine;
                sql += tableToQuery.Name;

                identifierName = hasExtractionIdentifier ? bestExtractionInformation[0].GetRuntimeName() : null;
            
                DbCommand cmd = server.GetCommand(sql,con);
                cmd.CommandTimeout = _timeout;

                DbDataReader r = cmd.ExecuteReader();
                r.Read();

                count = Convert.ToInt32(r["recordCount"]);
                distinct = hasExtractionIdentifier && _includeDistinctRowCounts ? Convert.ToInt32(r["recordCountDistinct"]) : -1;

                con.Close();
            }
        }

        public void Abort()
        {
            if(thread != null)
                thread.Interrupt();
        }
    }
}
