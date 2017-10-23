using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ReusableUIComponents
{
    /// <summary>
    /// Prompts the user to type in some text.  There will be a title text telling you what the system expects you to type (e.g. some DQE annotation text).
    /// </summary>
    [TechnicalUI]
    public partial class TypeTextOrCancelDialog : Form
    {
        public string ResultText {get { return textBox1.Text; }}

        public TypeTextOrCancelDialog(string header, string label, int maxCharacters, string startingTextForInputBox = null)
        {
            InitializeComponent();

            this.Text = header;
            this.label1.Text = label;
            this.textBox1.MaxLength = maxCharacters;

            textBox1.Text = startingTextForInputBox;

        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                btnCancel_Click(null, null);

            if (e.KeyCode == Keys.Enter)
                btnOk_Click(null, null);

        }

    }
}
