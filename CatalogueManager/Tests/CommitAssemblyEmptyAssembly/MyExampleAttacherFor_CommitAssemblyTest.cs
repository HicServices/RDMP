using System;
using System.Collections.Generic;
using System.Text;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine;
using DataLoadEngine.Attachers;
using DataLoadEngine.Job;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace CommitAssemblyEmptyAssembly
{
    public class MyExampleAttacherFor_CommitAssemblyTest : IPluginAttacher
    {
        
        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            throw new NotImplementedException();
        }

        public ExitCodeType Attach(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            
        }

        

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }


        public string DatabaseServer { get; private set; }
        public string DatabaseName { get; private set; }
        public IHICProjectDirectory HICProjectDirectory { get; set; }
        public bool RequestsExternalDatabaseCreation { get; private set; }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            throw new NotImplementedException();
        }
    }

}
