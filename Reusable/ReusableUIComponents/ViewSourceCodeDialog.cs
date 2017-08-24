using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace ReusableUIComponents
{
    /// <summary>
    /// Allows you to view a class file from the RDMP codebase.  See ExceptionViewerStackTraceWithHyperlinks for the mechanics of how this works (or UserManual.docx).  A green line will
    /// highlight the line on which the message or error occurred.
    /// </summary>
    [TechnicalUI]
    public partial class ViewSourceCodeDialog : Form
    {
        private Scintilla QueryEditor;

        private static HashSet<FileInfo> SupplementalSourceZipFiles = new HashSet<FileInfo>();
        private static object oSupplementalSourceZipFilesLock = new object();

        public static void AddSupplementalSourceZipFile(FileInfo f)
        {
            lock (oSupplementalSourceZipFilesLock)
            {
                SupplementalSourceZipFiles.Add(f);
            }
        }

        public ViewSourceCodeDialog(string filename, int lineNumber, Color highlightColor)
        {
            string toFind = Path.GetFileName(filename);
            
            InitializeComponent();

            if(filename == null)
                return;
            
            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

            if (designMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create(null, "csharp");

            panel1.Controls.Add(QueryEditor);

            lock (oSupplementalSourceZipFilesLock)
            {
                string readToEnd = GetSourceForFile(toFind);
                
                //entry was found
                if (readToEnd != null)
                {
                    QueryEditor.Text = readToEnd;

                    if (lineNumber != -1)
                    {
                        QueryEditor.FirstVisibleLine = Math.Max(0, lineNumber - 10);
                        new ScintillaLineHighlightingHelper().HighlightLine(QueryEditor, lineNumber - 1, highlightColor);
                    }
                }
                else
                    throw new FileNotFoundException("Could not find file called '" + toFind + "' in any of the zip archives");
            }
       
            Text = new FileInfo(filename).Name;
        }

        public ViewSourceCodeDialog(string filename):this(filename,-1,Color.White)
        {
        }

        public static string GetSourceForFile(string toFind)
        {
            var zipArchive = new FileInfo("SourceCodeForSelfAwareness.zip");

            //for each zip file (starting with the main archive)
            foreach (var zipFile in new[] { zipArchive }.Union(SupplementalSourceZipFiles))
            {
                //if the zip exists
                if (zipFile.Exists)
                {
                    //read the entry (if it is there)
                    using (var z = ZipFile.OpenRead(zipFile.FullName))
                    {
                        var readToEnd = GetEntryFromZipFile(z, toFind);

                        if (readToEnd != null) //the entry was found and read
                            return readToEnd;
                    }
                }
            }

            //couldn't find any text
            return null;
        }

        private static string GetEntryFromZipFile(ZipArchive z,string toFind)
        {
            var entry = z.Entries.SingleOrDefault(e => e.Name == toFind);

            if (entry == null)
                return null;

            return new StreamReader(entry.Open()).ReadToEnd();
        }
    }
}
