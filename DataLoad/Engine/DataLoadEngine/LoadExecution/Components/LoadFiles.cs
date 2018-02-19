using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Runtime;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    /// <summary>
    /// DLE component responsible for the LoadStage.GetFiles.  This includes running any user configured ProcessTask (which will be RuntimeTasks in _components).
    /// Also pushes DeleteForLoadingFilesOperation onto the disposal stack so that any files accumulated in ForLoading are cleared at the end of the DLE run (After
    /// archiving).
    /// </summary>
    public class LoadFiles : CompositeDataLoadComponent
    {
        public LoadFiles(List<IRuntimeTask> collection):base(collection.Cast<IDataLoadComponent>().ToList())
        {
            Description = Description = "LoadFiles";
        }
        
        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job)) 
                return ExitCodeType.Error;
            
            ExitCodeType toReturn = ExitCodeType.Success; //This default will be returned unless there is an explicit DataProvider or collection of runtime tasks to run which return a different result (See below)

            // Figure out where we are getting the source files from
            try
            {
                if (Components.Any())
                {
                   toReturn = base.Run(job, cancellationToken);
                }
                else if (job.HICProjectDirectory.ForLoading.EnumerateFileSystemInfos().Any())
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Using existing files in '" + job.HICProjectDirectory.ForLoading.FullName + "', there are no GetFiles processes or DataProviders configured"));
                else
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "There are no GetFiles tasks and there are no files in the ForLoading directory (" + job.HICProjectDirectory.ForLoading.FullName + ")"));
            }
            finally
            {
                // We can only clean up ForLoading after the job is finished, so give it the necessary disposal operation
                job.PushForDisposal(new DeleteForLoadingFilesOperation(job));
                
            }

            return toReturn;
        }

        public void DeleteAllFilesInForLoading(HICProjectDirectory hicProjectDirectory, DataLoadJob job)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleting files in ForLoading (" + hicProjectDirectory.ForLoading.FullName + ")"));
            hicProjectDirectory.ForLoading.EnumerateFiles().ToList().ForEach(info => info.Delete());
            hicProjectDirectory.ForLoading.EnumerateDirectories().ToList().ForEach(info => info.Delete(true));
        }
    }
}