using System;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.MainFormUITabs;
using DataLoadEngine.DatabaseManagement.Operations;

namespace CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding
{
    public  delegate void MultiSelectDestinationChangeHandler(DiscardedColumnDestination chosenDestination);

    /// <summary>
    /// Lets you configure a column in a dataset as discarded during the load process (either completely - Oblivion or stored in an identifiers only area - StoreInIdentifiersDump).  For
    /// full details of how to configure a PreLoadDiscardedColumn see ConfigurePreLoadDiscardedColumns
    /// </summary>
    public partial class PreLoadDiscardedColumnUI : UserControl
    {
        private PreLoadDiscardedColumn _preLoadDiscardedColumn;
        private bool _multiSelectMode;
        public event EventHandler Saved;


        public event MultiSelectDestinationChangeHandler MutliSelectDestinationChanged;
        public bool MultiSelectMode
        {
            get { return _multiSelectMode; }
            set
            {
                _multiSelectMode = value;

                if (value)
                     PreLoadDiscardedColumn = null;
            }
        }

        public PreLoadDiscardedColumn PreLoadDiscardedColumn
        {
            get { return _preLoadDiscardedColumn; }
            set
            {
                _preLoadDiscardedColumn = value; 

                if (value == null)
                {
                    tbID.Text = "";
                    tbRuntimeColumnName.Text  = "";
                    tbRuntimeColumnName.Enabled = false;
                    
                    tbSqlDataType.Text = "";
                    tbSqlDataType.Enabled = false;

                    //disable destination change unless we are in multiselect mode - i.e. user wants to change the destination of 10 columns at once in a further up hierarchy UI component
                    if(!MultiSelectMode)
                    {
                        ddDestination.SelectedItem = null;
                        ddDestination.Enabled = false;
                    }
                    btnSave.Enabled = false;
                    
                }
                else
                {
                    if(MultiSelectMode)
                        throw new Exception("Control is in multi select mode but had it's PreLoadDiscardedColumn changed.  Make sure to turn off MultiSelect mode first");

                    tbID.Text = value.ID.ToString();
                    tbRuntimeColumnName.Text = value.RuntimeColumnName;
                    tbRuntimeColumnName.Enabled = true;

                    tbSqlDataType.Text = value.SqlDataType;
                    tbSqlDataType.Enabled = true;

                    ddDestination.SelectedItem = value.Destination;
                    ddDestination.Enabled = true;
                
                    btnSave.Enabled = false;//this gets enabled when you make a change worth saving
                }
            }
        }

        public PreLoadDiscardedColumnUI()
        {
            MultiSelectMode = false;
            InitializeComponent();
            ddDestination.DataSource = Enum.GetValues(typeof (DiscardedColumnDestination));
        }

        private void ddDestination_SelectedIndexChanged(object sender, EventArgs e)
        {
            //its not in multi select mode
            if (PreLoadDiscardedColumn != null && ddDestination.SelectedItem != null)
            {

                PreLoadDiscardedColumn.Destination = (DiscardedColumnDestination) ddDestination.SelectedItem;
                PreLoadDiscardedColumn.SaveToDatabase();
            }
            else //its in multi select mode
            if (MultiSelectMode && ddDestination.SelectedItem != null)
                MutliSelectDestinationChanged((DiscardedColumnDestination) ddDestination.SelectedItem);
        }

        private void tbRuntimeColumnName_TextChanged(object sender, EventArgs e)
        {
            if(PreLoadDiscardedColumn != null)
            {
                PreLoadDiscardedColumn.RuntimeColumnName = tbRuntimeColumnName.Text;
                btnSave.Enabled = true;
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(PreLoadDiscardedColumn != null)
            {
                PreLoadDiscardedColumn.SaveToDatabase();
                btnSave.Enabled = false;
                
                if (Saved != null)
                    Saved(this,new EventArgs());
            }
        }

        private void tbSqlDataType_TextChanged(object sender, EventArgs e)
        {
            if (PreLoadDiscardedColumn != null)
            {
                PreLoadDiscardedColumn.SqlDataType = tbSqlDataType.Text;
                btnSave.Enabled = true;

                if(!string.IsNullOrWhiteSpace(tbSqlDataType.Text))
                {
                    lblErrorInType.Visible = false;
                    try
                    {
                        SMOTypeLookup lookup = new SMOTypeLookup();
                        lookup.GetSMODataTypeForSqlStringDataType(tbSqlDataType.Text);
                    }
                    catch (Exception ex)
                    {
                        lblErrorInType.Visible = true;
                        lblErrorInType.Text = ex.Message;
                    }
                }
            }
        }
    }
}

