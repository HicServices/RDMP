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
        
        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            var report = GetReportIfDiff(e.AssociatedControl);
            


            try
            {
                //get height of any given line
                var coreLineHeight = e.Graphics.MeasureString("I've got a lovely bunch of coconuts", e.Font).Height + (LINE_PADDING * 2);

                var propertyNames = report.Differences.Select(d => d.Property.Name.ToString()).ToArray();
                var dividerWidth = e.Graphics.MeasureString(propertyNames.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur), e.Font).Width;
                
                int midpointX = WIDTH/2;
                float dividerStartX = midpointX - (dividerWidth/2);
                float dividerEndX = dividerStartX + dividerWidth;

                var localBackground = e.ToolTipText.Equals("Save") ? Brushes.PaleGreen : Brushes.LightPink;
                var dbBackground = e.ToolTipText.Equals("Discard") ? Brushes.PaleGreen : Brushes.LightPink;
                
                //background for local
                e.Graphics.FillRectangle(localBackground, 0, 0, midpointX, HEIGHT);

                var diffYCoords = new List<float> { coreLineHeight };
                var currentLineY = coreLineHeight;

                foreach (var difference in report.Differences)
                {
                    currentLineY += Math.Max(e.Graphics.MeasureString("" + difference.LocalValue, e.Font).Height,
                        e.Graphics.MeasureString("" + difference.DatabaseValue, e.Font).Height) + coreLineHeight;

                    diffYCoords.Add(currentLineY);
                }

                for (int i = 0; i < report.Differences.Count; i++)
                {
                    var localValue = "" + report.Differences[i].LocalValue;
                    e.Graphics.DrawString(localValue, e.Font, Brushes.Black, 0, diffYCoords.ElementAt(i));
                }

                //background for db
                e.Graphics.FillRectangle(dbBackground, midpointX, 0, midpointX, HEIGHT);

                for (int i = 0; i < report.Differences.Count; i++)
                {
                    var dbValue = "" + report.Differences[i].DatabaseValue;
                    e.Graphics.DrawString(dbValue, e.Font, Brushes.Black, dividerEndX, diffYCoords.ElementAt(i));
                }
                
                //grey divider
                e.Graphics.FillRectangle(Brushes.Gainsboro, dividerStartX, 0, dividerWidth, HEIGHT);
                e.Graphics.DrawLine(Pens.White, dividerStartX, 0, dividerStartX, HEIGHT);
                e.Graphics.DrawLine(Pens.White, dividerEndX, 0, dividerEndX, HEIGHT);

                //property names
                for (int i = 0; i < report.Differences.Count; i++)
                {
                    e.Graphics.DrawString(propertyNames[i], e.Font, Brushes.DarkSlateGray, dividerStartX, diffYCoords.ElementAt(i));
                }

                //draw the title
                e.Graphics.FillRectangle(Brushes.DarkBlue, 0, 0, WIDTH, coreLineHeight);
                e.Graphics.DrawString(e.ToolTipText + " changes", e.Font, Brushes.White, LINE_PADDING, LINE_PADDING);
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
    }
}
