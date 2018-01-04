using System;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that hosts an IDataProvider.  The instance is hydrated from the users configuration (ProcessTask and ProcessTaskArguments) See
    /// RuntimeArgumentCollection
    /// </summary>
    public class DataProviderRuntimeTask : RuntimeTask, IMEFRuntimeTask
    {
        public IDataProvider Provider { get; private set; }
        public ICheckable MEFPluginClassInstance { get { return Provider; } }

        public DataProviderRuntimeTask(IProcessTask task, RuntimeArgumentCollection args, MEF mef)
            : base(task, args)
        {
            string classNameToInstantiate = task.Path;

            if (string.IsNullOrWhiteSpace(task.Path))
                throw new ArgumentException("Path is blank for ProcessTask '" + task + "' - it should be a class name of type " + typeof(IDataProvider).Name);

            Provider = mef.FactoryCreateA<IDataProvider>(classNameToInstantiate);

            try
            {
                SetPropertiesForClass(RuntimeArguments, Provider);
            }
            catch (Exception e)
            {
                throw new Exception("Error when trying to set the properties for '" + task.Name + "'", e);
            }

            Provider.Initialize(args.StageSpecificArguments.RootDir, RuntimeArguments.StageSpecificArguments.DbInfo);
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + Name + "'"));

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to fetch data using class " + Provider.GetType().FullName));

            return Provider.Fetch(job, cancellationToken);
        }

        public override bool Exists()
        {
            return true;
        }
        
        public override void Abort(IDataLoadEventListener postLoadEventListener)
        {
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            Provider.LoadCompletedSoDispose(exitCode,postLoadEventListener);
        }

        public override void Check(ICheckNotifier checker)
        {
            new MandatoryPropertyChecker(Provider).Check(checker);
            Provider.Check(checker);
        }
    }
}