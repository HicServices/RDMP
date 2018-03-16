using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.ExtractionTime.Commands;
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
    /// <summary>
    /// Use case for linking and extracting Project Extraction Configuration datasets and custom data (See IExtractCommand).
    /// </summary>
    public class ExtractionPipelineUseCase:PipelineUseCase
    {
        private readonly IPipeline _pipeline;
        public IExtractCommand ExtractCommand { get; set; }
        public ExecuteDatasetExtractionSource Source { get; private set; }

        private DataFlowPipelineContext<DataTable> _context;
        
        /// <summary>
        /// If Destination is an IExecuteDatasetExtractionDestination then it will be initialized properly with the configuration, cohort etc otherwise the destination will have to react properly 
        /// / dynamically based on what comes down the pipeline just like it would normally e.g. SqlBulkInsertDestination would be a logically permissable destination for an ExtractionPipeline
        /// </summary>
        public IExecuteDatasetExtractionDestination Destination { get; private set; }
        
        DataLoadInfo _dataLoadInfo;

        public ExtractionPipelineUseCase() : this(ExtractDatasetCommand.EmptyCommand, null, DataLoadInfo.Empty)
        {
            
        }

        public ExtractionPipelineUseCase(IExtractCommand extractCommand, IPipeline pipeline, DataLoadInfo dataLoadInfo)
        {
            _dataLoadInfo = dataLoadInfo;
            ExtractCommand = extractCommand;
            _pipeline = pipeline;

            //create the context using the standard context factory
            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            _context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo);

            //adjust context: we want a destination requirement of IExecuteDatasetExtractionDestination
            _context.MustHaveDestination = typeof(IExecuteDatasetExtractionDestination);//we want this freaky destination type
            _context.MustHaveSource = typeof(ExecuteDatasetExtractionSource);
        }
        
        public bool Crashed = false;
        private CancellationTokenSource _cancelToken;

        public void Execute(IDataLoadEventListener listener)
        {
            try
            {
                var engine = GetEngine(_pipeline, listener);

                try
                {
                    _cancelToken = new CancellationTokenSource();
                    engine.ExecutePipeline(new GracefulCancellationToken(_cancelToken.Token, _cancelToken.Token));
                }
                catch (Exception e)
                {
                    if (Source.CumulativeExtractionResults != null)
                    {
                        //audit to logging architecture
                        FatalErrorLogging.GetInstance()
                            .LogFatalError(_dataLoadInfo, "Execute extraction pipeline",
                                ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

                        //audit to extraction results
                        var audit = Source.CumulativeExtractionResults;
                        audit.Exception = ExceptionHelper.ExceptionToListOfInnerMessages(e, true);
                        audit.SaveToDatabase();
                    }

                    //throw so it can be audited to UI (triple audit yay!)
                    throw new Exception("An error occurred while executing pipeline", e);
                }

                if (Source == null)
                    throw new Exception("Execute Pipeline completed without Exception but Source was null somehow" +
                                        "?!");

                //Deal with finishing off the Cumulative Extraction Results (only applies to IExtractCommand objects of type IExtractDatasetCommand)
                var successAudit = Source.CumulativeExtractionResults;

                if (successAudit == null)
                    return;

                successAudit.DestinationType = Destination.GetType().FullName;
                successAudit.DestinationDescription = Destination.GetDestinationDescription();
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

        public override IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
        {
            var engine = base.GetEngine(pipeline, listener);
            
            Destination = (IExecuteDatasetExtractionDestination)engine.DestinationObject; //record the destination that was created as part of the Pipeline configured            
            Source = (ExecuteDatasetExtractionSource)engine.SourceObject;
            
            return engine;
        }

        public void ExtractGlobalsForDestination(IProject project, ExtractionConfiguration configuration, GlobalsBundle globalsBundle,IDataLoadEventListener listener, DataLoadInfo dataLoadInfo)
        {
            
            try
            {
                //if we don't yet know the destination create an engine which populates Destination/Source as a byproduct
                if (Destination == null)
                    GetEngine(_pipeline,listener);
                
                if (Destination == null)
                    throw new Exception("There is no destination configured on Pipeline " + _pipeline + " so we cannot extract globals!");

                listener.OnNotify("ExtractGlobalsForDestination", new NotifyEventArgs(ProgressEventType.Information, "successfully created " + Destination.GetType()));
                Destination.ExtractGlobals((Project)project, configuration, globalsBundle, listener, dataLoadInfo);
            }
            catch (Exception e)
            {
                listener.OnNotify("ExtractGlobalsForDestination",new NotifyEventArgs(ProgressEventType.Error,"Fatal error occurred while trying to extract globals",e));
            }
        }

        public override object[] GetInitializationObjects()
        {
            //initialize it with the extraction configuration request object and the audit object (this will initialize all objects in pipeline which implement IPipelineRequirement<ExtractionRequest> and IPipelineRequirement<TableLoadInfo>
            if (_pipeline != null)
                return new object[] { ExtractCommand, _dataLoadInfo, _pipeline.Repository};

            return new object[] { ExtractCommand, _dataLoadInfo};
        }

        public override IDataFlowPipelineContext GetContext()
        {
            return _context;
        }
    }

}

