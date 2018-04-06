using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.ForwardEngineering
{
    /// <summary>
    /// This dialog is shown when the RDMP learns about a new data table in your data repository that you want it to curate.  This can be either following a the successful flat file import
    /// or after selecting an existing table for importing metadata from (See ImportSQLTable).
    /// 
    /// <para>If you click 'No' then no dataset (Catalogue) will be created and you will only have the TableInfo/ColumnInfo collection stored in your RDMP database, you will need to manually wire
    /// these up to a Catalogue or delete them. </para>
    /// 
    /// <para>Alternatively you can create a new Catalogue, this will result in a Catalogue (dataset) of the same name as the table and a CatalogueItem being created for each ColumnInfo imported.
    /// If you choose to you can make these CatalogueItems extractable by creating ExtractionInformation too or you may choose to do this by hand (in CatalogueItemTab).  It is likely that
    /// you don't want to release every column in the dataset to researchers so make sure to review the extractability of the CatalogueItems created (in CatalogueItemTab). </para>
    /// 
    /// <para>You can choose a single extractable column to be the Patient Identifier (e.g. CHI / NHS number etc). This column must be the same (logically/datatype) across all your datasets i.e. 
    /// you can use either CHI number or NHS Number but you can't mix and match (but you could have fields with different names e.g. PatCHI, PatientCHI, MotherCHI, FatherChiNo etc).</para>
    /// 
    /// <para>The final alternative is to add the imported Columns to another already existing Catalogue.  Only use this option if you know it is possible to join the new table with the other 
    /// table(s) that underlie the selected Catalogue (e.g. if you are importing a Results table which joins to a Header table in the dataset Biochemistry on primary/foreign key LabNumber).
    /// If you choose this option you must configure the JoinInfo logic (See JoinConfiguration)</para>
    /// </summary>
    public partial class ForwardEngineerCatalogueUI : Form
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly TableInfo _tableInfo;
        private readonly ColumnInfo[] _columnInfos;
        private IActivateItems _activator;
        public Catalogue CatalogueCreatedIfAny { get; private set; }

        public ForwardEngineerCatalogueUI(IActivateItems activator, TableInfo tableInfo, ColumnInfo[] columnInfos)
        {
            _activator = activator;
            _repositoryLocator = activator.RepositoryLocator;
            _tableInfo = tableInfo;
            _columnInfos = columnInfos;
            
            InitializeComponent();

            if(tableInfo == null)
                return;

            var existingFolders
                = _repositoryLocator.CatalogueRepository.GetAllCatalogues().Select(c => c.Folder)
                    .Union(new[] {CatalogueFolder.Root})//always add the root
                        .Distinct().ToArray();

            cbxFolder.Items.AddRange(existingFolders);
            cbxFolder.Text = "\\";

            //configureCatalogueExtractabilityUI1.SetUp(columnInfos, activator);
        }

        private void cbAddToExisting_CheckedChanged(object sender, EventArgs e)
        {
            cbxExistingCatalogue.Enabled = cbAddToExisting.Checked;
        }

        private void cbGenerateEntireCatalogue_CheckedChanged(object sender, EventArgs e)
        {
            if (cbGenerateEntireCatalogue.Checked)
                cbxExistingCatalogue.Enabled = false;
            else
                cbxExistingCatalogue.Enabled = true;
        }

        private void ForwardEngineerCatalogue_Load(object sender, EventArgs e)
        {
            if (_repositoryLocator == null)
                return;

            cbxExistingCatalogue.Items.Clear();
            cbxExistingCatalogue.Items.AddRange(_repositoryLocator.CatalogueRepository.GetAllCatalogues().ToArray());

        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            if (rbYesToCatalogue.Checked)
                CreateCatalogue();
            else
                this.Close();
        }

        private void CreateCatalogue()
        {

            Catalogue toPopulate;

            if (cbAddToExisting.Checked)
            {
                object selected = cbxExistingCatalogue.SelectedItem;

                if (selected == null)
                {
                    MessageBox.Show("You need to select an existing catalogue");
                    return;
                }
                toPopulate = (Catalogue)selected;
            }
            else
                if (cbGenerateEntireCatalogue.Checked)
                {
                    toPopulate = null;
                }
                else
                {
                    throw new Exception("One of the two should be checked!");
                }

            

            //if add to existing is checked and enabled (not greyed out) then also add extraction informations
            var forwardEngineer = new CatalogueLibrary.ForwardEngineerCatalogue(_tableInfo, _columnInfos);

            //the artifacts that will be created by forward engineering
            Catalogue c;
            CatalogueItem[] ci;
            ExtractionInformation[] extractionInformations;

            //create the catalogue with all it's associated artifacts
            forwardEngineer.ExecuteForwardEngineering(toPopulate, out c, out ci, out extractionInformations);
            CatalogueCreatedIfAny = c;

            try
            {
                c.Folder = new CatalogueFolder(c, cbxFolder.Text);
                c.SaveToDatabase();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Could not set folder for new Catalogue (proceeding anyway, it will be left in the root)", exception);
            }

            this.Close();
        }


        private void cbxFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckPath();
        }

        private void CheckPath()
        {
            try
            {
                cbxFolder.ForeColor = CatalogueFolder.IsValidPath(cbxFolder.Text) ? Color.Black : Color.Red;
            }
            catch (Exception)
            {
                cbxFolder.ForeColor = Color.Red;
            }
        }

        private void cbxFolder_TextChanged(object sender, EventArgs e)
        {
            CheckPath();
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            gbCreateCatalogue.Enabled = rbYesToCatalogue.Checked;

            //default option if you want to create a catalogue is to 
            cbGenerateEntireCatalogue.Checked = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
