using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using Xceed.Words.NET;
using Image = System.Drawing.Image;

namespace CatalogueLibrary.Reports
{
    public delegate Bitmap RequestTypeImagesHandler(Type t);

    /// <summary>
    /// Turns the comment summaries extracted from SourceCodeForSelfAwareness.zip by DocumentationReportFormsAndControls into a user readable Microsoft Word file.  This
    /// includes writing out the headers and images of the controls.  Note RequestTypeImagesHandler delegate is used to generate the actual interface images.  This is done
    /// normally by launching the UI control and programatically screencapturing it (See DocumentationReportFormsAndControlsUI)
    /// </summary>
    public class DocumentationReportFormsAndControlsOfficeBit:RequiresMicrosoftOffice
    {
        public void GenerateReport(CommentStore commentStore,ICheckNotifier notifier, Dictionary<string, List<Type>> formsAndControlsByApplication, RequestTypeImagesHandler imageFetcher, Dictionary<string, Bitmap> wordImageDictionary, Dictionary<string, Bitmap> icons)
        {
            try
            {
                var f = GetUniqueFilenameInWorkArea("DocumentationReport");

                using (DocX document = DocX.Create(f.FullName))
                {
                    InsertHeader(document,"User Interfaces");

                    //all type names
                    var headers = new HashSet<string>(formsAndControlsByApplication.SelectMany(v => v.Value).Select(t => t.Name));
                    
                    foreach (var kvp in formsAndControlsByApplication)
                    {
                        if (!kvp.Value.Any())
                            continue;

                        InsertHeader(document,kvp.Key);

                        var report = new DocumentationReportFormsAndControls(commentStore,kvp.Value.ToArray());
                        report.Check(notifier);

                        Type[] keys = report.Summaries.Keys.ToArray();

                        for (int i = 0; i < report.Summaries.Count; i++)
                        {
                            InsertHeader(document, keys[i].Name,2);

                            Bitmap img = imageFetcher(keys[i]);

                            if (img != null)
                                InsertPicture(document, img);

                            InsertBodyText(document, headers, report.Summaries[keys[i]], icons);
                        }
                    }

                    AddBookmarks(document);

                    document.Save();
                    ShowFile(f);

                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed", CheckResult.Fail, e));
            }
        }

        private void InsertBodyText(DocX document,HashSet<string> headers, string s, Dictionary<string, Bitmap> icons)
        {
            var p = document.InsertParagraph();
            
            foreach (var word in s.Split(' '))
            {
                if (headers.Contains(word))
                {
                    var hyper = p.Append("");
                    AddCrossReference(document, hyper, word);
                    hyper.StyleName = "Strong";
                }
                else
                    p.Append(word);

                p.Append(" ");

                if (GetIconKeyForWordIfAny(word, icons) != null)
                    AddImage(document,p,icons,word);
            }
        }

        private string GetIconKeyForWordIfAny(string word, Dictionary<string, Bitmap> icons)
        {
            var exactMatch = icons.Keys.SingleOrDefault(k => k.Equals(word, StringComparison.CurrentCultureIgnoreCase));

            if (exactMatch != null)
                return exactMatch;

            if(word.Length > 1 && word.EndsWith("s"))
            {
                var nonPlural = word.Substring(0, word.Length - 1);
                return icons.Keys.SingleOrDefault(k => k.Equals(nonPlural, StringComparison.CurrentCultureIgnoreCase));
            }

            //no compatible icon
            return null;
        }

        private void AddImage(DocX document, Paragraph p, Dictionary<string, Bitmap> icons, string word)
        {
            p.Append("(");
            var pict = GetPicture(document, icons[GetIconKeyForWordIfAny(word, icons)]);
            p.AppendPicture(pict);
            p.Append(") ");
        }


        private void AddBookmarks(DocX doc)
        {
            var headers = doc.Paragraphs.Where(p => p.StyleName != null && p.StyleName.StartsWith("Heading")).ToArray();
            
            //make each header a bookmark
            foreach (var header in headers)
            {
                string h = header.Text;
                header.AppendBookmark(h);
            }
        }

        internal void AddCrossReference(DocX doc, Paragraph p, string destination)
        {
            XNamespace ns = doc.Xml.Name.NamespaceName;
            XNamespace xmlSpace = doc.Xml.GetNamespaceOfPrefix("xml");
            p = p.Append("");
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "begin"))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "instrText", new XAttribute(xmlSpace + "space", "preserve"), String.Format(" PAGEREF {0} \\h ", destination))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "separate"))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "rPr", new XElement(ns + "noProof")), new XElement(ns + "t", destination)));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "end"))));
        }
    }
}
