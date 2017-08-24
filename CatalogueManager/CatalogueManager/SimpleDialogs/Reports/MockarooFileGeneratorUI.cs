using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataHelper;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DataFlowPipeline.Destinations;
using Diagnostics.TestData;
using Microsoft.SqlServer.Management.Smo;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.Progress;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Allows you to generate large numbers of test files with random often problematic / long / convoluted data in them.  This uses the free public website Mockaroo web service for which you
    /// have to register.  Also allows you to upload the files generated (or any other CSV files) into the RDMP database as Catalogues.  
    /// 
    /// IMPORTANT:This form is really only to be used in testing and design and to build large populated RDMP databases to asses performance of the application etc.  You don't need this if you
    /// are actually administering a safehaven / data extraction service with real data.
    /// 
    /// If you are looking for meaningful test data with shared identifiers then you should use UserExercisesUI instead (See UserExercisesUI)
    /// </summary>
    public partial class MockarooFileGeneratorUI : RDMPForm
    {
        private readonly IActivateItems _activator;
        private int _numberOfFiles;
        private int _numberOfHeadersStart;
        private int _numberOfHeadersEnd;
        private int _numberOfRecordsStart;
        private int _numberOfRecordsEnd;
        private DirectoryInfo _directoryToCreateFilesIn;
        private PipelineSelectionUI<DataTable> _pipelineSelection;
        private DataFlowPipelineContext<DataTable> _context;
        
        public MockarooFileGeneratorUI(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();

            //record initial values
            tb_TextChanged(null,null);
        }

        protected override void OnRepositoryLocatorAvailable()
        {
            base.OnRepositoryLocatorAvailable();
            
            _pipelineSelection = new PipelineSelectionUI<DataTable>(null, null, RepositoryLocator.CatalogueRepository);

            _context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
            _context.MustHaveDestination = typeof(DataTableUploadDestination);
            _pipelineSelection.Context = _context;
            
            //only select database
            serverDatabaseTableSelector1.HideTableComponents();
            
            panel1.Controls.Add(_pipelineSelection);
        }

        

        private void tb_TextChanged(object sender, EventArgs e)
        {
            try
            {
                ragSmiley1.Reset();
                
                _numberOfFiles = int.Parse(tbNumberOfFiles.Text);
                _numberOfHeadersStart = int.Parse(tbNumberOfHeadersStart.Text);
                _numberOfHeadersEnd = int.Parse(tbNumberOfHeadersEnd.Text);
                _numberOfRecordsStart = int.Parse(tbNumberOfRecordsStart.Text);
                _numberOfRecordsEnd = int.Parse(tbNumberOfRecordsEnd.Text);

                _directoryToCreateFilesIn = new DirectoryInfo(tbDirectory.Text);

                if (string.IsNullOrWhiteSpace(tbAPIKey.Text))
                    ragSmiley1.Warning(new Exception("API Key has not been set yet"));

                if(_pipelineSelection != null)
                    _pipelineSelection.InitializationObjectsForPreviewPipeline = new List<object>(new object[]
                    {
                        new FlatFileToLoad(new FileInfo("imaginary.csv")),
                        serverDatabaseTableSelector1.GetDiscoveredDatabase()
                    });

                ValidateConfiguration();
            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }
        }

        private void ValidateConfiguration()
        {
            if(!_directoryToCreateFilesIn.Exists)
                throw new Exception("Directory " + _directoryToCreateFilesIn.Name + " does not exist");

            if(_numberOfHeadersEnd< _numberOfHeadersStart)
                throw new ArgumentOutOfRangeException("min number of headers should be less than max number of headers");

            if (_numberOfRecordsEnd < _numberOfRecordsStart)
                throw new ArgumentOutOfRangeException("min number of records should be less than max number of records");

        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            var r = new Random();

            

            try
            {
                for (int i = 0; i < _numberOfFiles; i++)
                {
                    var numberOfHeadersInThisFile = r.Next(_numberOfHeadersStart, _numberOfHeadersEnd);
                    var numberOfRecordsInThisFile = r.Next(_numberOfRecordsStart, _numberOfRecordsEnd);

                
                    var file = new FileInfo(Path.Combine(_directoryToCreateFilesIn.FullName,"MockarooFile" + r.Next(100) + ".csv"));

                    var generator = new MockarooTestDataFileGenerator();
                    generator.GenerateFile(tbAPIKey.Text,numberOfRecordsInThisFile,numberOfHeadersInThisFile,file);

                    lbFilesToAdd.Items.Add(file);
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private void AttemptUpload()
        {
            var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();
            
            string currentFile = null;

            if (_pipelineSelection.Pipeline != null && db != null)
            {
                try
                {
                    foreach (var file in lbFilesToAdd.Items.Cast<FileInfo>())
                    {
                        currentFile = file.FullName;

                        var factory = new DataFlowPipelineEngineFactory<DataTable>(RepositoryLocator.CatalogueRepository.MEF, _context);

                        var execution = factory.Create(_pipelineSelection.Pipeline, progressUI1);
                        execution.Initialize(new FlatFileToLoad(file), db);

                        execution.ExecutePipeline(new GracefulCancellationToken());

                        var dest = ((DataTableUploadDestination)execution.DestinationObject);

                        var createdTable = dest.TargetTableName;


                        TableInfoImporter importer = new TableInfoImporter(RepositoryLocator.CatalogueRepository, db.Server.Name, db.GetRuntimeName(), createdTable, DatabaseType.MicrosoftSQLServer, username: serverDatabaseTableSelector1.Username, password: serverDatabaseTableSelector1.Password);

                        TableInfo t;
                        ColumnInfo[] cols;
                        importer.DoImport(out t, out cols);

                        var engineer = new ForwardEngineerCatalogue(t, cols, true);
                        Catalogue catalogue;
                        CatalogueItem[] items;
                        ExtractionInformation[] eis;

                        engineer.ExecuteForwardEngineering(out catalogue, out items, out eis);
                        var i = eis.First();
                        i.IsExtractionIdentifier = true;
                        i.SaveToDatabase();

                        _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(catalogue));
                    }

                    MessageBox.Show("Done");
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show("Failed on file '" + currentFile +"'",e);
                }
            }
        }
        private void btnJumpToUpload_Click(object sender, EventArgs e)
        {
            AttemptUpload();
        }
        
        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lbFilesToAdd_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                lbFilesToAdd.Items.Add(new FileInfo(file));
        }

        private void lbFilesToAdd_DragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
    }
}
