using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;
using ReusableUIComponents;

namespace DataExportManager.Collections.Nodes
{
    public class ExtractionFolderNode
    {
        private readonly Project _project;

        public ExtractionFolderNode(Project project)
        {
            _project = project;

        }

        public bool CanActivate()
        {
            if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
                return false;

            return new DirectoryInfo(_project.ExtractionDirectory).Exists;
        }

        public void Activate()
        {
            if(CanActivate())
                try
                {
                    Process.Start(_project.ExtractionDirectory);
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show(e);
                }
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
                return "???";

            return CompactPath(_project.ExtractionDirectory,70);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx(
           [Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        public static string CompactPath(string longPathName, int wantedLength)
        {
            // NOTE: You need to create the builder with the 
            //       required capacity before calling function.
            // See http://msdn.microsoft.com/en-us/library/aa446536.aspx
            StringBuilder sb = new StringBuilder(wantedLength + 1);
            PathCompactPathEx(sb, longPathName, wantedLength + 1, 0);
            return sb.ToString();
        }
    }
}
