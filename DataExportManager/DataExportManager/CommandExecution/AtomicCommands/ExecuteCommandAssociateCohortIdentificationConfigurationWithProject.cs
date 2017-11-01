using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAssociateCohortIdentificationConfigurationWithProject:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _project;
        private CohortIdentificationConfiguration _cic;

        public ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(IActivateItems activator) : base(activator)
        {

        }

        public override void Execute()
        {
            if(_project == null)
            {
                var dialog =
                    new SelectIMapsDirectlyToDatabaseTableDialog(
                        Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>(),false,false);
                if (dialog.ShowDialog() == DialogResult.OK)
                     SetTarget((Project)dialog.Selected);
                else
                    return;
            }

            if (_cic == null)
            {
                var dialog =
                    new SelectIMapsDirectlyToDatabaseTableDialog(
                        Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(), false, false);
                if (dialog.ShowDialog() == DialogResult.OK)
                    SetTarget((CohortIdentificationConfiguration)dialog.Selected);
                else
                    return;
            }

            //command might be impossible

            base.Execute();

            new ProjectCohortIdentificationConfigurationAssociation(Activator.RepositoryLocator.DataExportRepository,_project, _cic);
            
            Publish(_project);
            Publish(_cic);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            //if we know the cic the context is 'pick a project'
            if(_cic != null)
                return iconProvider.GetImage(RDMPConcept.Project,OverlayKind.Add);

            //if we know the _project the context is 'pick a cic'  (or if we don't know either then just use this icon too)
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is Project)
                _project = (Project)target;

            if (target is CohortIdentificationConfiguration)
                _cic = (CohortIdentificationConfiguration)target;

            if (_project != null && _cic != null)
            {
                if(_project.GetAssociatedCohortIdentificationConfigurations().Contains(_cic))
                    SetImpossible("Cohort Identification Configuration is already associated with this Project");
            }

            return this;
        }
    }
}
