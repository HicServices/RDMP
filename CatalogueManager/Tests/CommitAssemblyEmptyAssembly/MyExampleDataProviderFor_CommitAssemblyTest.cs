using System;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace CommitAssemblyEmptyAssembly
{
    public class MyExampleDataProviderFor_CommitAssemblyTest : IPluginDataProvider
    {
        
        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            throw new NotImplementedException();
        }

        
        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            throw new NotImplementedException();
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public IDataProvider Clone()
        {
            throw new NotImplementedException();
        }

        public bool Validate(IHICProjectDirectory destination)
        {
            throw new NotImplementedException();
        }
    }
}