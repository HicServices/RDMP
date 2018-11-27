using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.AutoComplete;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.LoadExecution.Components;
using CatalogueManager.Copying;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{

    /// <summary>
    /// Lets you view/edit a single SQL file execution load task.  This SQL script will be run at the appropriate time in the data load (depending on which stage it is at and the order in the
    /// stage.  The SQL will be executed on the database/server that corresponds to the stage.  So an Adjust RAW script cannot modify STAGING since those tables won't even exist at the time of
    /// execution and might even be on a different server.  
    /// 
    /// <para>You should avoid modifying Live tables directly with SQL since it circumvents the 'no duplication', 'RAW->STAGING->LIVE super transaction' model of RDMP.</para>
    /// </summary>
    public partial class SqlProcessTaskUI : SqlProcessTaskUI_Design, ISaveableUI
    {
        private Scintilla _scintilla;
        private ProcessTask _processTask;
        private AutoCompleteProvider _autoComplete;

        public SqlProcessTaskUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataLoad;
        }

        public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _processTask = databaseObject;
            
            objectSaverButton1.SetupFor(_processTask, activator.RefreshBus);
            
            LoadFile();
            
            loadStageIconUI1.Setup(activator.CoreIconProvider, _processTask.LoadStage);
            loadStageIconUI1.Left = tbID.Right +2;
        }

        private bool _bLoading = false;

        private void LoadFile()
        {
            _bLoading = true;
            try
            {
                lblPath.Text = _processTask.Path;
                ragSmiley1.Left = lblPath.Right;
                btnBrowse.Left = ragSmiley1.Right;

                lblID.Left = btnBrowse.Right;
                tbID.Left = lblID.Right;

                pbFile.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.File);
                tbID.Text = _processTask.ID.ToString();
            

                if (_scintilla == null)
                {
                    ScintillaTextEditorFactory factory = new ScintillaTextEditorFactory();
                    _scintilla = factory.Create(new RDMPCommandFactory());
                    groupBox1.Controls.Add(_scintilla);
                    _scintilla.SavePointLeft += ScintillaOnSavePointLeft;
                    objectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;    
                }
            
                SetupAutocomplete();
                
                ragSmiley1.Reset();

                try
                {
                    _scintilla.Text = File.ReadAllText(_processTask.Path);
                    _scintilla.SetSavePoint();
                }
                catch (Exception e)
                {
                    ragSmiley1.Fatal(e);
                }

                if(!ragSmiley1.IsFatal())
                    ragSmiley1.StartChecking(_processTask);
            }
            finally
            {
                _bLoading = false;
            }
        }

        private void SetupAutocomplete()
        {
            //if theres an old one dispose it
            if (_autoComplete == null)
                _autoComplete = new AutoCompleteProviderFactory(_activator).Create(_processTask.LoadMetadata.GetQuerySyntaxHelper());
            else
                _autoComplete.Clear();

            foreach (var table in _processTask.LoadMetadata.GetDistinctTableInfoList(false))
                _autoComplete.Add(table, _processTask.LoadStage);

            _autoComplete.RegisterForEvents(_scintilla);
        }

        bool objectSaverButton1_BeforeSave(CatalogueLibrary.Data.DatabaseEntity arg)
        {
            File.WriteAllText(_processTask.Path,_scintilla.Text);
            _scintilla.SetSavePoint();
            
            return true;
        }

        private void ScintillaOnSavePointLeft(object sender, EventArgs eventArgs)
        {
            if (_bLoading)
                return;

            objectSaverButton1.Enable(true);
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Sql Files|*.sql";
            ofd.CheckFileExists = true;

            string oldFileName = null;
            //open the browse dialog at the location of the currently specified file
            if(!string.IsNullOrWhiteSpace(_processTask.Path))
            {
                var fi = new FileInfo(_processTask.Path);
                oldFileName = fi.Name;

                if (fi.Exists && fi.Directory != null)
                    ofd.InitialDirectory = fi.Directory.FullName;
            }

            if (ofd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {

                //replace the old name with the new name for example if user specified task name is 'Run bob.sql to rename all 'Roberts' to 'Bob' then the user selects a different file e.g. "truncateAllTables.sql" then the new name becomes Run truncateAllTables.sql to rename all 'Roberts' to 'Bob'
                if(oldFileName != null)
                    _processTask.Name = _processTask.Name.Replace(oldFileName,Path.GetFileName(ofd.FileName));

                _processTask.Path = ofd.FileName;
                _processTask.SaveToDatabase();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_processTask));
                LoadFile();
            }
        }
    }
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SqlProcessTaskUI_Design, UserControl>))]
    public abstract class SqlProcessTaskUI_Design : RDMPSingleDatabaseObjectControl<ProcessTask>
    {
        
    }
}
