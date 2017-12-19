using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractionConfigurationForProject:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Project _project;

        public ExecuteCommandCreateNewExtractionConfigurationForProject(IActivateItems activator,Project project) : base(activator)
        {
            _project = project;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var newConfig = new ExtractionConfiguration(Activator.RepositoryLocator.DataExportRepository, _project);

            //refresh the project
            Publish(_project);
            Activate(newConfig);
        }
    }
}
