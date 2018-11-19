using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary
{
    public delegate void CatalogueProgressHandler(int progress, int target,Catalogue currentCatalogue);

    /// <summary>
    /// Generates an extract of all the Catalogues and CatalogueItems in your Catalogue database in .dita format.  Dita is apparently all the rage when it comes to metadata
    /// sharing.
    /// </summary>
    public class DitaCatalogueExtractor : ICheckable
    {

        //use http://sourceforge.net/projects/dita-ot/files/DITA-OT%20Stable%20Release/DITA%20Open%20Toolkit%201.8/DITA-OT1.8.M2_full_easy_install_bin.zip/download
        //to convert .dita files into html

        /// <summary>
        /// Triggered after each <seealso cref="Catalogue"/> is extracted
        /// </summary>
        public event CatalogueProgressHandler CatalogueCompleted;

        private readonly CatalogueRepository _repository;
        private readonly DirectoryInfo _folderToCreateIn;

        public DitaCatalogueExtractor(CatalogueRepository repository, DirectoryInfo folderToCreateIn)
        {
            _repository = repository;
            _folderToCreateIn = folderToCreateIn;
        }

        public void Extract()
        {
            string xml = "";
            xml += @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE map PUBLIC ""-//OASIS//DTD DITA Map//EN""
""map.dtd"">" + Environment.NewLine;
            xml += "<map>" + Environment.NewLine;
            xml += "<title>HIC Data Catalogue</title>" + Environment.NewLine;

            xml += @"<topicmeta product=""hicdc"" rev=""1"">" + Environment.NewLine;
            xml += "<author>Wilfred Bonney; Thomas Nind; Mikhail Ghattas</author>" + Environment.NewLine;
            xml += "<publisher>Health Informatics Centre (HIC), University of Dundee</publisher>" + Environment.NewLine;
            xml += "</topicmeta>" + Environment.NewLine;
            
            
            xml += @"<topicref href=""introduction.dita""/>" + Environment.NewLine;
            GenerateIntroductionFile("introduction.dita");

            xml += @"<topicref href=""dataset.dita"">" + Environment.NewLine;
            GenerateDataSetFile("dataset.dita");

            xml += Environment.NewLine;

            //get all the catalogues then sort them alphabetically
            List<Catalogue> catas = new List<Catalogue>(_repository.GetAllCatalogues().Where(c => !(c.IsDeprecated || c.IsInternalDataset || c.IsColdStorageDataset)));
            catas.Sort();

            int cataloguesCompleted = 0;
            foreach (Catalogue c in catas)
            {
                //increment catalogue - ++ comes after so will start at 0 but with the catalogue c being passed
                if (CatalogueCompleted != null)
                    CatalogueCompleted(cataloguesCompleted++, catas.Count,c);

                //ensure that it has an acryonym
                if(string.IsNullOrWhiteSpace(c.Acronym))
                    throw new Exception("Dita Extraction requires that each catalogue have a unique Acronym, the catalogue " + c.Name + " is missing an Acronym");

                if (c.Name.Contains("\\") || c.Name.Contains("/"))
                    throw new Exception("Dita Extractor does not support catalogues with backslashes or forward slashs in their name");

                //catalogue main file
                xml += "<topicref href=\"" + GetFileNameForCatalogue(c) + "\">" + Environment.NewLine;
                CreateCatalogueFile(c);

                //catalogue items
                List<CatalogueItem> cataItems = c.CatalogueItems.ToList();
                cataItems.Sort();

                foreach (CatalogueItem ci in cataItems)
                {
                    xml += "<topicref href=\"" + GetFileNameForCatalogueItem(c,ci) + "\"/>" + Environment.NewLine;
                    CreateCatalogueItemFile(c,ci);        
                }
                xml += "</topicref>" + Environment.NewLine;

                //completed - mostly for end of loop tbh 
                if (CatalogueCompleted != null)
                    CatalogueCompleted(cataloguesCompleted, catas.Count, c);
            }

            xml += Environment.NewLine;
            xml += @"</topicref>" + Environment.NewLine;
            xml += "</map>";

            File.WriteAllText(Path.Combine(_folderToCreateIn.FullName ,"hic_data_catalogue.ditamap"  ),xml);

        }

      

        private string GetFileNameForCatalogueItem(Catalogue c,CatalogueItem ci)
        {
            string parentName = FixName(c.Acronym);
            string childName = FixName(ci.Name);
            return parentName + "_" + childName+ ".dita";
        }

        private string GetFileNameForCatalogue(Catalogue catalogue)
        {
            return FixName(catalogue.Name) + ".dita";
        }

        private string FixName(string name)
        {
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                name = name.Replace(invalidCharacter, '_');

            name = name.Replace("(", "");
            name = name.Replace(")", "");
            name = name.Replace(' ', '_');
            name = name.Replace("&", "and");
                
            return name.ToLower();
        }

        private void CreateCatalogueFile(Catalogue c)
        {
            string saveLocation = Path.Combine(_folderToCreateIn.FullName, GetFileNameForCatalogue(c));

            if(File.Exists(saveLocation))
                throw new Exception("Attempted to create Catalogue named " + saveLocation + " but it already existed (possibly you have two Catalogues with the same name");
            
            string xml = "";

            xml += @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">";
            xml += "<concept id=\"" + FixName(c.Acronym) + "\">" + Environment.NewLine;

            xml += "<title>" + FixName(c.Name) + "</title>" + Environment.NewLine;

            xml += "<conbody>" + Environment.NewLine;

            xml += @"<simpletable keycol=""1"">" + Environment.NewLine;

            xml += GenerateObjectPropertiesAsRowUsingReflection(c);


            xml += @"</simpletable>" + Environment.NewLine;
            
            xml += "</conbody>" + Environment.NewLine;

            xml += "</concept>" + Environment.NewLine;

            File.WriteAllText(saveLocation,xml);
        }

        private void CreateCatalogueItemFile(Catalogue c, CatalogueItem ci)
        {
            string saveLocation = Path.Combine(_folderToCreateIn.FullName, GetFileNameForCatalogueItem(c,ci));

            if (File.Exists(saveLocation))
                throw new Exception("Attempted to create CatalogueItem named " + saveLocation + " but it already existed (possibly you have two CatalogueItems with the same name");

            string xml = "";

            xml += @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">";

            xml+= "<concept id=\"" + FixName(c.Name) + "_" + FixName(ci.Name) + "\">" + Environment.NewLine;

            xml += "<title>" + FixName(ci.Name) + "</title>" + Environment.NewLine;

            xml += "<conbody>" + Environment.NewLine;

            xml += @"<simpletable keycol=""1"">" + Environment.NewLine;

            xml += GenerateObjectPropertiesAsRowUsingReflection(ci);


            xml += @"</simpletable>" + Environment.NewLine;

            xml += "</conbody>" + Environment.NewLine;

            xml += "</concept>" + Environment.NewLine;


            File.WriteAllText(saveLocation, xml);
        }

        private string GenerateObjectPropertiesAsRowUsingReflection(object o)
        {
            string toReturnXml = "";

            PropertyInfo[] propertyInfo = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //generate a strow for each property
            foreach (PropertyInfo property in propertyInfo)
            {
                //do not extract these
                if(property.GetCustomAttributes(typeof(DoNotExtractProperty)).Any())
                    continue;
                if (property.GetCustomAttributes(typeof(NoMappingToDatabase)).Any())
                    continue;
                
                //Check whether property can be written to
                if (property.CanRead)
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        toReturnXml += "<strow>" + Environment.NewLine;
                        toReturnXml += "<stentry>" + GetHtmlEncodedHeader(property.Name) + "</stentry>" + Environment.NewLine;
                        toReturnXml += "<stentry>" + GetHtmlEncodedValue(property.GetValue(o, null)) + "</stentry>" + Environment.NewLine;
                        toReturnXml += "</strow>" + Environment.NewLine;
                    }
                    //else
                        //throw new Exception("Didn't know how to treat property called " + property.Name);
            }


            return toReturnXml;
        }

        private string GetHtmlEncodedHeader(object header)
        {
            header = header ?? "";

            header = header.ToString().Replace("_", " ");
            header = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(header.ToString());

            return HttpUtility.HtmlEncode(header);
        }

        private string GetHtmlEncodedValue(object value)
        {
            value = value ?? "";
            return HttpUtility.HtmlEncode(value);
        }

        private void GenerateDataSetFile(string filename)
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">
<concept id=""datasets"">
";

            xml += "<title>HIC-held Datasets</title>" + Environment.NewLine;
            xml += "<conbody>" +
                   @"    <p>This section of the document describes the contents of the HIC-held
    datasets currently made available to researchers and data analysts. HIC’s
    data collection strategy is well-developed, covering long-term collections
    of whole populations, such as Scottish Morbidity Register (SMR)
    hospitalisation data, official death certification data, and dispensed
    community prescription data, to short-term collections and
    disease-specific coverage, such as the GoDARTS (Genetics of Diabetes Audit
    and Research in Tayside, Scotland). These datasets comes in various
    formats and are curated, maintained and governed by HIC. </p>" + Environment.NewLine +
                   "</conbody>" + Environment.NewLine +
                   "</concept>" + Environment.NewLine;

            File.WriteAllText(Path.Combine(_folderToCreateIn.FullName, filename), xml);
        }

        private void GenerateIntroductionFile(string filename)
        {
            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">
<concept id=""introduction"">
";

            xml += "<title>Introduction</title>" + Environment.NewLine;
            xml += "<conbody>placeholder introduction</conbody>" + Environment.NewLine;

            xml += "</concept>" + Environment.NewLine;

            File.WriteAllText(Path.Combine(_folderToCreateIn.FullName, filename), xml);
        }

        
        public void Check(ICheckNotifier notifier)
        {
            var catas = _repository.GetAllCatalogues().Where(c => !c.IsInternalDataset && !c.IsColdStorageDataset).ToArray();

            //Catalogues with no acronyms
            foreach (Catalogue c in catas.Where(c => string.IsNullOrWhiteSpace(c.Acronym)))
            {
                string suggestion = GetAcronymSuggestionFromCatalogueName(c.Name);
                bool useSuggestion = notifier.OnCheckPerformed(new CheckEventArgs("Catalogue " + c.Name + " has no Acronym", CheckResult.Fail, null, "Assign it a suggested acronym: '"+suggestion +"'?"));
                
                if(useSuggestion)
                {
                    c.Acronym = suggestion;
                    c.SaveToDatabase();
                }
            }

            //acronym collisions
            for(int i=0;i<catas.Length;i++)
            {
                string acronym = catas[i].Acronym;

                for (int j = i + 1; j < catas.Length; j++)
                {
                    if (catas[j].Acronym.Equals(acronym))
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            string.Format(
                                "Duplication in acronym between Catalogues {0} and {1}, duplicate acronym value is {2}",
                                catas[i], catas[j], acronym), CheckResult.Fail, null));
                }
                
            }
            


        }

        public string GetAcronymSuggestionFromCatalogueName(string name)
        {
            //concatenate all the capitals (and digits)
            string capsConcat = name.Where(c => char.IsUpper(c) || char.IsDigit(c)).Aggregate("", (s, n) => s + n);

            //if the capitals and digits go together to make something that is less than 10 long then suggest that
            if (capsConcat.Length >1 && capsConcat.Length < 10)
                return capsConcat;

            //else try to split up stuff and make suggestions based on that
            var words = Regex.Split(name, "\\s_").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if(words.Length == 0)
                throw new Exception("Could not generate acronym suggestion for name '"+ name +"' because split resulted in 0 words");

            //if there is only 1 word in the Catalogue name
            if(words.Length == 1)
                if (words[0].Length < 10)
                    return words[0];        //if the only word is less than 10 long it can be used as acronym anyway (will be the same as catalogue name)
                else
                    return words[0].Substring(0, 5); //theres only one word so just suggest using the first 5 letters... suboptimal but hey whatever

            //return the first letter from every word and also add in all numbers that appear after the first letter in the word
            return words.Aggregate("", (s, n) => s + n.Substring(0, 1) + n.Skip(1).Where(char.IsDigit));
        }
    }
}
