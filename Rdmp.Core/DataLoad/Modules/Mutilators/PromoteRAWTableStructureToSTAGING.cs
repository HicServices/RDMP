using FAnsi.Discovery;
using Microsoft.Data.SqlClient;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.Mutilators
{
    /// <summary>
    /// Used to allow the RAW structure to be used in STAGING. Users are responsible for ensuring PKs and structure are corrected before merging into LIVE.
    /// </summary>
    public class PromoteRAWTableStructureToSTAGING : IPluginMutilateDataTables
    {

        [DemandsInitialization("Attempt to add the Primary Keys from the destination to the Staging tables", defaultValue: true)]
        public bool UseDestinationPrimaryKeys { get; set; }


        private LoadStage _loadStage;

        public void Check(ICheckNotifier notifier)
        {
            if (_loadStage != LoadStage.AdjustRaw)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"RAW Promotion can ONLY occur in load stage AdjustStaging, it is currently configured as load stage: {_loadStage}",
                    CheckResult.Fail));
            }
        }

        public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
        {
            _loadStage = loadStage;


        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
        }

        public ExitCodeType Mutilate(IDataLoadJob job)
        {
            return ExitCodeType.Success;
        }
    }
}
