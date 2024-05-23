using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateVersionOfCohortConfiguration : BasicCommandExecution, IAtomicCommand
{
    CohortIdentificationConfiguration _cic;

    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
    {
        _cic = cic;
    }


    public override void Execute()
    {
        base.Execute();
        _cic.SaveCurrentConfigurationAsNewVersion();
    }
}
