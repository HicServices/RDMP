using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    public partial class Chip : UserControl
    {
        private Func<string,int> _clear;
        public Chip(string value, Func<string,int> clear)
        {
            InitializeComponent();
            lblText.Text = value;
            btnClear.Location = new Point(lblText.Width + lblText.Location.X+5, btnClear.Location.Y);
            this.Size = new System.Drawing.Size(lblText.Width + lblText.Location.X + btnClear.Width+15, this.Size.Height);
            _clear = clear;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _clear(lblText.Text);
        }

        private int radius = 20;
        [DefaultValue(20)]
        public int Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                this.RecreateRegion();
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void RecreateRegion()
        {
            var bounds = ClientRectangle;

            this.Region = Region.FromHrgn(CreateRoundRectRgn(bounds.Left, bounds.Top,
                bounds.Right, bounds.Bottom, Radius, radius));
            this.Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.RecreateRegion();
        }
    }
}
