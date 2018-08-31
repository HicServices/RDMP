using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// Extracts comments from SourceCodeForSelfAwareness.zip for the requested Types which are made available in the public property Summaries.  This ensures that you always
    /// have the latest documentation available at runtime and that descriptions match the codebase 100%.
    /// </summary>
    public class DocumentationReportFormsAndControls: ICheckable
    {
        private readonly CommentStore _commentStore;
        private readonly Type[] _formsAndControls;
        public Dictionary<Type, string> Summaries = new Dictionary<Type, string>();

        public DocumentationReportFormsAndControls(CommentStore commentStore,Type[] formsAndControls)
        {
            _commentStore = commentStore;
            _formsAndControls = formsAndControls;
        }

        public void Check(ICheckNotifier notifier)
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
                    toFind = t.Name.Substring(0, t.Name.Length - "`1".Length); //trim off the tick 1
                else
                    toFind = t.Name;//its just regular

                if (_commentStore.ContainsKey(toFind))
                    Summaries.Add(t, _commentStore[toFind]);
                else if (_commentStore.ContainsKey("I" + toFind))
                    Summaries.Add(t, _commentStore["I" + toFind]);
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Failed to get definition for class " + t.FullName, CheckResult.Fail));

            }
            
        }

        public static string StripTags(string s)
        {
            string toReturn = s.Replace("<para>", "").Replace("</para>", "").Replace("<see cref=\"","").Replace("\"/>","");
            
            return toReturn;
        }
    }
}
