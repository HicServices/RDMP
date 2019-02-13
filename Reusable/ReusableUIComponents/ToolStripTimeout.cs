using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    public class ToolStripTimeout
    {
        ToolStripLabel timeoutLabel = new ToolStripLabel("Timeout:");
        ToolStripTextBox tbTimeout = new ToolStripTextBox(){Text = "300"};
        private int _timeout;

        public int Timeout
        {
            get { return _timeout; }
            set
            {
                _timeout = value;
                tbTimeout.Text = value.ToString();
            }
        }

        public ToolStripTimeout()
        {
            tbTimeout.TextChanged += tbTimeout_TextChanged;
        }
        public IEnumerable<ToolStripItem> GetControls()
        {
            yield return timeoutLabel;
            yield return tbTimeout;
        }

        private void tbTimeout_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _timeout = int.Parse(tbTimeout.Text);
                tbTimeout.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTimeout.ForeColor = Color.Red;
            }
        }
    }
}
