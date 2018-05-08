using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataHelper;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using CatalogueManager.Tutorials;
using DataLoadEngine.DataFlowPipeline.Destinations;
using LoadModules.Generic.DataFlowSources;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;
using ReusableUIComponents.TransparentHelpSystem;

namespace CatalogueManager.SimpleDialogs.SimpleFileImporting
{
    /// <summary>
    /// Allows you to import a flat file into your database with appropriate column data types based on the values read from the file.  This data table will then be referenced by an RDMP
    /// Catalogue which can be used to interact with it through RDMP.  
    /// </summary>
    public partial class CreateNewCatalogueByImportingFileUI : Form
    {
        private readonly IActivateItems _activator;
        private readonly ExecuteCommandCreateNewCatalogueByImportingFile _command;

        private FileInfo _selectedFile;
        private DataFlowPipelineContext<DataTable> _context;

        private CreateNewCatalogueByImportingFileUI_Advanced advanced;

        public HelpWorkflow HelpWorkflow { get; set; }

        public CreateNewCatalogueByImportingFileUI(IActivateItems activator, ExecuteCommandCreateNewCatalogueByImportingFile command)
        {
            _activator = activator;
            _command = command;
            InitializeComponent();

            pbFile.Image = activator.CoreIconProvider.GetImage(RDMPConcept.File);
            serverDatabaseTableSelector1.HideTableComponents();
            serverDatabaseTableSelector1.SelectionChanged += serverDatabaseTableSelector1_SelectionChanged;
            SetupState(State.SelectFile);
            
            if (command.File != null)
                SelectFile(command.File);

            pbHelp.Image = FamFamFamIcons.help;

            BuildHelpFlow();
        }

        private void BuildHelpFlow()
        {
            var tracker = new TutorialTracker(_activator);

            HelpWorkflow = new HelpWorkflow(this, _command, tracker);

            //////Normal work flow
            var root = new HelpStage(gbPickFile, "Choose the file you want to import here.\r\n" +
                                                 "\r\n" +
                                                 "Click on the red icon to disable this help.");
            var stage2 = new HelpStage(gbPickDatabase, "Select the database to use for importing data.\r\n" +
                                                       "Username and Password are optional; if not set, the connection will be attempted using your windows user");
            var stage3 = new HelpStage(gbPickPipeline, "Select the pipeline to execute in order to transfer the data from the files into the DB.\r\n" +
                                                       "If you are not sure, ask the admin which one to use or click 'Advanced' to go into the advanced pipeline UI.");
            var stage4 = new HelpStage(gbExecute, "Click Preview to peek at what data is in the selected file.\r\n" +
                                                  "Click Execute to run the process and import your file.");

            root.SetOption(">>", stage2);
            stage2.SetOption(">>", stage3);
            stage3.SetOption(">>", stage4);
            stage4.SetOption("|<<", root);
            //stage4.SetOption("next...", stage2);
            
            HelpWorkflow.RootStage = root;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Comma Separated Values|*.csv|Excel File|*.xls*|All Files (Advanced UI Only)|*.*";
            DialogResult result = ofd.ShowDialog();

            if (result == DialogResult.OK)
                SelectFile(new FileInfo(ofd.FileName));
        }

        private void SelectFile(FileInfo fileName)
        {
            _selectedFile = fileName;
            SetupState(State.FileSelected);

        }

        private void btnClearFile_Click(object sender, EventArgs e)
        {
            //if advanced is instantiated it will have a pre-clear file state
            if (advanced != null)
            {
                //toggle off advanced
                if (isAdvanced)
                    ToggleAdvanced();

                advanced = null;
            }

            SetupState(State.SelectFile);

            btnConfirmDatabase.Enabled = serverDatabaseTableSelector1.GetDiscoveredDatabase() != null;

        }

        private void SetupState(State state)
        {
            switch (state)
            {
                case State.SelectFile:
                    
                    //turn things off
                    pbFile.Visible = false;
                    lblFile.Visible = false;
                    btnClearFile.Visible = false;
                    ragSmileyFile.Visible = false;
                    ddPipeline.DataSource = null;
                    gbPickPipeline.Enabled = false;
                    gbExecute.Enabled = false;
                    gbPickDatabase.Enabled = false;
                    btnConfirmDatabase.Enabled = false;

                    btnAdvanced.Enabled = false;
                    advanced = null;

                    _selectedFile = null;

                    //turn things on
                    btnBrowse.Visible = true;
                    
                    break;
                case State.FileSelected:

                    //turn things off
                    btnBrowse.Visible = false;
                    gbExecute.Enabled = false;

                    //turn things on
                    pbFile.Visible = true;
                    btnAdvanced.Enabled = false;
                    advanced = null;
                    gbPickDatabase.Enabled = true;

                    //text of the file they selected
                    lblFile.Text = _selectedFile.Name;
                    lblFile.Left = pbFile.Right + 2;
                    lblFile.Visible = true;

                    ragSmileyFile.Visible = true;
                    ragSmileyFile.Left = lblFile.Right + 2;

                    btnClearFile.Left = ragSmileyFile.Right + 2;
                    btnClearFile.Visible = true;

                    IdentifyCompatiblePipelines();

                    IdentifyCompatibleServers();

                    break;
                case State.DatabaseSelected:
                    //turn things off

                    //turn things on
                    gbExecute.Enabled = true;
                    btnAdvanced.Enabled = true;
                    gbPickDatabase.Enabled = true; //user still might want to change his mind about targets
                    btnConfirmDatabase.Enabled = false;

                    break;
                default:
                    throw new ArgumentOutOfRangeException("state");
            }
        }

        private void IdentifyCompatibleServers()
        {
            var servers = _activator.CoreChildProvider.AllServers;

            if (servers.Length == 1)
            {
                var s = servers.Single();


                var uniqueDatabaseNames =
                    _activator.CoreChildProvider.AllTableInfos.Select(t => t.GetDatabaseRuntimeName())
                        .Distinct()
                        .ToArray();

                if (uniqueDatabaseNames.Length == 1)
                {
                    serverDatabaseTableSelector1.SetExplicitDatabase(s.ServerName, uniqueDatabaseNames[0]);
                    SetupState(State.DatabaseSelected);
                }
                else
                    serverDatabaseTableSelector1.SetExplicitServer(s.ServerName);
            }
            else if(servers.Length > 1)
            {
                serverDatabaseTableSelector1.SetDefaultServers(
                    servers.Select(s=>s.ServerName).ToArray()
                    );
            }
        }

        void serverDatabaseTableSelector1_SelectionChanged()
        {
            btnConfirmDatabase.Enabled = serverDatabaseTableSelector1.GetDiscoveredDatabase() != null;
        }

        private void IdentifyCompatiblePipelines()
        {
            gbPickPipeline.Enabled = true;
            ragSmileyFile.Reset();

            _context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
            _context.MustHaveDestination = typeof(DataTableUploadDestination);

            if (_selectedFile.Extension == ".csv")
                _context.MustHaveSource = typeof (DelimitedFlatFileDataFlowSource);

            if(_selectedFile.Extension.StartsWith(".xls"))
                _context.MustHaveSource = typeof(ExcelDataFlowSource);

            var compatiblePipelines = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>().Where(_context.IsAllowable).ToArray();

            if (compatiblePipelines.Length == 0)
            {
                ragSmileyFile.OnCheckPerformed(new CheckEventArgs("No Pipelines are compatible with the selected file",CheckResult.Fail));
                return;
            }

            ddPipeline.DataSource = compatiblePipelines;
            ddPipeline.SelectedItem = compatiblePipelines.First();
        }

        private enum State
        {
            SelectFile,
            FileSelected,
            DatabaseSelected
        }

        private void ddPipeline_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataFlowPipelineEngineFactory<DataTable> factory = GetFactory();

            var p = ddPipeline.SelectedItem as Pipeline;

            if(p == null)
                return;
            try
            {
                var source = factory.CreateSourceIfExists(p);
                ((IPipelineRequirement<FlatFileToLoad>)source).PreInitialize(new FlatFileToLoad(_selectedFile),new FromCheckNotifierToDataLoadEventListener(ragSmileyFile));
                ((ICheckable) source).Check(ragSmileyFile);
            }
            catch (Exception exception)
            {
                ragSmileyFile.Fatal(exception);
            }
        }

        private DataFlowPipelineEngineFactory<DataTable> GetFactory()
        {
            return new DataFlowPipelineEngineFactory<DataTable>(_activator.RepositoryLocator.CatalogueRepository.MEF, _context);
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            ToggleAdvanced();
        }

        private bool isAdvanced = false;

        private void ToggleAdvanced()
        {
            //flip it
            isAdvanced = !isAdvanced;
            btnAdvanced.Text = isAdvanced ? "Simple" : "Advanced";

            if (isAdvanced)
            {
                if (advanced == null)
                    advanced =
                        new CreateNewCatalogueByImportingFileUI_Advanced(
                            _activator,
                            serverDatabaseTableSelector1.GetDiscoveredDatabase(), _selectedFile, true);

                advanced.Bounds = pSimplePanel.Bounds;
                advanced.Anchor = pSimplePanel.Anchor;

                Controls.Remove(pSimplePanel);
                Controls.Add(advanced);
            }
            else
            {

                pSimplePanel.Bounds = advanced.Bounds;
                pSimplePanel.Anchor = advanced.Anchor;

                Controls.Remove(advanced);
                Controls.Add(pSimplePanel);
            }
        }

        private void btnConfirmDatabase_Click(object sender, EventArgs e)
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

            if (db == null)
                MessageBox.Show("You must select a Database");
            else
            if(db.Exists())
                SetupState(State.DatabaseSelected);
            else
            {
                if(MessageBox.Show("Create Database '" + db.GetRuntimeName() +"'","Create Database",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.Server.CreateDatabase(db.GetRuntimeName());
                    SetupState(State.DatabaseSelected);
                }
            }
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var p = ddPipeline.SelectedItem as Pipeline;

            if (p == null)
            {
                MessageBox.Show("No Pipeline Selected");
                return;
            }

            var source = GetFactory().CreateSourceIfExists(p);
            ((IPipelineRequirement<FlatFileToLoad>)source).PreInitialize(new FlatFileToLoad(_selectedFile), new FromCheckNotifierToDataLoadEventListener(ragSmileyFile));
            
            Cursor.Current = Cursors.WaitCursor;
            var preview = source.TryGetPreview();
            Cursor.Current = Cursors.Default;

            if(preview != null)
            {
                DataTableViewer dtv = new DataTableViewer(preview,"Preview");
                SingleControlForm.ShowDialog(dtv);
            }
        }


        private void btnExecute_Click(object sender, EventArgs e)
        {
            var p = ddPipeline.SelectedItem as Pipeline;

            if (p == null)
            {
                MessageBox.Show("No Pipeline Selected");
                return;
            }

            ragSmileyExecute.Reset();
            try
            {
                var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();
                var engine = GetFactory().Create(p, new FromCheckNotifierToDataLoadEventListener(ragSmileyExecute));
                engine.Initialize(new FlatFileToLoad(_selectedFile), db);

                Cursor.Current = Cursors.WaitCursor;

                engine.ExecutePipeline(new GracefulCancellationToken());

                var dest = (DataTableUploadDestination) engine.DestinationObject;

                Cursor.Current = Cursors.Default;

                ForwardEngineer(db.ExpectTable(dest.TargetTableName));


            }
            catch (Exception exception)
            {
                ragSmileyExecute.Fatal(exception);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ForwardEngineer(DiscoveredTable targetTableName)
        {
            var extractionPicker = new ConfigureCatalogueExtractabilityUI(_activator,new TableInfoImporter(_activator.RepositoryLocator.CatalogueRepository, targetTableName),"File '"+ _selectedFile.FullName + "'");
            extractionPicker.ShowDialog();

            var catalogue = extractionPicker.CatalogueCreatedIfAny;
            if (catalogue != null)
            {
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(catalogue));
            
                MessageBox.Show("Successfully imported new Dataset '" + catalogue + "'." +
                                "\r\n" +
                                "The edit functionality will now open.");

                _activator.WindowArranger.SetupEditCatalogue(this, catalogue);
                
            }
            if (cbAutoClose.Checked)
                this.Close();
            else
                MessageBox.Show("Creation completed successfully, close the Form when you are finished reviewing the output");
        }

        private void pbHelp_Click(object sender, EventArgs e)
        {
            HelpWorkflow.Start(force: true);
        }
    }
}
