using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable.Revertable;
using Microsoft.Office.Interop.Word;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class BasicDataReleaseDestination : IPluginDataFlowComponent<ReleaseData>, IDataFlowDestination<ReleaseData>, IPipelineRequirement<Project>
    {
        [DemandsInitialization("Delete the released files from the origin location if release is succesful", DefaultValue = true)]
        public bool DeleteFilesOnSuccess { get; set; }

        [DemandsNestedInitialization()]
        public ReleaseEngineSettings ReleaseSettings { get; set; }

        public ReleaseData CurrentRelease { get; set; }
        private Project _project;

        public ReleaseData ProcessPipelineData(ReleaseData currentRelease, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            this.CurrentRelease = currentRelease;

            ReleaseEngine engine = new ReleaseEngine(_project, ReleaseSettings);

            //_project.ExtractionDirectory = String.IsNullOrWhiteSpace(OutputBaseFolder.FullName)
            //                                    ? _project.ExtractionDirectory
            //                                    : OutputBaseFolder.FullName;

            if (CurrentRelease.ReleaseState == ReleaseState.DoingPatch)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "CumulativeExtractionResults for datasets not included in the Patch will now be erased."));
                    
                int recordsDeleted = 0;

                foreach (var configuration in this.CurrentRelease.ConfigurationsForRelease.Keys)
                {
                    IExtractionConfiguration current = configuration;
                    var currentResults = configuration.CumulativeExtractionResults;
                
                    //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                    foreach (var redundantResult in currentResults.Where(r => CurrentRelease.ConfigurationsForRelease[current].All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                    {
                        redundantResult.DeleteInDatabase();
                        recordsDeleted++;
                    }
                }
                
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted " + recordsDeleted + " old CumulativeExtractionResults (That were not included in the final Patch you are preparing)"));
            }

            engine.DoRelease(CurrentRelease.ConfigurationsForRelease, CurrentRelease.EnvironmentPotential, isPatch: CurrentRelease.ReleaseState == ReleaseState.DoingPatch);
            
            return CurrentRelease;
        }

        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            if (pipelineFailureExceptionIfAny != null && CurrentRelease != null)
            {
                try
                {
                    int remnantsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in CurrentRelease.ConfigurationsForRelease.Keys)
                        foreach (ReleaseLogEntry remnant in configuration.ReleaseLogEntries)
                        {
                            remnant.DeleteInDatabase();
                            remnantsDeleted++;
                        }

                    if (remnantsDeleted > 0)
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Because release failed we are deleting ReleaseLogEntries, this resulted in " + remnantsDeleted + " deleted records, you will likely need to rextract these datasets or retrieve them from the Release directory"));
                }
                catch (Exception e1)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Error occurred when trying to clean up remnant ReleaseLogEntries", e1));
                }
            }
        }

        public void Abort(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void Check(ICheckNotifier notifier)
        {
        }

        public void PreInitialize(Project value, IDataLoadEventListener listener)
        {
            _project = value;
        }
    }
}