using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CatalogueManager;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.ProjectUI.DataUsers;
using DataExportManager.ProjectUI.Graphs;
using DataExportLibrary;
using DataExportLibrary.Checks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.SqlDialogs;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to view/edit a data extraction project including the extraction configurations that make it up (See ExtractionConfigurationUI).  
    /// 
    /// <para>First make sure your Project has a nice unique name that lets you rapidly identify it.  Next choose the 'Extraction Directory', this is the location where extracted data will be
    /// generated (See ExecuteExtractionUI).  Make sure that the extraction directory is accessible to every data analyst who is using the software / working on the project (e.g. it could
    /// be a shared network drive).</para>
    /// 
    /// <para>Optionally you can specify a Ticket for logging time/issues against (See TicketingSystemConfigurationUI)</para>
    /// 
    /// <para>Add a ProjectNumber, this number must be unique.  This number must match the project number of the cohorts you intend to use with the project in the Cohort Database (you only need
    /// to worry about a mismatch here if you are manually hacking your cohort database or if you change the project number halfway through its lifecycle).</para>
    ///  
    /// <para>Right clicking in the datagrid will allow you to create new Extraction Configurations for the project or edit existing ones.  An extraction configuration is a collection of 
    /// datasets linked against a cohort private identifier and released against an anonymous project specific identifier (See ExtractableCohortUI and ExtractionConfigurationUI).  Once 
    /// you have a few Extraction Configurations, they will appear in the datagrid too.</para>
    /// 
    /// <para>Selecting 'Check Project' will check all current and released extraction configurations in the project for problems (empty result sets, broken extraction SQL etc).</para>
    ///  
    /// </summary>
    public partial class ProjectUI : ProjectUI_Design, ISaveableUI
    {
        private Project _project;
        
        public Project Project
        {
            get { return _project; }
            set
            {
                //now load the UI form 
                _project = value;

                tbName.Text = value == null ? "" : value.Name ?? "";
                tbID.Text = value == null ? "" : value.ID.ToString() ?? "";
                dataGridView1.DataSource = value == null?null : LoadDatagridFor(value);
                tcMasterTicket.TicketText = value == null ? "" : value.MasterTicket;
                tbExtractionDirectory.Text = value == null ? "" : value.ExtractionDirectory;
                tbProjectNumber.Text = value == null ? "" : ""+value.ProjectNumber;
                
                btnConfigureDataUsers.Enabled = value != null;
                dataGridView1.Invalidate();

                SetCohorts();


            }
        }

        private void SetCohorts()
        {
            if(RepositoryLocator == null || _project == null || _project.ProjectNumber == null)
                return;

            var cohorts = RepositoryLocator.DataExportRepository.GetAllObjects<ExtractableCohort>()
                .Where(c => c.GetExternalData().ExternalProjectNumber == _project.ProjectNumber).ToArray();

            extractableCohortCollection1.SetupFor(cohorts);
        }

        //menu item setup
        private ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripMenuItem mi_SetDescription = new ToolStripMenuItem("Set Description");

        /// <summary>
        /// Set when the user right clicks a row, so that we can reference the row in the handlers of the ToolStripMenuItems
        /// </summary>
        int _rightClickedRowExtractionConfigurationID = -1;

        

        public ProjectUI()
        {
            InitializeComponent();
            
            mi_SetDescription.Click += new EventHandler(mi_SetDescription_Click);
            
            tcMasterTicket.Title = "Master Ticket";
            tcMasterTicket.TicketTextChanged += tcMasterTicket_TicketTextChanged;

            AssociatedCollection = RDMPCollection.DataExport;
        }

      

        public void RefreshLists()
        {
             dataGridView1.DataSource = LoadDatagridFor(Project);
        }


        public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            Project = databaseObject;
        }
        

        #region helper methods
        private void SetStringProperty(Control controlContainingValue, string property, object toSetOn)
        {
            if (toSetOn != null)
            {
                PropertyInfo target = toSetOn.GetType().GetProperty(property);
                FieldInfo targetMaxLength = toSetOn.GetType().GetField(property + "_MaxLength");


                if (target == null || targetMaxLength == null)
                    throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

                if (controlContainingValue.Text.Length > (int)targetMaxLength.GetValue(toSetOn))
                    controlContainingValue.ForeColor = Color.Red;
                else
                {
                    target.SetValue(toSetOn, controlContainingValue.Text, null);
                    controlContainingValue.ForeColor = Color.Black;
                }
            }
        }
        #endregion



        private DataTable LoadDatagridFor(Project value)
        {
            if (value == null)
                return null;

            IExtractionConfiguration[] configurations = value.ExtractionConfigurations;

            if (configurations == null || configurations.Length == 0)
                return null;

            DataTable dtToReturn = new DataTable();

            dtToReturn.Columns.Add("ID");
            dtToReturn.Columns.Add("Name");
            dtToReturn.Columns.Add("Date Created");
            dtToReturn.Columns.Add("Username");
            dtToReturn.Columns.Add("Status");
            dtToReturn.Columns.Add("Description");
            dtToReturn.Columns.Add("Separator");

            dtToReturn.Columns.Add("Cohort");

            dtToReturn.Columns.Add("RequestTicket");
            dtToReturn.Columns.Add("ReleaseTicket");
            dtToReturn.Columns.Add("ClonedFrom");


            dtToReturn.Columns.Add("Datasets");

            foreach (ExtractionConfiguration configuration in configurations)
            {
                DataRow r = dtToReturn.Rows.Add();

                r["ID"] = configuration.ID;
                r["Name"] = configuration.Name;
                r["Date Created"] = configuration.dtCreated;
                r["Username"] = configuration.Username;

                if (configuration.IsReleased)
                    r["Status"] = "Frozen (Because Released)";
                else
                    r["Status"] = "Editable";

                r["Description"] = configuration.Description;
                r["Separator"] = configuration.Separator;

                if (configuration.Cohort_ID == null)
                    r["Cohort"] = "None";
                else
                    try
                    {
                        r["Cohort"] = configuration.Cohort.ToString();
                    }
                    catch (Exception ex)
                    {
                        ExceptionViewer.Show(ex);
                        r["Cohort"] = "Error retrieving Cohort";
                    }

                r["RequestTicket"] = configuration.RequestTicket;
                r["ReleaseTicket"] = configuration.ReleaseTicket;
                r["ClonedFrom"] = configuration.ClonedFrom_ID;


                r["Datasets"] =
                    string.Join(",",configuration.GetAllExtractableDataSets().Select(ds=>ds.ToString()));

            }

            return dtToReturn;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (tbName.Text.Length == 0)
            {
                tbName.Text = "No Name";
                tbName.SelectAll();
            }

            SetStringProperty(tbName,"Name",Project);

        }


        #region Right Click Context Menu

        #region Menu Items
        
        void mi_SetDescription_Click(object sender, EventArgs e)
        {
            ExtractionConfiguration toSetDescriptionOn = RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(_rightClickedRowExtractionConfigurationID);

            if (toSetDescriptionOn.IsReleased)
                return;

            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Description", "Enter a Description for the Extraction:", ExtractionConfiguration.Description_MaxLength, toSetDescriptionOn.Description);

            dialog.ShowDialog(this);

            if (dialog.DialogResult == DialogResult.OK)
            {
                toSetDescriptionOn.Description = dialog.ResultText;
                toSetDescriptionOn.SaveToDatabase();
                RefreshLists();
            }
        }

        void mi_ChooseFileSeparator_Click(object sender, EventArgs e)
        {
            ExtractionConfiguration toSetDescriptionOn = RepositoryLocator.DataExportRepository.GetObjectByID<ExtractionConfiguration>(_rightClickedRowExtractionConfigurationID);

            if (toSetDescriptionOn.IsReleased)
                return;

            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Separator", "Choose a character(s) separator of up to " + ExtractionConfiguration.Separator_MaxLength + " characters long",
                                                     ExtractionConfiguration.Separator_MaxLength,toSetDescriptionOn.Separator);

            dialog.ShowDialog(this);

            if (dialog.DialogResult == DialogResult.OK)
            {
                toSetDescriptionOn.Separator = dialog.ResultText;
                toSetDescriptionOn.SaveToDatabase();
                RefreshLists();
            }
        }

        #endregion


        #region Menu popup/setup

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            //note that this only deals with clicking cells, to see what happens hwen user clicks in blank area of datagrid see dataGridView1_MouseClick
            if (e.RowIndex >= 0)
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    
                    menu.Items.Clear();

                   
                    _rightClickedRowExtractionConfigurationID = int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());

                    ExtractionConfiguration selectedExtractionConfiguration = RepositoryLocator.DataExportRepository.GetObjectByID <ExtractionConfiguration>(_rightClickedRowExtractionConfigurationID);
                    
                    menu.Items.Clear();
                    
                    if (!selectedExtractionConfiguration.IsReleased)
                        menu.Items.Add(mi_SetDescription);

                    menu.Show(Cursor.Position.X, Cursor.Position.Y);
                    
                }
        }

        

        #endregion


        #endregion


        private void btnShowExtractionDirectory_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbExtractionDirectory.Text))
            {
                try
                {
                    UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(new DirectoryInfo(tbExtractionDirectory.Text));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }

        }

        private void tbExtractionDirectory_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!tbExtractionDirectory.Text.StartsWith("\\") && !Directory.Exists(tbExtractionDirectory.Text))
                    tbExtractionDirectory.ForeColor = Color.Red;
                else
                {
                    Project.ExtractionDirectory = tbExtractionDirectory.Text;
                    tbExtractionDirectory.ForeColor = Color.Black;
                }
            }
            catch (Exception)
            {

                tbExtractionDirectory.ForeColor = Color.Red;
            }
            
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex == -1 || e.RowIndex == -1)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Description")
            {
                //simulate a right click by setting the ID and calling the handler directly
                _rightClickedRowExtractionConfigurationID = int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                mi_SetDescription_Click(null, null);
            }

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Separator")
            {
                //simulate a right click by setting the ID and calling the handler directly
                _rightClickedRowExtractionConfigurationID = int.Parse(dataGridView1.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                mi_ChooseFileSeparator_Click(null, null);
            }


        }

        private void tbProjectNumber_TextChanged(object sender, EventArgs e)
        {
            if (Project != null)
            {
                if (string.IsNullOrWhiteSpace(tbProjectNumber.Text))
                {
                    Project.ProjectNumber = null;
                    return;
                }

                try
                {
                    Project.ProjectNumber = int.Parse(tbProjectNumber.Text);
                    tbProjectNumber.ForeColor = Color.Black;
                    Project.SaveToDatabase();
                }
                catch (Exception )
                {
                    tbProjectNumber.ForeColor = Color.Red;
                }
            }
        }
        void tcMasterTicket_TicketTextChanged(object sender, EventArgs e)
        {
            Project.MasterTicket = tcMasterTicket.TicketText;
        }


        private void btnConfigureDataUsers_Click(object sender, EventArgs e)
        {
            DataUserManagement management = new DataUserManagement(Project);
            management.ShowDialog(this);
        }
        
        public void SwitchToCutDownUIMode()
        {
            dataGridView1.Visible = false;
            lblExtractions.Visible = false;
            this.Height = 160;
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fdlg = new FolderBrowserDialog();

            if (fdlg.ShowDialog() == DialogResult.OK)
                tbExtractionDirectory.Text = fdlg.SelectedPath;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ProjectUI_Design, UserControl>))]
    public abstract class ProjectUI_Design:RDMPSingleDatabaseObjectControl<Project>
    {
    }
}
