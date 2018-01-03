using System;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Checks.Checkers
{
    /// <summary>
    /// Checks all ProcessTasks that the user has configured for a given data load (See LoadMetadata).  This involves both constructing and initializing 
    /// the instances (which can fail if Type names don't resolve etc) and calling check on the instantiated ProcessTask.
    /// </summary>
    public class ProcessTaskChecks : ICheckable
    {
        private readonly ILoadMetadata _loadMetadata;
        LoadArgsDictionary dictionary;

        public ProcessTaskChecks(ILoadMetadata loadMetadata)
        {
            _loadMetadata = loadMetadata;
        }

        public void Check(ProcessTask processTask, ICheckNotifier notifier)
        {
            if (dictionary == null)
            {
                try
                {
                    dictionary = new LoadArgsDictionary(_loadMetadata, new HICDatabaseConfiguration(_loadMetadata).DeployInfo, false);
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(
                        new CheckEventArgs("Could not assemble LoadArgsDictionary, see inner exception for specifics",
                            CheckResult.Fail, e));
                    return;
                }
            }


            var factory = new RuntimeTaskFactory((CatalogueRepository) _loadMetadata.Repository);
            var created = factory.Create(processTask, dictionary.LoadArgs[processTask.LoadStage]);

            created.Check(notifier);
        }

        public void Check(ICheckNotifier notifier)
        {
            foreach (ProcessTask processTask in _loadMetadata.GetAllProcessTasks(false))
                Check(processTask, notifier);
        }
    }
}