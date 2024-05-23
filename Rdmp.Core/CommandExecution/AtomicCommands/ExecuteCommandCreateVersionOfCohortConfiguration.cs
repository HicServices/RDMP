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

public class ExecuteCommandCreateVersionOfCohortConfiguration : BasicCommandExecution, IAtomicCommandWithTarget
{
    CohortIdentificationConfiguration _cic;
    IBasicActivateItems _activateItems;

    [UseWithObjectConstructor]
    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
    {
        _activateItems = activator;
        _cic = cic;
    }


    public override void Execute()
    {
        base.Execute();
        _cic.SaveCurrentConfigurationAsNewVersion();
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _cic = (CohortIdentificationConfiguration)target;
        return this;
    }
}
