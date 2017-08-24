using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.SimpleDialogs.Revertable;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Governance
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
    /// provider.  It is important to store the document trail and schedule (e.g. do you require yearly re-approval) for audit purposes.  The RDMP does this through Governance Periods
    /// (See GovernanceUI).  
    /// 
    /// This control allows you to configure/view attachments of a GovernancePeriod (e.g. an email, a scan of a signed approval letter etc). For ease of reference you should describe
    /// what is in the document (e.g. 'letter to Fife healthboard (Mary Sue) listing the datasets we host and requesting re-approval for 2016.  Letter is signed by Dr Governancer.)'
    /// 
    /// This control saves changes as you make them (there is no need to press Ctrl+S)
    /// </summary>
    public partial class GovernanceDocumentUI : UserControl
    {
        private GovernanceDocument _governanceDocument;

        public GovernanceDocument GovernanceDocument
        {
            get { return _governanceDocument; }
            set
            {
                _governanceDocument = value;

                this.Enabled = value != null;

                if (value == null)
                {
                    tbName.Text = "";
                    tbDescription.Text = "";
                    tbID.Text = "";
                    tbPath.Text = "";
                }
                else
                {
                    tbName.Text = value.Name;
                    tbDescription.Text = value.Description;
                    tbID.Text = value.ID.ToString();
                    tbPath.Text = value.URL;
                }

            }
        }

        public GovernanceDocumentUI()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = ofd.FileName;
            }
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {

            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }
            try
            {
                if (_governanceDocument != null)
                {
                    _governanceDocument.Name = tbName.Text;
                    _governanceDocument.SaveToDatabase();
                }

                tbName.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbName.ForeColor = Color.Red;
            }
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_governanceDocument != null)
                {
                    _governanceDocument.Description = tbDescription.Text;
                    _governanceDocument.SaveToDatabase();
                }
                tbDescription.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbDescription.ForeColor = Color.Red;
            }
            
        }

        private void tbPath_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPath.Text))
                tbPath.Text = "<<<Missing File Path>>>";
            try
            {
                if (_governanceDocument != null)
                {
                    _governanceDocument.URL = tbPath.Text;
                    _governanceDocument.SaveToDatabase();
                }
                tbPath.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbPath.ForeColor = Color.Red;
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(tbPath.Text);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        private void btnOpenContainingFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new FileInfo(tbPath.Text).Directory.FullName);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

    }
}
