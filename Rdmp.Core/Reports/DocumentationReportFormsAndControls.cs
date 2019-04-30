// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NPOI.XWPF.UserModel;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace Rdmp.Core.Reports
{
    public delegate Bitmap RequestTypeImagesHandler(Type t);

    /// <summary>
    /// Turns the comment summaries extracted from SourceCodeForSelfAwareness.zip by DocumentationReportFormsAndControls into a user readable Microsoft Word file.  This
    /// includes writing out the headers and images of the controls.  Note RequestTypeImagesHandler delegate is used to generate the actual interface images.  This is done
    /// normally by launching the UI control and programatically screencapturing it (See DocumentationReportFormsAndControlsUI)
    /// </summary>
    public class DocumentationReportFormsAndControls:DocXHelper
    {
        public readonly Dictionary<Type, string> Summaries = new Dictionary<Type, string>();

        public void GenerateReport(CommentStore commentStore,ICheckNotifier notifier, Dictionary<string, List<Type>> formsAndControlsByApplication, RequestTypeImagesHandler imageFetcher, Dictionary<string, Bitmap> wordImageDictionary, Dictionary<string, Bitmap> icons)
        {
            try
            {
                using (var document = GetNewDocFile("DocumentationReport"))
                {
                    InsertHeader(document,"User Interfaces");

                    //all type names
                    var headers = new HashSet<string>(formsAndControlsByApplication.SelectMany(v => v.Value).Select(t => t.Name));
                    
                    foreach (var kvp in formsAndControlsByApplication)
                    {
                        if (!kvp.Value.Any())
                            continue;

                        InsertHeader(document,kvp.Key);

                        Check(notifier,commentStore,kvp.Value.ToArray());

                        Type[] keys = Summaries.Keys.ToArray();

                        for (int i = 0; i < Summaries.Count; i++)
                        {
                            InsertHeader(document, keys[i].Name,2);

                            Bitmap img = imageFetcher(keys[i]);

                            if (img != null)
                                GetPicture(document, img);

                            InsertBodyText(document, headers, Summaries[keys[i]], icons);
                        }
                    }

                    AddBookmarks(document);

                    ShowFile(document);

                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed", CheckResult.Fail, e));
            }
        }

        public void Check(ICheckNotifier notifier, CommentStore commentStore, Type[] formsAndControls)
        {
            Summaries.Clear();

            foreach (Type t in formsAndControls)
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

                var docs = commentStore.GetTypeDocumentationIfExists(t,true,true);

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
        
        private IDisposable FileStream()
        {
            throw new NotImplementedException();
        }

        private void InsertBodyText(XWPFDocument document,HashSet<string> headers, string s, Dictionary<string, Bitmap> icons)
        {
            var para = document.CreateParagraph();
            XWPFRun run = para.CreateRun();
            
            foreach (var word in s.Split(' '))
            {
         /*       if (headers.Contains(word))
                {
                    run.AppendText("");
                    AddCrossReference(document,para, word);
                }
                else*/
                    run.AppendText(word);

                run.AppendText(" ");

                if (GetIconKeyForWordIfAny(word, icons) != null)
                    AddImage(document,para,icons,word);
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

        private void AddImage(XWPFDocument document, XWPFParagraph p, Dictionary<string, Bitmap> icons, string word)
        {
            var run = p.Runs.Last();
            run.AppendText("(");
            var pict = GetPicture(run, icons[GetIconKeyForWordIfAny(word, icons)]);
            run.AppendText(")");
        }


        private void AddBookmarks(XWPFDocument doc)
        {
            /*var headers = doc.Paragraphs.Where(p => p.StyleName != null && p.StyleName.StartsWith("Heading")).ToArray();
            
            //make each header a bookmark
            foreach (var header in headers)
            {
                string h = header.Text;
                header.AppendBookmark(h);
            }*/
        }

        internal void AddCrossReference(XWPFDocument doc, XWPFParagraph p, string destination)
        {
            /*XNamespace ns = doc.Xml.Name.NamespaceName;
            XNamespace xmlSpace = doc.Xml.GetNamespaceOfPrefix("xml");
            p = p.Append("");
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "begin"))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "instrText", new XAttribute(xmlSpace + "space", "preserve"), String.Format(" PAGEREF {0} \\h ", destination))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "separate"))));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "rPr", new XElement(ns + "noProof")), new XElement(ns + "t", destination)));
            p.Xml.Add(new XElement(ns + "r", new XElement(ns + "fldChar", new XAttribute(ns + "fldCharType", "end"))));*/
        }
    }
}
