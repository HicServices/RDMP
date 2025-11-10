using NPOI.OpenXmlFormats.Encryption;
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
    public class ExecuteCommandCreateCohortIdentificationConfigurationTemplate: BasicCommandExecution,IAtomicCommandWithTarget
    {
        private CohortIdentificationConfiguration _cic;
        private IBasicActivateItems _activator;
        public ExecuteCommandCreateCohortIdentificationConfigurationTemplate(IBasicActivateItems activator, CohortIdentificationConfiguration cic): base(activator)
        {
            _activator = activator;
            _cic = cic;
        }

        public override void Execute()
        {
            base.Execute();
            var clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
            clone.IsTemplate = true;
            clone.Name = clone.Name.Replace("(Clone)", "") + " Template";
            clone.SaveToDatabase();
            Publish(clone);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if(target is not CohortIdentificationConfiguration)
            {
                throw new Exception("Provided database entity was not a CohortIdentificationConfiguration.");
            }
            _cic = target as CohortIdentificationConfiguration;
            return this;
        }
    }
}
