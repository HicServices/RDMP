using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace ReusableUIComponents
{
    internal class ViewSourceCodeToolTip : ToolTip
    {
        private int WIDTH = 600;
        private int HEIGHT = 450;
        private int LINE_PADDING = 1;

        public ViewSourceCodeToolTip()
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
        }

        static Dictionary<string, string[]> SourceFileCache = new Dictionary<string, string[]>();

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            try
            {
                var elements = e.ToolTipText.Split(new string[]{"|"}, StringSplitOptions.RemoveEmptyEntries);
                string filename = elements[0];
                int linenumber = int.Parse(elements[1]);

                linenumber = Math.Max(0,linenumber-1);

                var lines = ReadAllLinesCached(filename);

                //get height of any given line
                var coreLineHeight = e.Graphics.MeasureString("I've got a lovely bunch of coconuts" , e.Font).Height + (LINE_PADDING*2);

                int midpointY = HEIGHT/2;

                //white background
                e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);

                //the highlighted line
                e.Graphics.FillRectangle(Brushes.LawnGreen, 0,midpointY - LINE_PADDING,WIDTH,coreLineHeight);
                e.Graphics.DrawString(lines[linenumber],e.Font,Brushes.Black,0,midpointY);

                var index = linenumber - 1;
                var currentLineY = midpointY - coreLineHeight;
            
                //any other lines we can fit on above the current line
                while(currentLineY > 0 && index >= 0)
                {
                    e.Graphics.DrawString(lines[index],e.Font,Brushes.Black,0,currentLineY);
                    currentLineY -= coreLineHeight;
                    index --;
                }

                index = linenumber + 1;
                currentLineY = midpointY + coreLineHeight;
            
                //while there are lines below us
                while (currentLineY < HEIGHT && index < lines.Length)
                {
                    e.Graphics.DrawString(lines[index], e.Font, Brushes.Black, 0, currentLineY);
                    currentLineY += coreLineHeight;
                    index++;
                }

                //draw the name of the file
                e.Graphics.FillRectangle(Brushes.DarkBlue,0,0,WIDTH,coreLineHeight);
                e.Graphics.DrawString(Path.GetFileName(filename) ,e.Font,Brushes.White,LINE_PADDING,LINE_PADDING);
            }
            catch (Exception exception)
            {
                //white background
                e.Graphics.FillRectangle(Brushes.White, 0, 0, WIDTH, HEIGHT);
                e.Graphics.DrawString(exception.Message,e.Font,Brushes.Red,new RectangleF(0,0,WIDTH,HEIGHT));
            }
        }

        private string[] ReadAllLinesCached(string filename)
        {
            if(!SourceFileCache.ContainsKey(filename))
                SourceFileCache.Add(filename,File.ReadLines(filename).ToArray());
            
            return SourceFileCache[filename];
        }
    }
}
