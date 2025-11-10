using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandUseTemplateCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommandWithTarget
    {
        //todo will want to think about how to associate this with a project
        private CohortIdentificationConfiguration _cic;
        private IBasicActivateItems _activator;
        public ExecuteCommandUseTemplateCohortIdentificationConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
        {
            _activator = activator;
            _cic = cic;
        }

        public override void Execute()
        {
            base.Execute();
            var clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
            clone.IsTemplate = false;
            clone.Name = clone.Name.Replace(" Template", "");
            clone.SaveToDatabase();
            Publish(clone);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is not CohortIdentificationConfiguration cic || !cic.IsTemplate)
            {
                throw new Exception("Provided database entity was not a CohortIdentificationConfiguration.");
            }
            _cic = target as CohortIdentificationConfiguration;
            return this;
        }
    }
}
