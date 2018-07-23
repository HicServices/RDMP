using System;
using System.Drawing;
using System.Linq;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandGenerateReleaseDocument : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;

        public ExecuteCommandGenerateReleaseDocument(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
            /////////////////Other stuff///////////
            if(!extractionConfiguration.CumulativeExtractionResults.Any())
                SetImpossible("No datasets have been extracted");
        }

        public override string GetCommandHelp()
        {
            return "Generate a document describing what has been extracted so far for each dataset in the extraction configuration including number of rows, distinct patient counts etc";
        }

        public override void Execute()
        {
            base.Execute();
            
            try
            {
                WordDataReleaseFileGenerator generator = new WordDataReleaseFileGenerator(_extractionConfiguration, Activator.RepositoryLocator.DataExportRepository);
                
                //null means leave word file on screen and dont save
                generator.GenerateWordFile(null);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_word;
        }
    }
}