using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Cursors = System.Windows.Forms.Cursors;

namespace ReusableUIComponents.LinkLabels
{
    public class PathLinkLabel : Label
    {
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if(!string.IsNullOrWhiteSpace(Text))
                try
                {
                    string toLaunch = Text.Trim();

                    Process.Start(toLaunch);
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            //paint background
            using (SolidBrush b = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(b, Bounds);
            
            //paint text
            using(Font f = new Font(Font, FontStyle.Underline))
                TextRenderer.DrawText(e.Graphics, Text, f, ClientRectangle, Color.Blue, TextFormatFlags.PathEllipsis);
        }
    }
}
