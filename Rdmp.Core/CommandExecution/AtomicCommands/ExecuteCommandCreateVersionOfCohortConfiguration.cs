using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data.Cohort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateVersionOfCohortConfiguration: BasicCommandExecution
{
    CohortIdentificationConfiguration _cic;
    IBasicActivateItems _activateItems;

    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic)
    {
        _activateItems = activator;
        _cic = cic;
    }


    public override void Execute()
    {
        base.Execute();
        _cic.SaveCurrentConfigurationAsNewVersion();
    }
}
