using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleControls.MultiSelectChips
{
    public partial class Chip : UserControl
    {
        private readonly Func<string, int> _clear;
        public Chip(string value, Func<string, int> clear)
        {
            InitializeComponent();
            lblText.Text = AddSpacesToSentence(value, true);
            btnClear.Location = new Point(lblText.Width + lblText.Location.X + 5, btnClear.Location.Y);
            Size = new Size(lblText.Width + lblText.Location.X + btnClear.Width + 15, Size.Height);
            _clear = clear;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            _clear(lblText.Text);
        }

        private string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1]))))
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private int radius = 20;
        [DefaultValue(20)]
        public int Radius
        {
            get { return radius; }
            set
            {
                radius = value;
                RecreateRegion();
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void RecreateRegion()
        {
            var bounds = ClientRectangle;

            Region = Region.FromHrgn(CreateRoundRectRgn(bounds.Left, bounds.Top,
                bounds.Right, bounds.Bottom, Radius, radius));
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecreateRegion();
        }
    }
}
