using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline.Events;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.DataFlowPipeline.Destinations;
using RDMPObjectVisualisation;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableUIComponents;
using Message = System.ServiceModel.Channels.Message;

namespace CatalogueManager.SimpleDialogs.SimpleFileImporting
{
    /// <summary>
    /// Allows you to take data in a single data table and bulk insert it into a database (which you pick at the top of the screen).  You must select or create an appropriate pipeline.
    /// This will consist of a source that is capable of reading your file (e.g. if the file is CSV use DelimitedDataFlowSource) and zero or more middle components e.g. CleanStrings. 
    /// For destination your pipeline can have any destination that inherits from DataTableUploadDestination (this allows you to have custom plugin behaviour if you have some kind of
    ///  weird database repository).  After the pipeline has executed and your database has been populated with the data table then the ForwardEngineerCatalogue dialog will appear which 
    /// will let you create a Catalogue reference in the DataCatalogue database for the new table.  Note that this dialog should only be used for 'one off' or 'getting started' style 
    /// loads, if you plan to routinely load this data table then give it a LoadMetadata (See LoadMetadataUI).
    /// </summary>
    public partial class CreateNewCatalogueByImportingFileUI_Advanced : UserControl
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IActivateItems _activator;
        private readonly DiscoveredDatabase _database;
        private readonly bool _alsoForwardEngineerCatalogue;

        private DataFlowPipelineContext<DataTable> _context = new DataFlowPipelineContext<DataTable>();
        private FlatFileToLoad _fileToLoad;
        private Project _projectSpecific;

        public Catalogue CatalogueCreatedIfAny { get; private set; }

        public CreateNewCatalogueByImportingFileUI_Advanced(IActivateItems activator,DiscoveredDatabase database,FileInfo file, bool alsoForwardEngineerCatalogue,Project projectSpecific)
        {
            _repositoryLocator = activator.RepositoryLocator;
            _activator = activator;
            _database = database;
            _alsoForwardEngineerCatalogue = alsoForwardEngineerCatalogue;
            _projectSpecific = projectSpecific;
            _context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.LoadsSingleFlatFile);
            _context.MustHaveDestination = typeof (DataTableUploadDestination);

            InitializeComponent();
            
            _fileToLoad = new FlatFileToLoad(file);

            configureAndExecutePipeline1.AddInitializationObject(_fileToLoad);
            configureAndExecutePipeline1.AddInitializationObject(database);
            configureAndExecutePipeline1.PipelineExecutionFinishedsuccessfully += ConfigureAndExecutePipeline1OnPipelineExecutionFinishedsuccessfully;

            configureAndExecutePipeline1.SetPipelineOptions(null, null, _context, _repositoryLocator.CatalogueRepository);
        }

        
        private void ConfigureAndExecutePipeline1OnPipelineExecutionFinishedsuccessfully(object sender, PipelineEngineEventArgs args)
        {
            //pipeline executed successfully
            if (_alsoForwardEngineerCatalogue)
            {
                string targetTable = null;

                try
                {
                    var dest = (DataTableUploadDestination)args.PipelineEngine.DestinationObject;
                    targetTable = dest.TargetTableName;
                    var table = _database.ExpectTable(targetTable);

                    var ui = new ConfigureCatalogueExtractabilityUI(_activator, new TableInfoImporter(_repositoryLocator.CatalogueRepository, table), "File '" + _fileToLoad.File.FullName + "'",_projectSpecific);
                    
                    var cata = CatalogueCreatedIfAny = ui.CatalogueCreatedIfAny;

                    if (cata != null)
                        MessageBox.Show("Catalogue " + cata.Name + " successfully created");
                    else
                        MessageBox.Show("User cancelled Catalogue creation, data has been loaded and TableInfo/ColumnInfos exist in Data Catalogue but there will be no Catalogue");

                    this.ParentForm.Close();
                }
                catch (Exception e)
                {
                    ExceptionViewer.Show("Failed to import TableInfo/Forward Engineer Catalogue from " + _database.ToString() + "(Table was " + (targetTable ??"Null!")+ ")" + " - see Exception for details", e);
                }
            }
        }
    }
}
