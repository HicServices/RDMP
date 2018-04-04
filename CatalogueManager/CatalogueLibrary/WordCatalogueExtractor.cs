using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using ReusableLibraryCode;
using Xceed.Words.NET;

namespace CatalogueLibrary
{
    /// <summary>
    /// Generates tables in a Microsoft Word document describing a Catalogue, it's CatalogueItems and any Issues associated with it.  This is used in data extraction 
    /// to generate metadata documents for the researchers to read (See WordDataWriter)
    /// </summary>
    public class WordCatalogueExtractor: RequiresMicrosoftOffice
    {
        private ICatalogue Catalogue { get; set; }

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

        private DocX _document;


        public WordCatalogueExtractor(ICatalogue catalogue, DocX document)
        {
            Catalogue = catalogue;
            _document = document;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cataItems">The CatalogueItems you want to write out ( should all belong to the same Catalogue) but may be a subset of the whole Catalogue e.g. those columns that were actualy extracted</param>
        /// <param name="supplementalData">A bunch of key value pairs (Tuple actually) that accompany a CatalogueItem and should be rammed into the table as it is written out, this could contain information only determined at runtime e.g. not part of the Catalogue</param>
        public void AddMetaDataForColumns(CatalogueItem[] cataItems, Dictionary<CatalogueItem, Tuple<string, string>[]> supplementalData = null)
        {
            InsertHeader(_document,"Catalogue:" + Catalogue.Name);

            //first of all output all the info in the Catalogue

            int requiredRowsCount = CountWriteableProperties(Catalogue);

            var table = InsertTable(_document,requiredRowsCount, 2);

            GenerateObjectPropertiesAsRowUsingReflection(table, Catalogue, null);

            //then move onto CatalogueItems that have been extracte
            foreach (CatalogueItem catalogueItem in cataItems)
            {
                InsertHeader(_document,catalogueItem.Name,2);
                
                requiredRowsCount = CountWriteableProperties(catalogueItem);
                
                //allocate extra space for supplementalData
                if(supplementalData != null)
                    requiredRowsCount += supplementalData[catalogueItem].Length;
                
                //create a new table
                var t = InsertTable(_document, requiredRowsCount, 2, TableDesign.TableGrid);
                
                if(supplementalData!=null && supplementalData.ContainsKey(catalogueItem))
                    GenerateObjectPropertiesAsRowUsingReflection(t, catalogueItem,supplementalData[catalogueItem]);
                else
                    GenerateObjectPropertiesAsRowUsingReflection(t, catalogueItem, null);
            }
        }
        public void AddIssuesForCatalogue()
        {
            //first of all output all the info in the Catalogue
            var issues = Catalogue.GetAllIssues();
            
            InsertHeader(_document,"Issues");

            if (!issues.Any())
                InsertParagraph(_document,"No Issues recorded in this dataset");
            else
                foreach (CatalogueItemIssue issue in issues)
                {
                    string header = issue.Status + " Issue: " + issue.Name;

                    if (issue.ReportedBy_ID != null)
                        if (issue.ReportedOnDate == null)
                            header += "(Reported By " + issue.GetReportedByName() + ")";
                        else
                            header += "(Reported By " + issue.GetReportedByName() + " on " + issue.ReportedOnDate + ")";

                    InsertHeader(_document,header,2);

                    int requiredRowsCount = CountWriteableProperties(issue);
                    var t = InsertTable(_document, requiredRowsCount, 2, TableDesign.TableGrid);
                    
                    GenerateObjectPropertiesAsRowUsingReflection(t, issue, null);
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

            int currentRow = 0;

            //generate a row for each property
            foreach (PropertyInfo property in propertyInfo)
            {
                
                //Check whether property can be written to
                if (property.CanRead && Attribute.IsDefined(property, typeof(DoNotExtractProperty)) == false && !PropertyBlacklist.Contains(property.Name))
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        SetTableCell(table, currentRow, 0, property.Name);

                        object val = property.GetValue(o, null);

                        if(val != null)
                            SetTableCell(table, currentRow, 1, val.ToString());

                        currentRow++;
                    }
            }

            //add any supplemental data they want in the table
            if(supplementalData!= null)
                foreach (Tuple<string, string> tuple in supplementalData)
                {
                    SetTableCell(table, currentRow, 0, tuple.Item1);
                    SetTableCell(table, currentRow, 1, tuple.Item2);
                    currentRow++;
                }

            table.AutoFit = AutoFit.Contents;
            _document.InsertTable(table);
        }
        
        
    }
}
