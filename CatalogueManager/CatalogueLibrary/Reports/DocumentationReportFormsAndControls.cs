using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Extracts comments from SourceCodeForSelfAwareness.zip for the requested Types which are made available in the public property Summaries.  This ensures that you always
    /// have the latest documentation available at runtime and that descriptions match the codebase 100%.
    /// </summary>
    public class DocumentationReportFormsAndControls: ICheckable
    {
        private readonly Type[] _formsAndControls;
        public Dictionary<Type, string> Summaries = new Dictionary<Type, string>();

        public DocumentationReportFormsAndControls(Type[] formsAndControls)
        {
            _formsAndControls = formsAndControls;
        }

        public void Check(ICheckNotifier notifier)
        {
            const string zipArchive = "SourceCodeForSelfAwareness.zip";

            using (var z = ZipFile.Open(zipArchive,ZipArchiveMode.Read))
            {
                foreach (Type t in _formsAndControls)
                {
                    if(t.Name.EndsWith("_Design"))
                        continue;

                    try
                    {
                        //spontaneous objects don't exist in the database.
                        notifier.OnCheckPerformed(new CheckEventArgs("Found Type " + t.Name, CheckResult.Success,null));
                    }
                    catch(Exception)
                    {
                        continue;
                    }

                    //it's an abstract empty design class
                    if(t.Name.EndsWith("_Design"))
                        continue;
                    
                    string toFind;

                    //if it's a generic
                    if (t.Name.EndsWith("`1"))
                        toFind = t.Name.Substring(0, t.Name.Length - "`1".Length) + ".cs"; //trim off the tick 1
                    else
                        toFind = t.Name + ".cs";//its just regular
                    
                    var entries = z.Entries.Where(e => e.Name.Equals(toFind,StringComparison.CurrentCultureIgnoreCase)).ToArray();

                    if (entries.Length != 1)
                    {
                        notifier.OnCheckPerformed(
                            new CheckEventArgs("Found " + entries.Length + " files called " + toFind,
                                CheckResult.Fail));
                        continue;
                    }

                    string classSourceCode = new StreamReader(entries[0].Open()).ReadToEnd();

                    try
                    {
                        string definition = DocumentationReportMapsDirectlyToDatabase.GetSummaryFromContent(t, classSourceCode, notifier);

                        //somehow we have 2 copies of this?
                        if(Summaries.ContainsKey(t))
                            continue;

                        Summaries.Add(t, StripTags(definition??"Not documented"));
                    }
                    catch (Exception e)
                    {
                        notifier.OnCheckPerformed(
                            new CheckEventArgs("Failed to get definition for class " + t.FullName, CheckResult.Fail,
                                e));
                    }

                }
            }
        }

        public static string StripTags(string s)
        {
            string toReturn = s.Replace("<para>", "").Replace("</para>", "").Replace("<see cref=\"","").Replace("\"/>","");
            
            return toReturn;
        }
    }
}
