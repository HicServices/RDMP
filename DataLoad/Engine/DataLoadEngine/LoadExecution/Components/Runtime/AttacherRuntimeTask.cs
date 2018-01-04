using System;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    /// <summary>
    /// RuntimeTask that hosts an IAttacher.  The instance is hydrated from the users configuration (ProcessTask and ProcessTaskArguments) See
    /// RuntimeArgumentCollection
    /// </summary>
    public class AttacherRuntimeTask : RuntimeTask, IMEFRuntimeTask
    {
        public IAttacher Attacher { get; private set; }
        public ICheckable MEFPluginClassInstance { get { return Attacher; }}

        public AttacherRuntimeTask(IProcessTask task, RuntimeArgumentCollection args, MEF mef)
            : base(task, args)
        {
            //All attachers must be marked as mounting stages, and therefore we can pull out the RAW Server and Name 
            var mountingStageArgs = args.StageSpecificArguments ;
            if (mountingStageArgs.LoadStage != LoadStage.Mounting)
                throw new Exception("AttacherRuntimeTask can only be called as a Mounting stage process");
            
            if (string.IsNullOrWhiteSpace(task.Path))
                throw new ArgumentException("Path is blank for ProcessTask '" + task + "' - it should be a class name of type " + typeof(IAttacher).Name);

            Attacher = mef.FactoryCreateA<IAttacher>(ProcessTask.Path);
            SetPropertiesForClass(RuntimeArguments, Attacher);
            Attacher.Initialize(args.StageSpecificArguments.RootDir, RuntimeArguments.StageSpecificArguments.DbInfo);
        }


        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            
            var stringBefore = RuntimeArguments.StageSpecificArguments.DbInfo.Server.Builder.ConnectionString;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + ProcessTask.Name + "'"));
            job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Attacher class is:" + Attacher.GetType().FullName));

            try
            {
                return Attacher.Attach(job);
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Attach failed on job " + job + " Attacher was of type " + Attacher.GetType().Name + " see InnerException for specifics",e));
                return ExitCodeType.Error;
            }
            finally
            {
                string stringAfter = RuntimeArguments.StageSpecificArguments.DbInfo.Server.Builder.ConnectionString;

                if(!stringBefore.Equals(stringAfter))
                    job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Attacher " + Attacher.GetType().Name + " modified the ConnectionString during attaching"));
                
            }
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            Attacher.LoadCompletedSoDispose(exitCode,postLoadEventListener);
        }


        public override bool Exists()
        {
            var className = ProcessTask.Path;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == className);
                if (type != null) return true;
            }

            return false;
        }

        public override void Abort(IDataLoadEventListener postLoadEventListener)
        {
            LoadCompletedSoDispose(ExitCodeType.Abort,postLoadEventListener);
        }

        public override void Check(ICheckNotifier checker)
        {
            new MandatoryPropertyChecker(Attacher).Check(checker);
            Attacher.Check(checker);
        }
    }
}