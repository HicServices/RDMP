using Rdmp.Core.CommandExecution;
using ReusableLibraryCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.Performance
{
    public partial class LastCommandUI : Form
    {
        public LastCommandUI()
        {
            InitializeComponent();

            TopMost = true;
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(label1.Text);                
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = BasicCommandExecution.LastCommand?.GetType().Name ?? "No Commands Run Yet";
        }
    }
}
