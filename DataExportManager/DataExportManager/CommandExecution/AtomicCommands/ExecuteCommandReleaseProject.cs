using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.DataRelease;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandReleaseProject: BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Project _project;

        public ExecuteCommandReleaseProject(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Release);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _project = (Project) target;

            if(!_project.ExtractionConfigurations.Any(ec=>ec.IsReleased))
                SetImpossible("There are no unreleased ExtractionConfigurations in Project");

            return this;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<DataReleaseUI, Project>(_project);
        }
    }
}
