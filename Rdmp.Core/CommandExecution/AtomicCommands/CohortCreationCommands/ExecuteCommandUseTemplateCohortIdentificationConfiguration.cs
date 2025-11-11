using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
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
        private CohortIdentificationConfiguration _cic;
        private IBasicActivateItems _activator;
        public ExecuteCommandUseTemplateCohortIdentificationConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
        {
            _activator = activator;
            _cic = cic;
        }

        private string RenameTemplateForUse(string name)
        {
            if (name.EndsWith("Template"))
            {
                name = name.Substring(0,name.Length - 8);
            }
            while(name.EndsWith(" "))
            {
                name = name.Substring(0,name.Length - 1);
            }
            return name;
        }

        public override void Execute()
        {
            var associations = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>();
            var projectAssociations = associations.Where(a => a.CohortIdentificationConfiguration_ID == _cic.ID).ToList();
            IMapsDirectlyToDatabaseTable selectedProject = null;
            if (_activator.IsInteractive && projectAssociations.Any())
            {
                if (projectAssociations.Count > 1)
                {
                    //multiple, make them pick
                    if (_activator.YesNo("This Template is associated with multiple Projects. Would you like to associate this cohort configuration with one of them?", "Associate with a Project"))
                    {
                        selectedProject = _activator.SelectOne("Select a Project to associate this cohort with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
                else if (projectAssociations.Count == 1)
                {

                    //ask them if they want to use this one
                    if (_activator.YesNo($"This template is already associated with the {projectAssociations.First().Project.Name} project. Would you like to associate this cohort configuration with this project?", "Use Existing Project"))
                    {
                        selectedProject = projectAssociations.First().Project;
                    }
                }
                else
                {
                    //ask if they want to use it in a project
                    if (_activator.YesNo("Would you like to associate this cohort configuration with a project?", "Associate with a Project"))
                    {
                        selectedProject = _activator.SelectOne("Select a Project to associate this cohort with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
            }
            base.Execute();
            var clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
            clone.IsTemplate = false;
            clone.Name = RenameTemplateForUse(clone.Name);
            clone.SaveToDatabase();
            if (selectedProject != null)
            {
                var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator);
                cmd.SetTarget(clone);
                cmd.SetTarget((Project)selectedProject);
                cmd.Execute();
            }
            Publish(clone);
            Emphasise(clone);
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
