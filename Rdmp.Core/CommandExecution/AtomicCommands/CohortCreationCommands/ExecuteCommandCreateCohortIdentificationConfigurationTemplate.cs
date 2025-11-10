using NPOI.OpenXmlFormats.Encryption;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
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
            //todo think about asking if we want to associate to he project if it already is
            var associations = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>();
            var projectAssociations = associations.Where(a => a.CohortIdentificationConfiguration_ID == _cic.ID).ToList();
            base.Execute();
            var clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
            clone.IsTemplate = true;
            clone.Freeze();
            clone.Name = clone.Name.Replace("(Clone)", "") + " Template";
            clone.SaveToDatabase();
            IMapsDirectlyToDatabaseTable selectedProject = null;

            if (_activator.IsInteractive && projectAssociations.Any()) {
                if (projectAssociations.Count > 1)
                {
                    //multiple, make them pick
                    if (_activator.YesNo("This Cohort Configuration is associated with multiple Projects. Would you like to associate this Template with one of them?", "Associate with a Project"))
                    {
                        selectedProject = _activator.SelectOne("Select a Project to associate this Template with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
                else if (projectAssociations.Count == 1)
                {

                    //ask them if they want to use this one
                    if (_activator.YesNo($"This cohort configuration is already associated with the {projectAssociations.First().Project.Name} project. Would you like to associate this Template with this project?", "Use Existing Project"))
                    {
                        selectedProject = projectAssociations.First().Project;
                    }
                }
                else
                {
                    //ask if they want to use it in a project
                    if (_activator.YesNo("Would you like to associate this Template with a project?", "Associate with a Project"))
                    {
                        selectedProject = _activator.SelectOne("Select a Project to associate this Template with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
            }
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
