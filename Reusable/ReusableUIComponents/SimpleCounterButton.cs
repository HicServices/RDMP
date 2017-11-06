using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    /// <summary>
    /// ToolStripButton with a public Property Count on it which displays a number up to 99 (after which it displays 99+) 
    /// </summary>
    [TechnicalUI]
    [System.ComponentModel.DesignerCategory("")]
    public class SimpleCounterButton : ToolStripButton
    {
        private int? _count;

        public int? Count
        {
            get { return _count; }
            set
            {
                _count = value; 
                Invalidate();
            }
        }

        public float EmSize = 6f;
        public int LabelPadding = 2;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            base.OnPaint(e);

            var labelFont = new Font(FontFamily.GenericMonospace, EmSize);

            if (_count != null)
            {
                var label = Count.ToString();

                if (Count >= 100)
                    label = "99+";

                var labelSize = e.Graphics.MeasureString(label, labelFont);

                var labelXStart = (Width - labelSize.Width)/2;

                var labelRect = new RectangleF(new PointF(labelXStart,Height - (labelSize.Height + LabelPadding)), labelSize);
                
                e.Graphics.FillRectangle(Brushes.White,labelRect);
                e.Graphics.DrawRectangle(Pens.Gray,Rectangle.Round(labelRect));
                e.Graphics.DrawString(label,labelFont,Brushes.Black,labelRect);
    
            }
            
        }

        
    }
}