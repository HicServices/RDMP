using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunAnalytics: BasicCommandExecution, IAtomicCommand
    {
        private readonly Catalogue _catalogue;
        private readonly int _dataLoadID;
        private readonly IBasicActivateItems _activator;

        public ExecuteCommandRunAnalytics(IBasicActivateItems activator, Catalogue catalogue, int DataLoadID ) : base(activator)
        {
            _activator = activator;
            _catalogue = catalogue;
            _dataLoadID = DataLoadID;
        }

        public override void Execute()
        {
            base.Execute();
            AnalyticsOptions options = new()
            {
                Catalogue = _catalogue.ID.ToString(),
                DataLoad_ID = _dataLoadID,
                Command = CommandLineActivity.run
            };
            var runner = RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(_activator.RepositoryLocator), options);
            runner.Run(_activator.RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                        new GracefulCancellationToken());
        }
    }
}
