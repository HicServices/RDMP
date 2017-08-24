using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Attachers;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Runtime;
using DataLoadEngine.LoadProcess;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    public class PopulateRAW : CompositeDataLoadComponent
    {
        private readonly HICDatabaseConfiguration _databaseConfiguration;
        private readonly Stack<IDisposeAfterDataLoad> _toDispose = new Stack<IDisposeAfterDataLoad>();

        public PopulateRAW(List<IRuntimeTask> collection,HICDatabaseConfiguration databaseConfiguration):base(collection.Cast<IDataLoadComponent>().ToList())
        {
            _databaseConfiguration = databaseConfiguration;
            Description = "Populate RAW";
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job))
                return ExitCodeType.Error;

            // We may or may not need to create the raw database, depending on how we are getting the data
            CreateRawDatabaseIfRequired(job);

            var toReturn = base.Run(job, cancellationToken);
            
            if (toReturn == ExitCodeType.Success)
                // Verify that we have put something into the database
                VerifyExistenceOfRawData(job);

            return toReturn;
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            base.LoadCompletedSoDispose(exitCode,postLoadEventListener);

            while (_toDispose.Any())
                _toDispose.Pop().LoadCompletedSoDispose(exitCode,postLoadEventListener);
        }

        private bool MustCreateRawDatabase()
        {
            var attachingProcesses = _components.OfType<AttacherRuntimeTask>().ToArray();

            //we do not have any attaching processes ... magically the data must appear somehow in our RAW database so better create it -- maybe an executable is going to populate it or something
            if (attachingProcesses.Length == 0)
                return true;

            if (attachingProcesses.Length > 1)
            {
                // if there are multiple attachers, ensure that they all agree on whether or not they require external database creation
                var attachers = attachingProcesses.Select(runtime => (runtime).Attacher).ToList();
                var numAttachersRequiringDbCreation = attachers.Count(attacher => attacher.RequestsExternalDatabaseCreation);

                if (numAttachersRequiringDbCreation > 0 && numAttachersRequiringDbCreation < attachingProcesses.Length)
                    throw new Exception("If there are multiple attachers then they should all agree on whether they require database creation or not: " + attachers.Aggregate("", (s, attacher) => s + " " + attacher.GetType().Name + ":" + attacher.RequestsExternalDatabaseCreation));
            }

            return (attachingProcesses[0]).Attacher.RequestsExternalDatabaseCreation;
        }

        private void CreateRawDatabaseIfRequired(IDataLoadJob job)
        {
            // Ask the runtime process host if we need to create the RAW database
            if (!MustCreateRawDatabase()) return;

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Determined that we must create the RAW database tables..."));

            var cloner = new DatabaseCloner(_databaseConfiguration);
            cloner.CreateDatabaseForStage(LoadBubble.Raw);

            job.CreateTablesInStage(cloner,LoadBubble.Raw);
        }

        // Check that either Raw database exists and is populated, or that 'forLoading' is not empty
        private void VerifyExistenceOfRawData(IDataLoadJob job)
        {
            var raw = _databaseConfiguration.DeployInfo[LoadBubble.Raw];

            if (!raw.Exists())
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "The Mounting stage has not created the " + raw.GetRuntimeName() + " database."));

            var rawDbInfo = _databaseConfiguration.DeployInfo[LoadBubble.Raw];
            if (DatabaseOperations.CheckTablesAreEmptyInDatabaseOnServer(rawDbInfo))
            {
                var message = "The Mounting stage has not populated the RAW database (" + rawDbInfo + ") with any data";
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, message));
                throw new Exception(message);
                
            }
        }

    }
}