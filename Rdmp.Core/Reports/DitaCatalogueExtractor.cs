// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Reports;

/// <summary>
/// Generates an extract of all the Catalogues and CatalogueItems in your Catalogue database in .dita format.  Dita is apparently all the rage when it comes to metadata
/// sharing.
/// </summary>
public class DitaCatalogueExtractor : ICheckable
{
    //use http://sourceforge.net/projects/dita-ot/files/DITA-OT%20Stable%20Release/DITA%20Open%20Toolkit%201.8/DITA-OT1.8.M2_full_easy_install_bin.zip/download
    //to convert .dita files into html
        
    private readonly ICatalogueRepository _repository;
    private readonly DirectoryInfo _folderToCreateIn;

    /// <summary>
    /// Prepares class to convert all <see cref="Catalogue"/> stored in the <paramref name="repository"/> into .dita files containing dataset/column descriptions.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="folderToCreateIn"></param>
    public DitaCatalogueExtractor(ICatalogueRepository repository, DirectoryInfo folderToCreateIn)
    {
        _repository = repository;
        _folderToCreateIn = folderToCreateIn;
    }

    /// <summary>
    /// Generates the dita files and logs progress / errors to the <paramref name="listener"/>
    /// </summary>
    /// <param name="listener"></param>
    public void Extract(IDataLoadEventListener listener)
    {
        var xml = "";
        xml += $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE map PUBLIC ""-//OASIS//DTD DITA Map//EN""
""map.dtd"">{Environment.NewLine}";
        xml += $"<map>{Environment.NewLine}";
        xml += $"<title>HIC Data Catalogue</title>{Environment.NewLine}";

        xml += $@"<topicmeta product=""hicdc"" rev=""1"">{Environment.NewLine}";
        xml += $"<author>Wilfred Bonney; Thomas Nind; Mikhail Ghattas</author>{Environment.NewLine}";
        xml += $"<publisher>Health Informatics Centre (HIC), University of Dundee</publisher>{Environment.NewLine}";
        xml += $"</topicmeta>{Environment.NewLine}";
            
            
        xml += $@"<topicref href=""introduction.dita""/>{Environment.NewLine}";
        GenerateIntroductionFile("introduction.dita");

        xml += $@"<topicref href=""dataset.dita"">{Environment.NewLine}";
        GenerateDataSetFile("dataset.dita");

        xml += Environment.NewLine;

        //get all the catalogues then sort them alphabetically
        var catas = new List<Catalogue>(_repository.GetAllObjects<Catalogue>().Where(c => !(c.IsDeprecated || c.IsInternalDataset || c.IsColdStorageDataset)));
        catas.Sort();

        var sw = Stopwatch.StartNew();

        var cataloguesCompleted = 0;
        foreach (var c in catas)
        {
            listener.OnProgress(this, new ProgressEventArgs("Extracting", new ProgressMeasurement(cataloguesCompleted++,ProgressType.Records,catas.Count), sw.Elapsed));

            //ensure that it has an acryonym
            if(string.IsNullOrWhiteSpace(c.Acronym))
                throw new Exception(
                    $"Dita Extraction requires that each catalogue have a unique Acronym, the catalogue {c.Name} is missing an Acronym");

            if (c.Name.Contains("\\") || c.Name.Contains("/"))
                throw new Exception("Dita Extractor does not support catalogues with backslashes or forward slashs in their name");

            //catalogue main file
            xml += $"<topicref href=\"{GetFileNameForCatalogue(c)}\">{Environment.NewLine}";
            CreateCatalogueFile(c);

            //catalogue items
            var cataItems = c.CatalogueItems.ToList();
            cataItems.Sort();

            foreach (var ci in cataItems)
            {
                xml += $"<topicref href=\"{GetFileNameForCatalogueItem(c, ci)}\"/>{Environment.NewLine}";
                CreateCatalogueItemFile(c,ci);        
            }
            xml += $"</topicref>{Environment.NewLine}";

            //completed - mostly for end of loop tbh 
            listener.OnProgress(this, new ProgressEventArgs("Extracting", new ProgressMeasurement(cataloguesCompleted, ProgressType.Records, catas.Count), sw.Elapsed));
        }

        xml += Environment.NewLine;
        xml += $@"</topicref>{Environment.NewLine}";
        xml += "</map>";

        File.WriteAllText(Path.Combine(_folderToCreateIn.FullName ,"hic_data_catalogue.ditamap"  ),xml);

    }

      

    private string GetFileNameForCatalogueItem(Catalogue c,CatalogueItem ci)
    {
        var parentName = FixName(c.Acronym);
        var childName = FixName(ci.Name);
        return $"{parentName}_{childName}.dita";
    }

    private string GetFileNameForCatalogue(Catalogue catalogue)
    {
        return $"{FixName(catalogue.Name)}.dita";
    }

    private string FixName(string name)
    {
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
            name = name.Replace(invalidCharacter, '_');

        name = name.Replace("(", "");
        name = name.Replace(")", "");
        name = name.Replace(' ', '_');
        name = name.Replace("&", "and");
                
        return name.ToLower();
    }

    private void CreateCatalogueFile(Catalogue c)
    {
        var saveLocation = Path.Combine(_folderToCreateIn.FullName, GetFileNameForCatalogue(c));

        if(File.Exists(saveLocation))
            throw new Exception(
                $"Attempted to create Catalogue named {saveLocation} but it already existed (possibly you have two Catalogues with the same name");
            
        var xml = "";

        xml += @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">";
        xml += $"<concept id=\"{FixName(c.Acronym)}\">{Environment.NewLine}";

        xml += $"<title>{FixName(c.Name)}</title>{Environment.NewLine}";

        xml += $"<conbody>{Environment.NewLine}";

        xml += $@"<simpletable keycol=""1"">{Environment.NewLine}";

        xml += GenerateObjectPropertiesAsRowUsingReflection(c);


        xml += $@"</simpletable>{Environment.NewLine}";
            
        xml += $"</conbody>{Environment.NewLine}";

        xml += $"</concept>{Environment.NewLine}";

        File.WriteAllText(saveLocation,xml);
    }

    private void CreateCatalogueItemFile(Catalogue c, CatalogueItem ci)
    {
        var saveLocation = Path.Combine(_folderToCreateIn.FullName, GetFileNameForCatalogueItem(c,ci));

        if (File.Exists(saveLocation))
            throw new Exception(
                $"Attempted to create CatalogueItem named {saveLocation} but it already existed (possibly you have two CatalogueItems with the same name");

        var xml = "";

        xml += @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">";

        xml+= $"<concept id=\"{FixName(c.Name)}_{FixName(ci.Name)}\">{Environment.NewLine}";

        xml += $"<title>{FixName(ci.Name)}</title>{Environment.NewLine}";

        xml += $"<conbody>{Environment.NewLine}";

        xml += $@"<simpletable keycol=""1"">{Environment.NewLine}";

        xml += GenerateObjectPropertiesAsRowUsingReflection(ci);


        xml += $@"</simpletable>{Environment.NewLine}";

        xml += $"</conbody>{Environment.NewLine}";

        xml += $"</concept>{Environment.NewLine}";


        File.WriteAllText(saveLocation, xml);
    }

    private string GenerateObjectPropertiesAsRowUsingReflection(object o)
    {
        var toReturnXml = "";

        var propertyInfo = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //generate a strow for each property
        foreach (var property in propertyInfo)
        {
            //do not extract these
            if(property.GetCustomAttributes(typeof(DoNotExtractProperty)).Any())
                continue;
            if (property.GetCustomAttributes(typeof(NoMappingToDatabase)).Any())
                continue;
                
            //Check whether property can be written to
            if (property.CanRead)
                if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(string)))
                {
                    toReturnXml += $"<strow>{Environment.NewLine}";
                    toReturnXml += $"<stentry>{GetHtmlEncodedHeader(property.Name)}</stentry>{Environment.NewLine}";
                    toReturnXml +=
                        $"<stentry>{GetHtmlEncodedValue(property.GetValue(o, null))}</stentry>{Environment.NewLine}";
                    toReturnXml += $"</strow>{Environment.NewLine}";
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
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">
<concept id=""datasets"">
";

        xml += $"<title>HIC-held Datasets</title>{Environment.NewLine}";
        xml +=
            $"<conbody>    <p>This section of the document describes the contents of the HIC-held\r\n    datasets currently made available to researchers and data analysts. HIC’s\r\n    data collection strategy is well-developed, covering long-term collections\r\n    of whole populations, such as Scottish Morbidity Register (SMR)\r\n    hospitalisation data, official death certification data, and dispensed\r\n    community prescription data, to short-term collections and\r\n    disease-specific coverage, such as the GoDARTS (Genetics of Diabetes Audit\r\n    and Research in Tayside, Scotland). These datasets comes in various\r\n    formats and are curated, maintained and governed by HIC. </p>{Environment.NewLine}</conbody>{Environment.NewLine}</concept>{Environment.NewLine}";

        File.WriteAllText(Path.Combine(_folderToCreateIn.FullName, filename), xml);
    }

    private void GenerateIntroductionFile(string filename)
    {
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE concept PUBLIC ""-//OASIS//DTD DITA Concept//EN""
""concept.dtd"">
<concept id=""introduction"">
";

        xml += $"<title>Introduction</title>{Environment.NewLine}";
        xml += $"<conbody>placeholder introduction</conbody>{Environment.NewLine}";

        xml += $"</concept>{Environment.NewLine}";

        File.WriteAllText(Path.Combine(_folderToCreateIn.FullName, filename), xml);
    }

        
    /// <summary>
    /// Checks whether the dita file generation is likely to work e.g. that all datasets have unique acronymns etc
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        var catas = _repository.GetAllObjects<Catalogue>().Where(c => !c.IsInternalDataset && !c.IsColdStorageDataset).ToArray();

        //Catalogues with no acronyms
        foreach (var c in catas.Where(c => string.IsNullOrWhiteSpace(c.Acronym)))
        {
            var suggestion = GetAcronymSuggestionFromCatalogueName(c.Name);
            var useSuggestion = notifier.OnCheckPerformed(new CheckEventArgs($"Catalogue {c.Name} has no Acronym", CheckResult.Fail, null,
                $"Assign it a suggested acronym: '{suggestion}'?"));
                
            if(useSuggestion)
            {
                c.Acronym = suggestion;
                c.SaveToDatabase();
            }
        }

        //acronym collisions
        for(var i=0;i<catas.Length;i++)
        {
            var acronym = catas[i].Acronym;

            for (var j = i + 1; j < catas.Length; j++)
            {
                if (catas[j].Acronym.Equals(acronym))
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        string.Format(
                            "Duplication in acronym between Catalogues {0} and {1}, duplicate acronym value is {2}",
                            catas[i], catas[j], acronym), CheckResult.Fail, null));
            }
                
        }
    }

    /// <summary>
    /// Suggests an appropriate short acryonymn based on the supplied full <paramref name="name"/> e.g. BIO for Biochemistry
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetAcronymSuggestionFromCatalogueName(string name)
    {
        //concatenate all the capitals (and digits)
        var capsConcat = name.Where(c => char.IsUpper(c) || char.IsDigit(c)).Aggregate("", (s, n) => s + n);

        //if the capitals and digits go together to make something that is less than 10 long then suggest that
        if (capsConcat.Length >1 && capsConcat.Length < 10)
            return capsConcat;

        //else try to split up stuff and make suggestions based on that
        var words = Regex.Split(name, "\\s_").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        if(words.Length == 0)
            throw new Exception(
                $"Could not generate acronym suggestion for name '{name}' because split resulted in 0 words");

        //if there is only 1 word in the Catalogue name
        if(words.Length == 1)
            if (words[0].Length < 10)
                return words[0];        //if the only word is less than 10 long it can be used as acronym anyway (will be the same as catalogue name)
            else
                return words[0][..5]; //there's only one word so just suggest using the first 5 letters... suboptimal but hey whatever

        //return the first letter from every word and also add in all numbers that appear after the first letter in the word
        return words.Aggregate("", (s, n) => s + n[..1] + n.Skip(1).Where(char.IsDigit));
    }
}