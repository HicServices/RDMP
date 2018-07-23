using System;
using System.Drawing;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExtractionConfigurationForProject:BasicUICommandExecution,IAtomicCommand
    {
        private readonly Project _project;

        public ExecuteCommandCreateNewExtractionConfigurationForProject(IActivateItems activator,Project project) : base(activator)
        {
            _project = project;
        }

        public override string GetCommandHelp()
        {
            return "Starts a new extraction for the project containing one or more datasets linked against a given cohort";
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
