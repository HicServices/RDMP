using MongoDB.Driver;
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
    public partial class AdditionalInfomation : UserControl
    {
        private string _text = null;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TooltipText
        {
            get => _text;
            set {
                _text = value;
                this.toolTip1.SetToolTip(this.pictureBox1, this._text);

            }
        }

        public AdditionalInfomation()
        {
            InitializeComponent();
        }
    }
}
