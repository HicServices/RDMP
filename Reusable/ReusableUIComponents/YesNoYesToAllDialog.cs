using System;
using System.Windows.Forms;

namespace ReusableUIComponents
{
    /// <summary>
    /// Asks you if you want to carry out a particular activity with the option to say Yes to this activity or 'Yes to All' (activities that are similar to this one).
    /// </summary>
    [TechnicalUI]
    public partial class YesNoYesToAllDialog : Form
    {
        private bool YesToAllClicked = false;
        private bool NoToAllClicked = false;

        public YesNoYesToAllDialog()
        {
            InitializeComponent(); 
        }

        new private DialogResult ShowDialog()
        {
            if(YesToAllClicked)
                return DialogResult.Yes;

            if(NoToAllClicked)
                return DialogResult.No;

            return base.ShowDialog();
        }

        public DialogResult ShowDialog(string message, string caption)
        {
            label1.Text = message;
            this.Text = caption;

            return ShowDialog();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnYesToAll_Click(object sender, EventArgs e)
        {
            YesToAllClicked = true;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNoToAll_Click(object sender, EventArgs e)
        {
            NoToAllClicked = true;
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    } 
}
