using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandCreateCohortIdentificationConfigurationTemplate : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private CohortIdentificationConfiguration _cic;
        private IMapsDirectlyToDatabaseTable _selectedProject;
        private readonly IBasicActivateItems _activator;
        public ExecuteCommandCreateCohortIdentificationConfigurationTemplate(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
        {
            _activator = activator;
            _cic = cic;
        }

        private string GenerateTemplateName(string name)
        {
            if (name.EndsWith(" (Clone)")) name = name.Substring(0, name.Length - 8);
            if (name.EndsWith(" Template")) return name;
            return name + " Template";
        }

        public override void Execute()
        {
            var associations = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>();
            var projectAssociations = associations.Where(a => a.CohortIdentificationConfiguration_ID == _cic.ID).ToList();
            base.Execute();
            var clone = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
            clone.IsTemplate = true;
            clone.Freeze();
            clone.Name = GenerateTemplateName(clone.Name);
            clone.SaveToDatabase();

            if (_activator.IsInteractive && projectAssociations.Any())
            {
                if (projectAssociations.Count > 1)
                {
                    //multiple, make them pick
                    if (_activator.YesNo("This Cohort Configuration is associated with multiple Projects. Would you like to associate this Template with one of them?", "Associate with a Project"))
                    {
                        _selectedProject = _activator.SelectOne("Select a Project to associate this Template with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
                else if (projectAssociations.Count == 1)
                {

                    //ask them if they want to use this one
                    if (_activator.YesNo($"This cohort configuration is already associated with the {projectAssociations.First().Project.Name} project. Would you like to associate this Template with this project?", "Use Existing Project"))
                    {
                        _selectedProject = projectAssociations.First().Project;
                    }
                }
                else
                {
                    //ask if they want to use it in a project
                    if (_activator.YesNo("Would you like to associate this Template with a project?", "Associate with a Project"))
                    {
                        _selectedProject = _activator.SelectOne("Select a Project to associate this Template with", _activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>());
                    }
                }
            }
            if (_selectedProject != null)
            {
                var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator);
                cmd.SetTarget(clone);
                cmd.SetTarget((Project)_selectedProject);
                cmd.Execute();
            }
            Publish(clone);
            Emphasise(clone);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is not CohortIdentificationConfiguration && target is not Project)
            {
                throw new Exception("Provided database entity was not a CohortIdentificationConfiguration or a Project.");
            }
            if (target is Project p) _selectedProject = p;
            else if (target is CohortIdentificationConfiguration c) _cic = c;
            return this;
        }
    }
}
