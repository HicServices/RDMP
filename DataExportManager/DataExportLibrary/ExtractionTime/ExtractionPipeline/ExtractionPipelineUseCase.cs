using System;
using System.Data;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline
{
    /// <summary>
    /// Use case for linking and extracting Project Extraction Configuration datasets and custom data (See IExtractCommand).
    /// </summary>
    public class ExtractionPipelineUseCase : PipelineUseCase
    {
        private readonly IProject _project;
        private readonly IPipeline _pipeline;
        DataLoadInfo _dataLoadInfo;
        private DataFlowPipelineContext<DataTable> _context;
        
        public IExtractCommand ExtractCommand { get; set; }
        public ExecuteDatasetExtractionSource Source { get; private set; }

        public GracefulCancellationToken Token { get; set; }

        /// <summary>
        /// If Destination is an IExecuteDatasetExtractionDestination then it will be initialized properly with the configuration, cohort etc otherwise the destination will have to react properly 
        /// / dynamically based on what comes down the pipeline just like it would normally e.g. SqlBulkInsertDestination would be a logically permissable destination for an ExtractionPipeline
        /// </summary>
        public IExecuteDatasetExtractionDestination Destination { get; private set; }
      
        public ExtractionPipelineUseCase(IProject project) : this(project, ExtractDatasetCommand.EmptyCommand, null, DataLoadInfo.Empty)
        { }

        public ExtractionPipelineUseCase(IProject project, IExtractCommand extractCommand, IPipeline pipeline, DataLoadInfo dataLoadInfo)
        {
            _project = project;
            _dataLoadInfo = dataLoadInfo;
            ExtractCommand = extractCommand;
            _pipeline = pipeline;

            //create the context using the standard context factory
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            _context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo);

            //adjust context: we want a destination requirement of IExecuteDatasetExtractionDestination
            _context.MustHaveDestination = typeof(IExecuteDatasetExtractionDestination);//we want this freaky destination type
            _context.MustHaveSource = typeof(ExecuteDatasetExtractionSource);

            extractCommand.ElevateState(ExtractCommandState.NotLaunched);
        }
        
        public void Execute(IDataLoadEventListener listener)
        {
            try
            {
                ExtractCommand.ElevateState(ExtractCommandState.WaitingToExecute);
                var engine = GetEngine(_pipeline, listener);

                try
                {
                    engine.ExecutePipeline(Token?? new GracefulCancellationToken());
                    listener.OnNotify(Destination, new NotifyEventArgs(ProgressEventType.Information, "Extraction completed successfully into : " + Destination.GetDestinationDescription()));
                }
                catch (Exception e)
                {
                    ExtractCommand.ElevateState(ExtractCommandState.Crashed);
                    FatalErrorLogging.GetInstance()
                                     .LogFatalError(_dataLoadInfo, "Execute extraction pipeline", ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

                    if (ExtractCommand is ExtractDatasetCommand)
                    {
                        //audit to extraction results
                        var result = (ExtractCommand as ExtractDatasetCommand).CumulativeExtractionResults;
                        result.Exception = ExceptionHelper.ExceptionToListOfInnerMessages(e, true);
                        result.SaveToDatabase();
                    }
                    else
                    {
                        //audit to extraction results
                        var result = (ExtractCommand as ExtractGlobalsCommand).ExtractionResults;
                        foreach (var extractionResults in result)
                        {
                            extractionResults.Exception = ExceptionHelper.ExceptionToListOfInnerMessages(e, true);
                            extractionResults.SaveToDatabase();   
                        }
                    }

                    //throw so it can be audited to UI (triple audit yay!)
                    throw new Exception("An error occurred while executing pipeline", e);
                }

                if (Source == null)
                    throw new Exception("Execute Pipeline completed without Exception but Source was null somehow?!");

                if (Source.WasCancelled)
                {
                    FatalErrorLogging.GetInstance()
                        .LogFatalError(Destination.TableLoadInfo.DataLoadInfoParent, this.GetType().Name,
                            "User Cancelled Extraction");
                    ExtractCommand.ElevateState(ExtractCommandState.UserAborted);

                    if (ExtractCommand is ExtractDatasetCommand)
                    {
                        //audit to extraction results
                        var result = (ExtractCommand as ExtractDatasetCommand).CumulativeExtractionResults;
                        result.Exception = "User Cancelled Extraction";
                        result.SaveToDatabase();
                    }
                    else
                    {
                        //audit to extraction results
                        var result = (ExtractCommand as ExtractGlobalsCommand).ExtractionResults;
                        foreach (var extractionResults in result)
                        {
                            extractionResults.Exception = "User Cancelled Extraction";
                            extractionResults.SaveToDatabase();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Execute pipeline failed with Exception",ex));
                ExtractCommand.ElevateState( ExtractCommandState.Crashed);
            }

            //if it didn't crash / get aborted etc
            if (ExtractCommand.State < ExtractCommandState.WritingMetadata && ExtractCommand is ExtractDatasetCommand)
                WriteMetadata(listener);
        }

        public override IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
        {
            var engine = base.GetEngine(pipeline, listener);
            
            Destination = (IExecuteDatasetExtractionDestination)engine.DestinationObject; //record the destination that was created as part of the Pipeline configured            
            Source = (ExecuteDatasetExtractionSource)engine.SourceObject;
            
            return engine;
        }
        
        public override object[] GetInitializationObjects()
        {
            //initialize it with the extraction configuration request object and the audit object (this will initialize all objects in pipeline which implement IPipelineRequirement<ExtractionRequest> and IPipelineRequirement<TableLoadInfo>
            if (_pipeline != null)
                return new object[] { ExtractCommand, _dataLoadInfo, _project, _pipeline.Repository };

            return new object[] { ExtractCommand, _dataLoadInfo, _project };
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }



        /*private void DoExtractionAsync(IExtractCommand request)
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
                            

                    }
        }
        */
        private void WriteMetadata(IDataLoadEventListener listener)
        {
            ExtractCommand.ElevateState(ExtractCommandState.WritingMetadata);
            WordDataWriter wordDataWriter;

            
            try
            {
                wordDataWriter = new WordDataWriter(this);
                wordDataWriter.GenerateWordFile();//run the report
            }
            catch (Exception e)
            {
                //something about the pipeline resulted i a known unsupported state (e.g. extracting to a database) so we can't use WordDataWritter with this
                // tell user that we could not run the report and set the status to warning
                ExtractCommand.ElevateState(ExtractCommandState.Warning);
                
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Word metadata document NOT CREATED",e));
                return;
            }
            
            //if there were any exceptions
            if (wordDataWriter.ExceptionsGeneratingWordFile.Any())
            {
                ExtractCommand.ElevateState(ExtractCommandState.Warning);
                    
                foreach (Exception e in wordDataWriter.ExceptionsGeneratingWordFile)
                    listener.OnNotify(wordDataWriter, new NotifyEventArgs(ProgressEventType.Warning, "Word metadata document creation caused exception", e));
            }
            else
            {
                ExtractCommand.ElevateState(ExtractCommandState.Completed);
            }
        }
    }
}

