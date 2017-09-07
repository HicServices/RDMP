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
        [DemandsInitialization("Delete the released files from the origin location if release is succesful",defaultValue:true)]
        public bool DeleteFilesOnSuccess { get; set; }

        [DemandsInitialization("Output folder")]
        public string OutputBaseFolder { get; set; }

        private readonly List<FileInfo> _processedFiles = new List<FileInfo>();
        private Project _project;

        public ReleaseData ProcessPipelineData(ReleaseData currentRelease, IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            try
            {
                ReleaseEngine engine = new ReleaseEngine(_project);

                _project.ExtractionDirectory = String.IsNullOrWhiteSpace(OutputBaseFolder)
                                                    ? _project.ExtractionDirectory
                                                    : OutputBaseFolder;

                if (currentRelease.ReleaseState == ReleaseState.DoingPatch)
                {
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "CumulativeExtractionResults for datasets not included in the Patch will now be erased."));
                    
                    int recordsDeleted = 0;
                
                    foreach (var configuration in currentRelease.ConfigurationsForRelease.Keys)
                    {
                        IExtractionConfiguration current = configuration;
                        var currentResults = configuration.CumulativeExtractionResults;
                
                        //foreach existing CumulativeExtractionResults if it is not included in the patch then it should be deleted
                        foreach (var redundantResult in currentResults.Where(r => currentRelease.ConfigurationsForRelease[current].All(rp => rp.DataSet.ID != r.ExtractableDataSet_ID)))
                        {
                            redundantResult.DeleteInDatabase();
                            recordsDeleted++;
                        }
                    }
                
                    listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleted " + recordsDeleted + " old CumulativeExtractionResults (That were not included in the final Patch you are preparing)"));
                }

                engine.DoRelease(currentRelease.ConfigurationsForRelease, currentRelease.EnvironmentPotential, isPatch: currentRelease.ReleaseState == ReleaseState.DoingPatch);
            }
            catch (Exception exception)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to start the engine", exception));
                
                try
                {
                    int remnantsDeleted = 0;

                    foreach (ExtractionConfiguration configuration in currentRelease.ConfigurationsForRelease.Keys)
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
            return currentRelease;
        }
        
        public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
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