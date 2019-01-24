using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.ImportExport.Exceptions;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using FAnsi.Discovery;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider
{
    /// <summary>
    /// Data Provider Process Task for DLE which will look for *.sd files and import them into RDMP
    /// </summary>
    public class ShareDefinitionImporter: IPluginDataProvider
    {
        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            
        }

        public void Check(ICheckNotifier notifier)
        {
            
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            int imported = 0;
            try
            {
                var shareManager = new ShareManager(job.RepositoryLocator);

                foreach (var shareDefinitionFile in job.HICProjectDirectory.ForLoading.EnumerateFiles("*.sd"))
                {
                    job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Found '" + shareDefinitionFile.Name + "'"));
                    using (var stream = File.Open(shareDefinitionFile.FullName, FileMode.Open))
                        shareManager.ImportSharedObject(stream);

                    imported++;
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Imported '" + shareDefinitionFile.Name + "' Succesfully"));
                }
            }
            catch (SharingException ex)
            {
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Error occured importing ShareDefinitions",ex));
            }

            job.OnNotify(this, new NotifyEventArgs(imported == 0 ? ProgressEventType.Warning : ProgressEventType.Information, "Imported " + imported + " ShareDefinition files"));

            return ExitCodeType.Success;
        }
    }
}
