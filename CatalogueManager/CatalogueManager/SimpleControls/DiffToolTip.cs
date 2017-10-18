using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable.Revertable;
using ReusableUIComponents;

namespace CatalogueManager.SimpleControls
{
    [System.ComponentModel.DesignerCategory("")]
    public class DiffToolTip:ToolTip
    {
        private int WIDTH = 600;
        private int HEIGHT = 450;
        private int LINE_PADDING = 1;

        public DiffToolTip()
        {
            OwnerDraw = true;
            ToolTipIcon = ToolTipIcon.None;
            ToolTipTitle = string.Empty;

            Popup += OnPopup;
            Draw += OnDraw;
        }

        
        private void OnPopup(object sender, PopupEventArgs e)
        {
            if(Screen.PrimaryScreen != null && Screen.PrimaryScreen.Bounds != Rectangle.Empty)
            {
                //use half the screen width or 600 if they are playing on a gameboy advanced
                WIDTH = Math.Max(600,Screen.PrimaryScreen.Bounds.Width/2);
                HEIGHT = Math.Max(450, Screen.PrimaryScreen.Bounds.Height / 2);
            }

            e.ToolTipSize = new Size(WIDTH, HEIGHT);
            e.Cancel = GetReportIfDiff(e.AssociatedControl) == null;
        }

        static Dictionary<string, string[]> SourceFileCache = new Dictionary<string, string[]>();

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            var report = GetReportIfDiff(e.AssociatedControl);
            
            try
            {

                //white background
                e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);

                e.Graphics.FillRectangle(Brushes.Red, 25, 25, 25,25);
            }
            catch (Exception exception)
            {
                //white background
                e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);
                e.Graphics.DrawString(exception.Message,e.Font,Brushes.Red,new RectangleF(0,0,WIDTH,HEIGHT));
            }
        }

        private RevertableObjectReport GetReportIfDiff(Control associatedControl)
        {
            var report = associatedControl.Tag as RevertableObjectReport;
            
            if (report == null)
                return null;

            if (report.Evaluation == ChangeDescription.DatabaseCopyDifferent)
                return report;

            return null;
        }

        private string[] ReadAllLinesCached(string filename)
        {
            if(!SourceFileCache.ContainsKey(filename))
            {
                string[] fileContents;
                
                //if you have the original file
                if (File.Exists(filename))
                    fileContents = File.ReadLines(filename).ToArray();
                //otherwise get it from SourceCodeForSelfAwareness.zip / Plugin zip source codes
                else
                {
                    string contentsInOneLine = ViewSourceCodeDialog.GetSourceForFile(Path.GetFileName(filename));

                    if (contentsInOneLine == null)
                        return null;

                    fileContents = contentsInOneLine.Split('\n');
                    
                }

                SourceFileCache.Add(filename,fileContents);
            }
            
            return SourceFileCache[filename];
        }
    }
}
