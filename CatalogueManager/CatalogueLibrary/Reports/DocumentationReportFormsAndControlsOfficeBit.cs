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
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;
using Xceed.Words.NET;
using Image = System.Drawing.Image;

namespace CatalogueLibrary.Reports
{
    public delegate Bitmap RequestTypeImagesHandler(Type t);

    public class DocumentationReportFormsAndControlsOfficeBit:RequiresMicrosoftOffice
    {
        private Dictionary<string, Bitmap> _wordImageDictionary;

        public void GenerateReport(ICheckNotifier notifier, Dictionary<string, List<Type>> formsAndControlsByApplication, RequestTypeImagesHandler imageFetcher, Dictionary<string, Bitmap> wordImageDictionary)
        {
            _wordImageDictionary = wordImageDictionary;
            try
            {
                using (DocX document = DocX.Create("DocumentationReport.docx"))
                {
                    InsertTitle(document,"User Interfaces");

                    foreach (var kvp in formsAndControlsByApplication)
                    {
                        if (!kvp.Value.Any())
                            continue;

                        InsertHeader(document,kvp.Key);

                        var report = new DocumentationReportFormsAndControls(kvp.Value.ToArray());
                        report.Check(notifier);

                        Type[] keys = report.Summaries.Keys.ToArray();

                        for (int i = 0; i < report.Summaries.Count; i++)
                        {
                            InsertHeader(document, keys[i].Name,2);

                            Bitmap img = imageFetcher(keys[i]);

                            if (img != null)
                                InsertPicture(document, img);

                            InsertParagraph(document,report.Summaries[keys[i]]);
                        }
                    }

                    AddBookmarks(document);
                }
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Report generation failed", CheckResult.Fail, e));
            }
        }


        private void AddBookmarks(DocX doc)
        {
            /*
            object headers_r = doc.GetCrossReferenceItems(WdReferenceType.wdRefTypeHeading);
            Array headers = ((Array)(headers_r));
            
            string text = doc.Content.Text;

            string[] lines = text.Split('\r');

            int lineStart = 0;
            
            Regex splitByWord = new Regex("\\b");
            
            WordHelper helper = new WordHelper(wrdApp);

            //find all references to headers
            foreach (string line in lines)
            {
                string[] words = splitByWord.Split(line);

                int wordStart = 0;

                foreach (string word in words)
                {
                    bool imageAdded = false;
                    Bitmap img = null;

                    if (_wordImageDictionary.ContainsKey(word))
                        img = _wordImageDictionary[word];
                    else if (_wordImageDictionary.ContainsKey(word.TrimEnd(new[] {'s'})))
                        img = _wordImageDictionary[word.TrimEnd(new[] {'s'})];

                    if (img != null && 
                        
                        //not
                        !(
                        //things we don't want to highlight
                        string.Equals(word, "sql", StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(word, "AggregateGraph", StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Range range = doc.Range(lineStart + wordStart + word.Length , lineStart + wordStart + word.Length);

                        helper.WriteImage(img,doc,false,range);
                        imageAdded = true;
                    }

                    //word is a reference to another class
                    for(int i=1;i<=headers.Length;i++)
                    {
                        string s = (string)headers.GetValue(i);

                        //if one word in the paragraph matches a header (but it is not a header itself i.e. the entire line doesnt match)
                        if(s.Trim().Equals(word) && !s.Trim().Equals(line))
                        {
                            //select the word
                            Range range = doc.Range(lineStart + wordStart, lineStart + wordStart + word.Length);
                            range.Select();
                            range.InsertCrossReference(WdReferenceType.wdRefTypeHeading, WdReferenceKind.wdContentText,i,true);

                            lineStart += @" { REF _Ref123456789 \h } ".Length;

                            var range2 = doc.Range(lineStart + wordStart-1, lineStart + wordStart + word.Length);
                            range2.Select();
                            range2.Font.Color = WdColor.wdColorBlue;
                        }
                    }

                    if(imageAdded)
                        lineStart += 1;
                    
                    wordStart += word.Length;   
                }

                //adjust start
                lineStart += line.Length + 1;//+1 for the \r we stripped out
            }*/
        }
    }
}
