using System;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCloneExtractionConfiguration : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;

        public ExecuteCommandCloneExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;

            if(!_extractionConfiguration.SelectedDataSets.Any())
                SetImpossible("ExtractionConfiguration does not have any selected datasets");
        }

        public override string GetCommandHelp()
        {
            return "Creates an exact copy of the Extraction Configuration including the cohort selection, all selected datasets, parameters, filter containers, filters etc";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.CloneExtractionConfiguration;
        }

        public override void Execute()
        {
            base.Execute();

            try
            {
                var clone = _extractionConfiguration.DeepCloneWithNewIDs();
                
                Publish((DatabaseEntity)clone.Project);
                Emphasise(clone,int.MaxValue);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}