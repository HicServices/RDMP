using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.ComponentModel;

namespace Rdmp.UI.SimpleControls
{
    public partial class AdditionalInfomationUI : RDMPUserControl
    {
        private string _text = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TooltipText
        {
            get => _text;
            set
            {
                _text = value;
                toolTip1.SetToolTip(pictureBox1, _text);

            }
        }

        public AdditionalInfomationUI()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Activator.Show(_text);
        }
    }
}
