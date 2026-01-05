using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleControls
{
    public partial class EditableLabelUI : UserControl
    {
        private string _value = "";
        private string _title = "";
        private Image _image;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TextValue
        {
            get => _value;
            set
            {
                _value = value;
                lblEditable.Text = _value;
                pictureBox1.Location = new Point(lblEditable.Location.X + lblEditable.Width + 5, pictureBox1.Location.Y);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                lblTitle.Text = _title;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Icon
        {
            get => _image;
            set
            {
                _image = value;
                pbIcon.Image = _image;
            }
        }

        public EditableLabelUI()
        {
            InitializeComponent();
            tbEditable.Visible = false;
            lblEditable.Visible = true;
        }

        private void tbEditable_Lostfocus(object sender, EventArgs e)
        {
            TextValue = tbEditable.Text;
            tbEditable.Visible = false;
            lblEditable.Visible = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            tbEditable.Text = TextValue;
            lblEditable.Visible = false;
            tbEditable.Visible = true;
            tbEditable.Focus();
        }
    }
}
