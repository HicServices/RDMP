using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Xceed.Words.NET;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Describes a method that generates images for a <seealso cref="Catalogue"/> e.g. aggregate graphs
    /// </summary>
    /// <param name="catalogue"></param>
    /// <returns></returns>
    public delegate BitmapWithDescription[] RequestCatalogueImagesHandler(Catalogue catalogue);

    /// <summary>
    /// Generates a high level summary Microsoft Word DocX file of one or more Catalogues.  This includes the rowcount, distinct patient count, description and descriptions
    /// of extractable columns as well as an Appendix of Lookups.  In addition any IsExtractable AggregateConfiguration graphs will be run and screen captured and added to 
    /// the report (including heatmap if a dynamic pivot is included in the graph).
    /// </summary>
    public class MetadataReport:RequiresMicrosoftOffice
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

        public void GenerateWordFileAsync(IDataLoadEventListener listener)
        {

            thread = new Thread(() => GenerateWordFile(listener));
            thread.Start();
        }

        private void GenerateWordFile(IDataLoadEventListener listener)
        {
            try
            {

                int version = OfficeVersionFinder.GetMajorVersion(OfficeVersionFinder.OfficeComponent.Word);

                if (version == 0)
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Microsoft Word not found, is it installed?"));
                else
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Found Microsoft Word " + version + " installed"));

                var f = GetUniqueFilenameInWorkArea("MetadataReport");

                using (DocX document = DocX.Create(f.FullName))
                {
                    const int marginSize = 20;
                    try
                    {
                        document.MarginLeft = marginSize;
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("The message filter indicated that the application is busy"))
                            listener.OnNotify(this,
                                new NotifyEventArgs(
                                    ProgressEventType.Error, 
                                    "Word is trying to display a dialog (probably asking you about file associations or somethihg), you must manually launch Microsoft Word - resolve any popup dialogs and tick any boxes marked 'never warn me about this again'.  Once Word launches properly (without throwing up dialog boxes) this report will work",
                                    e));
                        else
                            throw;
                    }
                    document.MarginRight= marginSize;
                    document.MarginTop = marginSize;
                    document.MarginBottom = marginSize;

                    PageWidthInPixels = document.PageWidth;
                    
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        int completed = 0;


                        foreach (Catalogue c in _catalogues)
                        {
                            listener.OnProgress(this, new ProgressEventArgs("Extracting", new ProgressMeasurement(completed++, ProgressType.Records, _catalogues.Length), sw.Elapsed));

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
                                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Error processing record count for Catalogue " + c.Name,e));
                            }

                            InsertHeader(document,c.Name);

                            if (TimespanCalculator != null)
                            {
                                string timespan = TimespanCalculator.GetHumanReadableTimepsanIfKnownOf(c, true);
                                if (!string.IsNullOrWhiteSpace(timespan))
                                    InsertParagraph(document,timespan, TextFontSize);
                            }

                            InsertParagraph(document,c.Description, TextFontSize);

                            if (gotRecordCount)
                            {
                                InsertHeader(document,"Record Count", 3);
                                CreateCountTable(document,recordCount, distinctRecordCount, identifierName);
                            }
                            
                            if (!_skipImages)
                            {
                                BitmapWithDescription[] onRequestCatalogueImages = RequestCatalogueImages(c);

                                if (onRequestCatalogueImages.Any())
                                {
                                    InsertHeader(document,"Aggregates",2);
                                    AddImages(document,onRequestCatalogueImages);
                                }

                            }

                            InsertHeader(document,"Columns",2);
                            CreateDescriptionsTable(document,c);

                            //if this is not the last Catalogue create a new page
                            if (completed != _catalogues.Length)
                                document.InsertSectionPageBreak();

                            listener.OnProgress(this, new ProgressEventArgs("Extracting", new ProgressMeasurement(completed, ProgressType.Records, _catalogues.Length), sw.Elapsed));
                        }

                        if (LookupsEncounteredToAppearInAppendix.Any())
                            CreateLookupAppendix(document, listener);

                        document.Save();
                        ShowFile(f);
                    }
                    catch (ThreadInterruptedException)
                    {
                        //user hit abort   
                    }

                }
                
            }
            catch (Exception e)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Entire process failed, see Exception for details", e));
            }
        }

        private void CreateLookupAppendix(DocX document, IDataLoadEventListener listener)
        {
            document.InsertSectionPageBreak();
            InsertHeader(document,"Appendix 1 - Lookup Tables");
            
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
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Failed to get the contents of loookup " + lookupTable.Name, e));
                }
                
                if(dt == null)
                    continue;

                //if it has too many columns
                if (dt.Columns.Count > 5)
                {
                    listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Lookup table " + lookupTable.Name + " has more than 5 columns so will not be processed"));
                    continue;
                }

                //write name of lookup
                InsertHeader(document,lookupTable.Name);

                var table = InsertTable(document,Math.Min(dt.Rows.Count + 1, MaxLookupRows + 2), dt.Columns.Count);

                int tableLine = 0;

                //write the headers to the table
                for (int i = 0; i < dt.Columns.Count; i++)
                    SetTableCell(table, tableLine, i, dt.Columns[i].ColumnName, TextFontSize);

                //move to next line
                tableLine++;

                int maxLineCountDowner = MaxLookupRows+2;//2, 1 for the headers and 1 for the ... row
                
                //see if it has any lookups
                foreach (DataRow row in dt.Rows)
                { 
                    for (int i = 0; i < dt.Columns.Count; i++)
                        SetTableCell(table,tableLine, i, Convert.ToString(row[i]));

                    //move to next line
                    tableLine++;
                    maxLineCountDowner--;

                    if (maxLineCountDowner == 0)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                            SetTableCell(table,tableLine, i, "...");
                        break;
                    }
                }

                table.AutoFit = AutoFit.Contents;

                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Wrote out lookup table " + lookupTable.Name + " successfully"));
            }
        }

        private DataTable GetLookupTableInfoContentsFromDatabase(TableInfo lookupTable)
        {
            //get the contents of the lookup
            using(var con = DataAccessPortal.GetInstance().ExpectServer(lookupTable,DataAccessContext.InternalDataProcessing).GetConnection())
            {
                con.Open();
               
                var cmd = DatabaseCommandHelper.GetCommand("Select * from " + lookupTable.Name, con);
                var da = DatabaseCommandHelper.GetDataAdapter(cmd);

                var dt = new System.Data.DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private void AddImages(DocX document, BitmapWithDescription[] onRequestCatalogueImages)
        {
            foreach (BitmapWithDescription image in onRequestCatalogueImages)
            {
                if(!string.IsNullOrWhiteSpace(image.Header))
                    InsertHeader(document,image.Header,3);

                if (!string.IsNullOrWhiteSpace(image.Description))
                    InsertParagraph(document, image.Description);

                InsertPicture(document,image.Bitmap);
            }
        }

        private void CreateDescriptionsTable(DocX document, Catalogue c)
        {
            var extractionInformations = c.GetAllExtractionInformation(ExtractionCategory.Any).ToList();
            extractionInformations.Sort();

            var table = InsertTable(document,extractionInformations.Count + 1, 3);
            
            int tableLine = 0;

            SetTableCell(table, tableLine, 0, "Column", TextFontSize);
            SetTableCell(table, tableLine, 1, "Datatype", TextFontSize);
            SetTableCell(table, tableLine, 2, "Description", TextFontSize);

            tableLine++;


            foreach (ExtractionInformation information in extractionInformations)
            {
                SetTableCell(table,tableLine, 0, information.GetRuntimeName(),TextFontSize);
                SetTableCell(table,tableLine, 1, information.ColumnInfo.Data_type,TextFontSize);
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

                SetTableCell(table,tableLine, 2,description, TextFontSize);

                tableLine++;
            }

            table.AutoFit = AutoFit.Contents;
        }

        private void CreateCountTable(DocX document, int recordCount, int distinctCount, string identifierName)
        {
            var table = InsertTable(document,2, identifierName != null && _includeDistinctRowCounts ? 2 : 1);
            
            int tableLine = 0;

            SetTableCell(table,tableLine, 0, "Records",TextFontSize);

            //only add column values if there is an IsExtractionIdentifier returned 
            if (identifierName != null && _includeDistinctRowCounts)
                SetTableCell(table,tableLine, 1, "Distinct " + identifierName,TextFontSize);
            
            tableLine++;

            
            SetTableCell(table,tableLine, 0,recordCount.ToString("N0"),TextFontSize);

            //only add column values if there is an IsExtractionIdentifier returned 
            if (identifierName != null &&  _includeDistinctRowCounts)
                SetTableCell(table, tableLine, 1, distinctCount.ToString("N0"), TextFontSize);
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
