using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using LoadModules.Generic.LoadProgressUpdating;
using Microsoft.SqlServer.Server;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace LoadModules.GenericUIs.LoadProgressUpdating
{
    /// <summary>
    /// A LoadProgress object can be used as part of a LoadMetadata to record how far through a longitudinal loading task a load is (See LoadProgressUI).  This dialog lets you specify 
    /// how to update that LoadProgress after a succesful data load.  By default the data load engine will identify a window of days it wants to load (always in the past) e.g. 2001-01-01 to 
    /// 2001-01-29 and the load will execute with that window available to load components.  However sometimes a load component will only find part of that date range is available e.g. the
    /// dataset fetched only contains data up until 2001-01-15.  In this case the component needs to update the progress (on success of data load) to the 2001-01-15 date instead.  This 
    /// dialog lets you do that by specifying one of 4 update strategies:
    /// 
    /// <para> UseMaxRequestedDay - uses the upper limit of the load window i.e. 2001-01-29 
    ///  ExecuteScalarSQLInRAW - allows you to execute an SQL query in RAW bubble to determine the max date e.g. 'Select MAX(dtCreated) from MyTable'.  In the above example this would be 2001-01-15
    ///  ExecuteScalarSQLInLIVE - same as above except the SQL query is executed against the LIVE dataset post load (this is the super set of all existing dataset records + the records loaded in the data load)
    ///  DoNothing - The load progress is not updated, use this only if you have multiple components that share the same LoadProgress and you only want the last one to register for the progress update</para>
    /// </summary>
    [Export(typeof(ICustomUI<>))]
    public partial class DataLoadProgressUpdateInfoUI : Form, ICustomUI<DataLoadProgressUpdateInfo>
    {

        private const string WarningLIVE = "(Must return a single date value.  IMPORTANT: Since you are targetting LIVE, you MUST fully specify all table names with the correct database e.g. [MyDatabase]..[MyTable])";
        private const string WarningRAW = "(Must return a single date value.  IMPORTANT: Since you are targetting RAW, you MUST only specify table names, do not add a database qualifier e.g. [MyTable] NOT [MyLIVEDb]..[MyTable])";
        
        public Scintilla QueryEditor { get; set; }
        
        public DataLoadProgressUpdateInfoUI()
        {
            InitializeComponent();
            
            QueryEditor = new ScintillaTextEditorFactory().Create(null);

            pSQL.Controls.Add(QueryEditor);
            QueryEditor.TextChanged += QueryEditor_TextChanged;

            ddStrategy.DataSource = Enum.GetValues(typeof(DataLoadProgressUpdateStrategy));

        }

        public ICatalogueRepository CatalogueRepository { get; set; }
        public void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value, DataTable previewIfAvailable)
        {
            SetUnderlyingObjectTo((DataLoadProgressUpdateInfo)value,previewIfAvailable);
        }

        public ICustomUIDrivenClass GetGenericFinalStateOfUnderlyingObject()
        {
            return GetFinalStateOfUnderlyingObject();
        }

        public void SetUnderlyingObjectTo(DataLoadProgressUpdateInfo value, DataTable previewIfAvailable)
        {
            if (value == null)
                return;

            QueryEditor.Text  = value.ExecuteScalarSQL;
            tbTimeout.Text = value.Timeout.ToString();
            ddStrategy.SelectedItem = value.Strategy;

        }

        public DataLoadProgressUpdateInfo GetFinalStateOfUnderlyingObject()
        {
            var toReturn = new DataLoadProgressUpdateInfo();
            toReturn.ExecuteScalarSQL = QueryEditor.Text;
            toReturn.Timeout = _timeout;
            toReturn.Strategy = (DataLoadProgressUpdateStrategy) ddStrategy.SelectedItem;

            return toReturn;
        }

        private void ddStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = (DataLoadProgressUpdateStrategy) ddStrategy.SelectedItem;

            bool setvisible = selected == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInLIVE ||selected == DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW;
            pSQL.Visible = setvisible;
            tbTimeout.Visible = setvisible;
            lblTimeout.Visible = setvisible;

            switch (selected)
            {
                case DataLoadProgressUpdateStrategy.UseMaxRequestedDay:
                    lblWarning.Text = "";
                    break;
                case DataLoadProgressUpdateStrategy.DoNothing:
                    lblWarning.Text = "";
                    break;
                case DataLoadProgressUpdateStrategy.ExecuteScalarSQLInRAW:
                    lblWarning.Text = WarningRAW;
                    break;
                case DataLoadProgressUpdateStrategy.ExecuteScalarSQLInLIVE:
                    lblWarning.Text = WarningLIVE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CheckObject();
        }
        
        private bool _programaticClose;

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _programaticClose = true;
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _programaticClose = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        private void DataLoadProgressUpdateInfoUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            //use pressed Ok or Cancel 
            if (_programaticClose || CatalogueRepository == null)
                return;

            //User hit the X in the corner or Alt+F4 etc.

            var result = MessageBox.Show("Save Changes?", "Save Changes?", MessageBoxButtons.YesNoCancel);
            switch (result)
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    DialogResult = DialogResult.OK;
                    break;
                case DialogResult.No:
                    DialogResult = DialogResult.Cancel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void QueryEditor_TextChanged(object sender, EventArgs e)
        {
            CheckObject();
        }

        private void CheckObject()
        {
            var obj = GetFinalStateOfUnderlyingObject();
            checksUIIconOnly1.Check(obj);
        }

        private int _timeout;
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
