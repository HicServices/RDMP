using System;
using System.Collections.Generic;
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

                var docs = _commentStore.GetTypeDocumentationIfExists(t);

                if(docs != null)
                {
                    if (!Summaries.ContainsKey(t))
                        Summaries.Add(t, docs);
                }
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
