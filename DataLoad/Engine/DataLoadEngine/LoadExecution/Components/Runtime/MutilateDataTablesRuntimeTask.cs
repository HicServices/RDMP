using System;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.Mutilators;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Runtime
{
    public class MutilateDataTablesRuntimeTask : RuntimeTask, IMEFRuntimeTask
    {
        //the class (built by reflection) that will do all the heavy lifting
        public IMutilateDataTables MutilateDataTables { get; set; }
        public ICheckable MEFPluginClassInstance { get { return MutilateDataTables; } }

        public MutilateDataTablesRuntimeTask(IProcessTask task, RuntimeArgumentCollection args, MEF mef)
            : base(task, args)
        {
            //All attachers must be marked as mounting stages, and therefore we can pull out the RAW Server and Name 
            var stageArgs = args.StageSpecificArguments;

            if (stageArgs == null)
                throw new NullReferenceException("Stage args was null");
            if(stageArgs.DbInfo == null)
                throw new NullReferenceException("Stage args had no DbInfo, unable to mutilate tables without a database - mutilator is sad");

            if(string.IsNullOrWhiteSpace(task.Path))
                throw new ArgumentException("Path is blank for ProcessTask '"+task+"' - it should be a class name of type " + typeof(IMutilateDataTables).Name);

            MutilateDataTables = mef.FactoryCreateA<IMutilateDataTables>(Path);
            SetPropertiesForClass(RuntimeArguments, MutilateDataTables);
            MutilateDataTables.Initialize(stageArgs.DbInfo, LoadStage);
        }



        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to run Task '" + Name + "'"));
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Mutilate class is" + MutilateDataTables.GetType().FullName));

            try
            {
                return MutilateDataTables.Mutilate(job);
            }
            catch (Exception e)
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Mutilate failed on job " + job + " Mutilator was of type " + MutilateDataTables.GetType().Name + " see InnerException for specifics", e));
                return ExitCodeType.Error;
            }

        }

        public override bool Exists()
        {
            return true;
        }
        
        public override void Abort(IDataLoadEventListener postLoadEventListener)
        {
            LoadCompletedSoDispose(ExitCodeType.Abort, postLoadEventListener);
        }

        public override void Check(ICheckNotifier checker)
        {
            new MandatoryPropertyChecker(MutilateDataTables).Check(checker);
            MutilateDataTables.Check(checker);
        }


        public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postDataLoadEventListener)
        {
            MutilateDataTables.LoadCompletedSoDispose(exitCode, postDataLoadEventListener);
        }

        
    }
}