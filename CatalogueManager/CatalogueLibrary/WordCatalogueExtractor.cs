using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode;

namespace CatalogueLibrary
{
    public class WordCatalogueExtractor
    {
        private ICatalogue Catalogue { get; set; }
        private Application wrdApp { get; set; }
        private _Document wrdDoc { get; set; }

        //This is an alternative for [DoNotExtractProperty] that only applies to this class where [DoNotExtractProperty] applies to all users of Catalogue e.g. DITAExtractor
        private static string[] PropertyBlacklist = new string[]
        {
            "Statistical_cons", "Research_relevance", "Topic", "Agg_method", "Limitations", "Comments", "Periodicity",
            "Acronym",
"Detail_Page_URL",
"Type",
"Periodicity",
"Coverage",
"Number_of_these",
"BackgroundSummary",
"Topics",
"Update_freq",
"Update_sched",
"Time_coverage",
"Last_revision_date",
"Contact_details",
"Data_origin",
"Attribution_citation",
"Access_options",
"API_access_URL",
"Browse_URL",
"Bulk_Download_URL",
"Query_tool_URL",
"Source_URL",
"Geographical_coverage",
"Background_summary",
"Search_keywords",
"Resource_owner",
"Granularity",
"Country_of_origin",
"Data_standards",
"Administrative_contact_name",
"Administrative_contact_email",
"Administrative_contact_telephone",
"Administrative_contact_address",
"Explicit_consent",
"Ethics_approver",
"Source_of_data_collection",
"SubjectNumbers",
"Ticket",
"DatasetStartDate",
"Name"

        };


        public WordCatalogueExtractor(ICatalogue catalogue, Application app, _Document doc)
        {
            Catalogue = catalogue;
            wrdApp = app;
            wrdDoc = doc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cataItems">The CatalogueItems you want to write out ( should all belong to the same Catalogue) but may be a subset of the whole Catalogue e.g. those columns that were actualy extracted</param>
        /// <param name="tableStyle">A word style for the table format</param>
        /// <param name="supplementalData">A bunch of key value pairs (Tuple actually) that accompany a CatalogueItem and should be rammed into the table as it is written out, this could contain information only determined at runtime e.g. not part of the Catalogue</param>
        public void AddMetaDataForColumns(CatalogueItem[] cataItems, string tableStyle, Dictionary<CatalogueItem, Tuple<string, string>[]> supplementalData = null)
        {
            var helper = new WordHelper(wrdApp);

            WriteLine("Catalogue:" + Catalogue.Name, WdBuiltinStyle.wdStyleHeading1);

            //first of all output all the info in the Catalogue
            object start = wrdApp.Selection.End;
            object end = wrdApp.Selection.End;

            int requiredRowsCount = CountWriteableProperties(Catalogue);
            Range tableLocation = wrdDoc.Range(ref start, ref end);
            Table tableOfMainCatalogueDescription = wrdDoc.Tables.Add(tableLocation,requiredRowsCount , 2);
            tableOfMainCatalogueDescription.set_Style(tableStyle);
            
            GenerateObjectPropertiesAsRowUsingReflection(tableOfMainCatalogueDescription, Catalogue,null);

            //then move onto CatalogueItems that have been extracte
            foreach (CatalogueItem catalogueItem in cataItems)
            {
                helper.GoToEndOfDocument();

                WriteLine(catalogueItem.Name, WdBuiltinStyle.wdStyleHeading2);
                
                start = wrdApp.Selection.End;
                end = wrdApp.Selection.End;

                requiredRowsCount = CountWriteableProperties(catalogueItem);
                
                //allocate extra space for supplementalData
                if(supplementalData != null)
                    requiredRowsCount += supplementalData[catalogueItem].Length;
                
                //create a new table
                tableLocation = wrdDoc.Range(ref start, ref end);
                Table tableOfCatalogueItem = wrdDoc.Tables.Add(tableLocation,requiredRowsCount , 2);
                tableOfCatalogueItem.set_Style(tableStyle);

                if(supplementalData!=null && supplementalData.ContainsKey(catalogueItem))
                    GenerateObjectPropertiesAsRowUsingReflection(tableOfCatalogueItem, catalogueItem,supplementalData[catalogueItem]);
                else
                    GenerateObjectPropertiesAsRowUsingReflection(tableOfCatalogueItem, catalogueItem, null);
            }
        }
        public void AddIssuesForCatalogue(string tableStyle)
        {
            var helper = new WordHelper(wrdApp);

            //first of all output all the info in the Catalogue
            object start = wrdApp.Selection.End;
            object end = wrdApp.Selection.End;
            Range tableLocation = wrdDoc.Range(ref start, ref end);

            var issues = Catalogue.GetAllIssues();
            
            helper.GoToEndOfDocument();

            WriteLine("Issues", WdBuiltinStyle.wdStyleHeading1);

            if (!issues.Any())
                WriteLine("No Issues recorded in this dataset", WdBuiltinStyle.wdStyleNormal);
            else
                foreach (CatalogueItemIssue issue in issues)
                {
                    helper.GoToEndOfDocument();

                    string header = issue.Status + " Issue: " + issue.Name;

                    if (issue.ReportedBy_ID != null)
                        if (issue.ReportedOnDate == null)
                            header += "(Reported By " + issue.GetReportedByName() + ")";
                        else
                            header += "(Reported By " + issue.GetReportedByName() + " on " + issue.ReportedOnDate + ")";

                    WriteLine(header, WdBuiltinStyle.wdStyleHeading2);
                    helper.GoToEndOfDocument();

                    start = wrdApp.Selection.End;
                    end = wrdApp.Selection.End;

                    tableLocation = wrdDoc.Range(ref start, ref end);

                    int requiredRowsCount = CountWriteableProperties(issue);
                    Table issueTable = wrdDoc.Tables.Add(tableLocation, requiredRowsCount, 2);
                    issueTable.set_Style(tableStyle);

                    GenerateObjectPropertiesAsRowUsingReflection(issueTable, issue, null);
                    issueTable.Columns.AutoFit();
                }
        }

        private int CountWriteableProperties(object o)
        {
            PropertyInfo[] propertyInfo =
                o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            int count = 0;
            //generate a row for each property
            foreach (PropertyInfo property in propertyInfo )
            {
                //Check whether property can be written to
                if (property.CanRead && Attribute.IsDefined(property, typeof(DoNotExtractProperty)) == false && !PropertyBlacklist.Contains(property.Name))
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum ||property.PropertyType.Equals(typeof (System.String)))
                            count++;

            }
            return count;
        }

        private void GenerateObjectPropertiesAsRowUsingReflection(Table table, object o, Tuple<string,string>[] supplementalData )
        {

            PropertyInfo[] propertyInfo = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //word tables start at 1

            int currentRow = 1;
            //generate a row for each property
            foreach (PropertyInfo property in propertyInfo)
            {
                
                //Check whether property can be written to
                if (property.CanRead && Attribute.IsDefined(property, typeof(DoNotExtractProperty)) == false && !PropertyBlacklist.Contains(property.Name))
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        table.Cell(currentRow, 1).Range.Text = property.Name;

                        object val = property.GetValue(o, null);

                        if(val != null)
                            table.Cell(currentRow, 2).Range.Text = val.ToString();

                        currentRow++;
                    }
            }

            //add any supplemental data they want in the table
            if(supplementalData!= null)
                foreach (Tuple<string, string> tuple in supplementalData)
                {
                    table.Cell(currentRow, 1).Range.Text = tuple.Item1;
                    table.Cell(currentRow, 2).Range.Text = tuple.Item2;
                    currentRow++;
                }

            table.Columns.AutoFit();
        }

        #region Word helper methods
        private void Write(string toWrite, WdBuiltinStyle style, WdColor? color = null)
        {
            Object oStyle = style;

            if (color != null)
                wrdApp.Selection.Font.Color = (WdColor)color; //set colour then type text

            wrdApp.Selection.TypeText(toWrite);

            if (color == null)
                wrdApp.Selection.set_Style(ref oStyle); //type text then overwrite with font
        }

        private void WriteLine(string toWrite, WdBuiltinStyle style, WdColor? color = null)
        {
            Object oStyle = style;

            if (color != null)
                wrdApp.Selection.Font.Color = (WdColor)color;
            else
                wrdApp.Selection.Font.Color = WdColor.wdColorBlack;

            wrdApp.Selection.TypeText(toWrite);

            if (color == null)
                wrdApp.Selection.set_Style(ref oStyle);

            wrdApp.Selection.TypeParagraph();
        }

        private void WriteLine()
        {
            wrdApp.Selection.TypeParagraph();
        }

        private void StartNewPageInDocument()
        {

            object what = WdGoToItem.wdGoToPercent;
            object which = WdGoToDirection.wdGoToLast;
            Object oMissing = System.Reflection.Missing.Value;

            wrdApp.Selection.GoTo(ref what, ref which, ref oMissing, ref oMissing);

            wrdApp.Selection.InsertBreak(WdBreakType.wdPageBreak);
        }
        #endregion

        
    }
}
