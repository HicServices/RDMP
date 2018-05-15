using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.ExtractionTime.UserPicks;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ThreadState = System.Threading.ThreadState;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Reports the progress of a single dataset bundle in a project extraction.  A dataset bundle is the dataset, any accompanying lookups / supporting documents etc.  You will see one
    /// control per dataset you ticked in ChooseExtractablesUI plus a custom one called Globals which is for all global attachments (See SupportingDocumentUI).
    /// 
    /// <para>The notifications will include the extraction SQL sent to the data repository (including the join against the cohort and any extraction specific filters - See ConfigureDatasetUI).
    /// Then depending on the pipeline you selected in ExecuteExtractionUI you will then see messages about the data being extracted including the number of records written/fetched and
    /// any problems encountered.  These messages are also stored in the Logging database (See LogViewerForm).</para>
    ///  
    /// <para>The pipeline destination component in the pipeline you selected in ExecuteExtractionUI will determine the exact file types of the destination.  The RDMP ships with two destinations,
    /// one produces a CSV file and word metadata document, the other extracts records into a new SQL Server database which can be detached to ship the data in .mdb format (useful for extracts
    /// that are too large for traditional flat file adapters in programs like SPSS and STATA).</para>
    /// </summary>
    public partial class ExecuteDatasetExtractionHostUI : RDMPUserControl
    {
        private readonly DataLoadInfo _dataLoadInfo;
        private readonly IPipeline _pipeline;

        public IExtractCommand ExtractCommand { get; set; }

        private ExtractionPipelineUseCase _pipelineUseCase;
        public Project Project { get; set; }

        public static Semaphore NumberBuildingQueries = new Semaphore(5,5);
        Thread _extractorThread;
        public event Action Finished;

        public ExecuteDatasetExtractionHostUI(IExtractCommand extractCommand,Project project, DataLoadInfo dataLoadInfo, IPipeline pipeline )
        {
            ExtractCommand = extractCommand;

            _dataLoadInfo = dataLoadInfo;
            _pipeline = pipeline;
            
            Project = project;
            InitializeComponent();

            if (extractCommand == null)
                return;

            lblDataset.Text = "Dataset: " + extractCommand;
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_pipelineUseCase != null)
            {   
                //display it on the screen
                progressUI1.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Attempting to Cancel"));
                
                try
                {
                    _pipelineUseCase.Cancel();
                }
                catch (Exception exception)
                {
                    progressUI1.OnNotify(_pipelineUseCase,new NotifyEventArgs(ProgressEventType.Error,exception.Message,exception));
                }
            }
        
        }
        
        public void DoExtraction()
        {
            _extractorThread = new Thread(DoExtractionAsync);
            _extractorThread.Name = ExtractCommand + "(Host)";
            _extractorThread.Start();
        }

        private void DoExtractionAsync()
        {
            try
            {
                //Waits on Semaphore
                WaitForExecutionOpportunity(ExtractCommand);

                progressUI1.ShowRunning(true);

                //var extractionRequest = (ExtractDatasetCommand)ExtractCommand;

                DoExtractionAsync(ExtractCommand);
            }
            catch (Exception e)
            {
                ExtractCommand.State = ExtractCommandState.Crashed;
                progressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Extraction crashed catastrophically while trying to process ExtractCommand '" + ExtractCommand + "'", e));
            }
            finally
            {
                //Always release the Semaphore
                NumberBuildingQueries.Release();

                progressUI1.ShowRunning(false);
                Finished();
            }
        }

        
        private void DoExtractionAsync(IExtractCommand request)
        {
            request.State = ExtractCommandState.WaitingForSQLServer;
                
            _pipelineUseCase = new ExtractionPipelineUseCase(Project, request, _pipeline, _dataLoadInfo);
            _pipelineUseCase.Execute(progressUI1);

            if (_pipelineUseCase.Crashed)
            {
                request.State = ExtractCommandState.Crashed;
            }
            else
                if (_pipelineUseCase.Source != null)
                    if (_pipelineUseCase.Source.WasCancelled)
                        request.State = ExtractCommandState.UserAborted;
                    else if (_pipelineUseCase.Source.ValidationFailureException != null)
                        request.State = ExtractCommandState.Warning;
                    else
                    {
                        request.State = ExtractCommandState.Completed;
                            
                        progressUI1.OnNotify(_pipelineUseCase.Destination,
                            new NotifyEventArgs(ProgressEventType.Information,
                                "Extraction completed successfully into : " +
                                _pipelineUseCase.Destination.GetDestinationDescription()));

                        if (request is ExtractDatasetCommand)
                            WriteMetadata(request);
                        // TODO: add metadata for Globals?
                    }
        }

        private void WriteMetadata(IExtractCommand request)
        {
            request.State = ExtractCommandState.WritingMetadata;
            WordDataWriter wordDataWriter;

            try
            {
                wordDataWriter = new WordDataWriter(_pipelineUseCase);
            }
            catch (NotSupportedException e)
            {
                //something about the pipeline resulted i a known unsupported state (e.g. extracting to a database) so we can't use WordDataWritter with this
                // tell user that we could not run the report and set the status to warning
                request.State = ExtractCommandState.Warning;
                
                progressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Word metadata document NOT CREATED because of NotSupportedException",e));
                return;
            }

            wordDataWriter.GenerateWordFile();//run the report

            //if there were any exceptions
            if (wordDataWriter.ExceptionsGeneratingWordFile.Any())
            {
                request.State = ExtractCommandState.Warning;
                    
                foreach (Exception e in wordDataWriter.ExceptionsGeneratingWordFile)
                    progressUI1.OnNotify(wordDataWriter, new NotifyEventArgs(ProgressEventType.Warning, "Word metadata document creation caused exception", e));
            }
            else
            {
                //word data extracted ok
                request.State = ExtractCommandState.Completed;
            }
        }

        private void WaitForExecutionOpportunity(IExtractCommand request)
        {
           request.State = ExtractCommandState.WaitingToExecute;
            Thread.Sleep(500);
            NumberBuildingQueries.WaitOne(new TimeSpan(2, 0, 0, 0));//wait up to 2 days to kick off - only have 5 at once (see semaphore declaration which should be X,X where X is the number to run at any time
        }

        public void Cancel()
        {
            btnCancel_Click(null,null);
        }

        private void btnWhereIsPipe_Click(object sender, EventArgs e)
        {
            if(_extractorThread != null)
                if (_extractorThread.ThreadState == ThreadState.Stopped)
                    WideMessageBox.Show("Extractor Thread state is Stopped");
                else
                {
                    var stack = UsefulStuff.GetStackTrace(_extractorThread);
                    WideMessageBox.Show("Press view stack to see what is going on",environmentDotStackTrace: stack.ToString());
                }
        }
    }
}
