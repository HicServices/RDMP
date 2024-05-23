using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateVersionOfCohortConfiguration : BasicCommandExecution, IAtomicCommand
{
    CohortIdentificationConfiguration _cic;
    IBasicActivateItems _activator;

    public ExecuteCommandCreateVersionOfCohortConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
    {
        _cic = cic;
        _activator = activator;
    }


    public override void Execute()
    {
        base.Execute();
        CohortIdentificationConfiguration clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
        if (clone != null)
        {
            clone.Name = clone.Name[..^8];//strip out clone
            clone.Version = 1;
            var previousClones = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ClonedFrom_ID", _cic.ID).Where(cic => cic.Version != null);
            if (previousClones.Any())
            {
                clone.Version = previousClones.Select(pc => pc.Version).Where(v => v != null).Max() + 1;
            }
            clone.SaveToDatabase();
            Publish(clone);
        }
        else
        {
            //something has gone wrong
        }
    }
}
