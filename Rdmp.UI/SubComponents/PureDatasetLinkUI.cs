using Rdmp.Core.Datasets.PureItems;
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

namespace Rdmp.UI.SubComponents
{
    public partial class PureDatasetLinkUI : RDMPUserControl
    {
        public PureLink Link { get; }

        public string linkUrl { get; set; }
        public string linkDescription { get; set; }
        public PureDatasetLinkUI(PureLink link)
        {
            InitializeComponent();
            Link = link;
            tbLink.Text = link.Url;
            linkUrl = link.Url;
            tbDescription.Text = link.Description.En_GB;
            linkDescription = link.Description.En_GB;
            tbLink.Width = TextRenderer.MeasureText(link.Url, tbLink.Font).Width;
            tbDescription.Width = TextRenderer.MeasureText(link.Description.En_GB, tbLink.Font).Width;
            tbDescription.Location = new Point(20 + tbLink.Width, tbDescription.Location.Y);
            this.Width = tbLink.Width + tbDescription.Width + 60;
        }

        private void tbDescription_TextChanged_1(object sender, EventArgs e)
        {
            linkDescription = tbDescription.Text;
        }

        private void tbLink_TextChanged_1(object sender, EventArgs e)
        {
            linkUrl = tbLink.Text;
        }
    }
}
