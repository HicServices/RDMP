using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Repositories;
using HIC.Logging;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline
{
    public class ExtractionPipelineHost
    {
        private readonly IPipeline _pipeline;
        private DataFlowPipelineEngine<DataTable> _engine;

        public IExtractCommand ExtractCommand { get; set; }
        public ExecuteDatasetExtractionSource Source { get; private set; }

        /// <summary>
        /// If Destination is a ExecuteDatasetExtractionDestination then it will be initialized properly with the configuration, cohort etc otherwise the destination will have to react properly 
        /// / dynamically based on what comes down the pipeline just like it would normally e.g. SqlBulkInsertDestination would be a logically permissable destination for an ExtractionPipeline
        /// </summary>
        public IExecuteDatasetExtractionDestination Destination { get; private set; }
        
        public static DataFlowPipelineContext<DataTable> Context;

        static ExtractionPipelineHost()
        {
            //create the context using the standard context factory
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            Context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo);

            //adjust context: we want a destination requirement of IExecuteDatasetExtractionDestination
            Context.MustHaveDestination = typeof(IExecuteDatasetExtractionDestination);//we want this freaky destination type

            Context.MustHaveSource = typeof (ExecuteDatasetExtractionSource);
        }

        DataLoadInfo _dataLoadInfo;
        private readonly MEF _mef;

        public ExtractionPipelineHost(IExtractCommand extractCommand,MEF mef, IPipeline pipeline, DataLoadInfo dataLoadInfo)
        {
            _dataLoadInfo = dataLoadInfo;
            _mef = mef;
            ExtractCommand = extractCommand;
            _pipeline = pipeline;
        }

        private void SetupPipeline(IDataLoadEventListener listener)
        {
           //now create the engine factory giving it the context so that it can validate the pipeline for us
            var factory = new DataFlowPipelineEngineFactory<DataTable>(_mef, Context);

            //ask the engine factory to create an engine for our pipeline (and check it against the context)
            _engine = factory.Create(_pipeline, listener) as DataFlowPipelineEngine<DataTable>;
            Destination = (IExecuteDatasetExtractionDestination) _engine.Destination; //record the destination that was created as part of the Pipeline configured            
            Source = (ExecuteDatasetExtractionSource)_engine.Source;
        }

        public bool Crashed = false;
        private CancellationTokenSource _cancelToken;

        public void Execute(IDataLoadEventListener listener)
        {
            try
            {
                if (Destination == null)
                    SetupPipeline(listener);

                //initialize it with the extraction configuration request object and the audit object (this will initialize all objects in pipeline which implement IPipelineRequirement<ExtractionRequest> and IPipelineRequirement<TableLoadInfo>
                _engine.Initialize(ExtractCommand, _dataLoadInfo);


                try
                {
                    _cancelToken = new CancellationTokenSource();
                    _engine.ExecutePipeline(new GracefulCancellationToken(_cancelToken.Token, _cancelToken.Token));
                }
                catch (Exception e)
                {
                    if (_engine.Source is ExecuteDatasetExtractionSource &&
                        ((ExecuteDatasetExtractionSource) _engine.Source).CumulativeExtractionResults != null)
                    {
                        //audit to logging architecture
                        FatalErrorLogging.GetInstance()
                            .LogFatalError(_dataLoadInfo, "Execute extraction pipeline",
                                ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

                        //audit to extraction results
                        var audit = ((ExecuteDatasetExtractionSource) _engine.Source).CumulativeExtractionResults;
                        audit.Exception = ExceptionHelper.ExceptionToListOfInnerMessages(e, true);
                        audit.SaveToDatabase();
                    }

                    //throw so it can be audited to UI (triple audit yay!)
                    throw new Exception("An error occurred while executing pipeline", e);
                }

                if (_engine.Source == null)
                    throw new Exception("Execute Pipeline completed without Exception but Source was null somehow" +
                                        "?!");

                //Deal with finishing off the Cumulative Extraction Results (only applies to IExtractCommand objects of type IExtractDatasetCommand)
                var successAudit = ((ExecuteDatasetExtractionSource) _engine.Source).CumulativeExtractionResults;

                if (successAudit == null)
                    return;

                successAudit.Filename = Destination.GetDestinationDescription();
                successAudit.DistinctReleaseIdentifiersEncountered = Source.UniqueReleaseIdentifiersEncountered.Count;
                successAudit.RecordsExtracted = Destination.TableLoadInfo.Inserts;

                if (Source.WasCancelled)
                {
                    successAudit.Exception = "User Cancelled Extraction";
                    FatalErrorLogging.GetInstance()
                        .LogFatalError(Destination.TableLoadInfo.DataLoadInfoParent, this.GetType().Name,
                            "User Cancelled Extraction");
                }

                successAudit.SaveToDatabase();
            }
            catch (Exception ex)
            {
                listener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Execute pipeline failed with Exception",ex));
                Crashed = true;
            }
        }

        public void Cancel()
        {
            if (_cancelToken == null)
                throw new Exception("Cannot cancel now because cancellation token has not been set yet, you must wait till _engine.ExecutePipeline is called");
            
            _cancelToken.Cancel();
        }

        public static void ExtractGlobalsForDestination(IProject project, ExtractionConfiguration configuration, IPipeline pipeline, GlobalsBundle globalsBundle,IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {

            
            var mef = ((DataExportRepository) project.Repository).CatalogueRepository.MEF;
            var factory = new DataFlowPipelineEngineFactory<DataTable>(mef, Context);
            

            try
            {
                var destination = factory.CreateDestinationIfExists(pipeline);
                var destinationAsExtractionDestination = destination as IExecuteDatasetExtractionDestination;

                if(destination == null)
                    throw new Exception("There is no destination configured on Pipeline " + pipeline + " so we cannot extract globals!");

                if (destinationAsExtractionDestination == null)
                    throw new Exception("Destination " + destination.GetType() + " is not a valid destination type (IExecuteDatasetExtractionDestination)");

                listener.OnNotify("ExtractGlobalsForDestination", new NotifyEventArgs(ProgressEventType.Information, "successfully created " + destination.GetType()));
                destinationAsExtractionDestination.ExtractGlobals((Project)project, configuration, globalsBundle, listener, dataLoadInfo);
            }
            catch (Exception e)
            {
                listener.OnNotify("ExtractGlobalsForDestination",new NotifyEventArgs(ProgressEventType.Error,"Fatal error occurred while trying to extract globals",e));
            }
        }
    }
}

