using MongoDB.Driver;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                this.toolTip1.SetToolTip(this.pictureBox1, this._text);

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
