using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CatalogueLibrary;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    /// <summary>
    /// Allows you to either create a new HICProjectDirectory or point the software to an existing one.  These folders have a special hierarchy including Cache,ForArchiving, ForLoading, 
    /// Executables etc.  In almost all cases you want to have a different directory for each load, this prevents simultaneous loads tripping over one another.
    /// 
    /// <para>To create a new directory with all the appropriate folders and example configuration files enter the path to an empty folder.  If the folder does not exist yet it will be created
    /// when you click Ok.</para>
    /// 
    /// <para>Alternatively if you want to reuse an existing directory (for example if you have accidentally deleted your old data load configuration and lost the reference to it's folder) then
    /// you can select the 'use existing' checkbox and enter the path to the existing folder (this should be the root folder i.e. not the Data folder).  This will run Checks on the folder
    /// to confirm that it is has an intact structure and then use it for your load.</para>
    /// 
    /// </summary>
    public partial class ChooseHICProjectDialog : Form
    {
        /// <summary>
        /// The users final choice of project directory, also check DialogResult for Ok / Cancel
        /// </summary>
        public HICProjectDirectory Result { get; private set; }

        Regex _endsWithDataFolder = new Regex(@"[/\\]Data[/\\ ]*$", RegexOptions.IgnoreCase);

        public ChooseHICProjectDialog()
        {
            InitializeComponent();
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            tbCreateNew.Enabled = rbCreateNew.Checked;
            tbUseExisting.Enabled = rbUseExisting.Checked;
            btnOk.Enabled = true;

        }

        private void tbUseExisting_Leave(object sender, EventArgs e)
        {
            CheckExistingProjectDirectory();
        }

        private void CheckExistingProjectDirectory()
        {
            ragSmiley1.Visible = true;
            try
            {
                new HICProjectDirectory(tbUseExisting.Text);
                ragSmiley1.Reset();
            }
            catch (Exception ex)
            {
                ragSmiley1.Fatal(ex);
            }
        }


        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rbCreateNew.Checked)
            {
                try
                {
                    var dir = new DirectoryInfo(tbCreateNew.Text);
                
                    if(!dir.Exists)
                        dir.Create();

                    Result = HICProjectDirectory.CreateDirectoryStructure(dir.Parent,dir.Name);

                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
            }

            if (rbUseExisting.Checked)
            {
                try
                {
                    Result = new HICProjectDirectory(tbUseExisting.Text);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show(exception);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnCreateNewBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            
            if (fbd.ShowDialog() == DialogResult.OK)
                tbCreateNew.Text = fbd.SelectedPath;
        }

        private void btnBrowseForExisting_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                tbUseExisting.Text = fbd.SelectedPath;
                CheckExistingProjectDirectory();
            }
        }

        private void tbUseExisting_TextChanged(object sender, EventArgs e)
        {
            lblDataIsReservedWordExisting.Visible = _endsWithDataFolder.IsMatch(tbUseExisting.Text);
        }

        private void tbCreateNew_TextChanged(object sender, EventArgs e)
        {
            lblDataIsReservedWordNew.Visible = _endsWithDataFolder.IsMatch(tbCreateNew.Text);
        }
    }
}
