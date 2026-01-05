// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Logging;
using Rdmp.Core.Reports.ExtractionTime;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline;

/// <summary>
/// Use case for linking and extracting Project Extraction Configuration datasets and custom data (See IExtractCommand).
/// </summary>
public sealed class ExtractionPipelineUseCase : PipelineUseCase
{
    private readonly IPipeline _pipeline;
    private readonly DataLoadInfo _dataLoadInfo;

    public IExtractCommand ExtractCommand { get; set; }
    public ExecuteDatasetExtractionSource Source { get; private set; }

    public GracefulCancellationToken Token { get; set; }

    /// <summary>
    /// If Destination is an IExecuteDatasetExtractionDestination then it will be initialized properly with the configuration, cohort etc otherwise the destination will have to react properly
    /// / dynamically based on what comes down the pipeline just like it would normally e.g. SqlBulkInsertDestination would be a logically permissible destination for an ExtractionPipeline
    /// </summary>
    public IExecuteDatasetExtractionDestination Destination { get; private set; }

    public ExtractionPipelineUseCase(IBasicActivateItems activator, IProject project, IExtractCommand extractCommand,
        IPipeline pipeline, DataLoadInfo dataLoadInfo)
    {
        _dataLoadInfo = dataLoadInfo;
        ExtractCommand = extractCommand;
        _pipeline = pipeline;

        extractCommand.ElevateState(ExtractCommandState.NotLaunched);

        AddInitializationObject(ExtractCommand);
        AddInitializationObject(project);
        AddInitializationObject(_dataLoadInfo);
        AddInitializationObject(project.DataExportRepository.CatalogueRepository);
        AddInitializationObject(activator);

        GenerateContext();
    }


    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        //create the context using the standard context factory
        var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
        var context = contextFactory.Create(PipelineUsage.LogsToTableLoadInfo);

        //adjust context: we want a destination requirement of IExecuteDatasetExtractionDestination
        context.MustHaveDestination =
            typeof(IExecuteDatasetExtractionDestination); //we want this freaky destination type
        context.MustHaveSource = typeof(ExecuteDatasetExtractionSource);

        return context;
    }

    public void Execute(IDataLoadEventListener listener)
    {
        if (ExtractCommand is ExtractDatasetCommand eds)
        {
            bool runAgain;
            var totalFailureCount = 0;
            var consecutiveFailureCount = 0;

            do
            {
                Token?.ThrowIfStopRequested();
                Token?.ThrowIfAbortRequested();

                bool runSuccessful;
                try
                {
                    runSuccessful = ExecuteOnce(listener);
                }
                catch (Exception)
                {
                    runSuccessful = false;
                }

                if (runSuccessful)
                {
                    runAgain = IncrementProgressIfAny(eds, listener);
                    consecutiveFailureCount = 0;

                    if (runAgain)
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information,
                                "Running pipeline again for next batch in ExtractionProgress"));
                }
                else
                {
                    totalFailureCount++;
                    consecutiveFailureCount++;

                    runAgain = ShouldRetry(eds, listener, totalFailureCount, consecutiveFailureCount);

                    if (runAgain)
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information, "Retrying pipeline"));
                }
            } while (runAgain);
        }
        else
        {
            ExecuteOnce(listener);
        }
    }

    /// <summary>
    /// Inspects the <paramref name="extractDatasetCommand"/> to see if it is a batch load that has
    /// only done part of its full execution.  If so then progress will be recorded and true will be returned
    /// (i.e. run again).
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private bool IncrementProgressIfAny(ExtractDatasetCommand extractDatasetCommand, IDataLoadEventListener listener)
    {
        var progress = extractDatasetCommand.SelectedDataSets.ExtractionProgressIfAny;

        if (progress == null)
            return false;

        // if load ended successfully and it is a batch load
        if (extractDatasetCommand.BatchEnd != null)
        {
            // update our progress
            progress.ProgressDate = extractDatasetCommand.BatchEnd.Value;
            progress.SaveToDatabase();
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    $"Saving batch extraction progress as {progress.ProgressDate}"));

            if (progress.MoreToFetch())
            {
                // clear the query builder so it can be rebuilt for the new dates
                extractDatasetCommand.Reset();
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Returns whether to retry the extraction.  This method may perform a wait operation
    /// before returning true.
    /// </summary>
    /// <param name="extractDatasetCommand"></param>
    /// <param name="listener"></param>
    /// <param name="totalFailureCount"></param>
    /// <param name="consecutiveFailureCount"></param>
    /// <returns></returns>
    private bool ShouldRetry(ExtractDatasetCommand extractDatasetCommand, IDataLoadEventListener listener,
        int totalFailureCount, int consecutiveFailureCount)
    {
        var progress = extractDatasetCommand.SelectedDataSets.ExtractionProgressIfAny;

        return progress?.ApplyRetryWaitStrategy(Token, listener, totalFailureCount, consecutiveFailureCount) == true;
    }


    /// <summary>
    /// Runs the extraction once and returns true if it was success otherwise false
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    private bool ExecuteOnce(IDataLoadEventListener listener)
    {
        try
        {
            ExtractCommand.ElevateState(ExtractCommandState.WaitingToExecute);

            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    $"Running Extraction {ExtractCommand} with Pipeline {_pipeline.Name} (ID={_pipeline.ID})"));

            var engine = GetEngine(_pipeline, listener);

            try
            {
                engine.ExecutePipeline(Token ?? new GracefulCancellationToken());
                listener.OnNotify(Destination, new NotifyEventArgs(ProgressEventType.Information,
                    $"Extraction completed successfully into : {Destination.GetDestinationDescription()}"));
            }
            catch (Exception e)
            {
                ExtractCommand.ElevateState(ExtractCommandState.Crashed);
                _dataLoadInfo.LogFatalError("Execute extraction pipeline",
                    ExceptionHelper.ExceptionToListOfInnerMessages(e, true));

                if (ExtractCommand is ExtractDatasetCommand command)
                {
                    //audit to extraction results
                    var result = command.CumulativeExtractionResults;
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
                Destination.TableLoadInfo.DataLoadInfoParent.LogFatalError(GetType().Name, "User Cancelled Extraction");
                ExtractCommand.ElevateState(ExtractCommandState.UserAborted);

                if (ExtractCommand is ExtractDatasetCommand command)
                {
                    //audit to extraction results
                    var result = command.CumulativeExtractionResults;
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
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error, "Execute pipeline failed with Exception", ex));
            ExtractCommand.ElevateState(ExtractCommandState.Crashed);
        }

        //if it didn't crash / get aborted etc
        if (ExtractCommand.State < ExtractCommandState.WritingMetadata)
        {
            if (ExtractCommand is ExtractDatasetCommand)
                WriteMetadata(listener);
            else
                ExtractCommand.ElevateState(ExtractCommandState.Completed);
        }
        else
        {
            return false; // it crashed or was aborted etc
        }

        return true;
    }

    public override IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener)
    {
        var engine = base.GetEngine(pipeline, listener);

        Destination =
            (IExecuteDatasetExtractionDestination)engine
                .DestinationObject; //record the destination that was created as part of the Pipeline configured
        Source = (ExecuteDatasetExtractionSource)engine.SourceObject;

        return engine;
    }

    private void WriteMetadata(IDataLoadEventListener listener)
    {
        ExtractCommand.ElevateState(ExtractCommandState.WritingMetadata);
        WordDataWriter wordDataWriter;


        try
        {
            wordDataWriter = new WordDataWriter(this);
            wordDataWriter.GenerateWordFile(); //run the report
        }
        catch (Exception e)
        {
            //something about the pipeline resulted i a known unsupported state (e.g. extracting to a database) so we can't use WordDataWritter with this
            // tell user that we could not run the report and set the status to warning
            ExtractCommand.ElevateState(ExtractCommandState.Warning);

            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error, "Word metadata document NOT CREATED", e));
            return;
        }
        try
        {
            var datasetVariableReportWriter = new DatasetVariableReportGenerator(this);
            datasetVariableReportWriter.GenerateDatasetVariableReport();
        }
        catch (Exception e)
        {
            ExtractCommand.ElevateState(ExtractCommandState.Warning);

            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error, "Dataset variable document NOT CREATED", e));
            return;
        }

        //if there were any exceptions
        if (wordDataWriter.ExceptionsGeneratingWordFile.Any())
        {
            ExtractCommand.ElevateState(ExtractCommandState.Warning);

            foreach (var e in wordDataWriter.ExceptionsGeneratingWordFile)
                listener.OnNotify(wordDataWriter,
                    new NotifyEventArgs(ProgressEventType.Warning, "Word metadata document creation caused exception",
                        e));
        }
        else
        {
            ExtractCommand.ElevateState(ExtractCommandState.Completed);
        }
    }

    private ExtractionPipelineUseCase()
        : base(new Type[]
        {
            typeof(IExtractCommand),
            typeof(IProject),
            typeof(DataLoadInfo),
            typeof(ICatalogueRepository),
            typeof(IBasicActivateItems)
        })
    {
        GenerateContext();
    }

    public static ExtractionPipelineUseCase DesignTime() => new();
}