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
using DataExportManager.ProjectUI;
using Diagnostics.TestData;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionConfiguration:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private readonly bool _autoStart;
        private ExtractionConfiguration _extractionConfiguration;

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, bool autoStart = false) : base(activator)
        {
            _autoStart = autoStart;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.ExecuteArrow;
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _extractionConfiguration = (ExtractionConfiguration)target;

            //must have datasets, must not be released and must have a cohort

            if(_extractionConfiguration.IsReleased)
            {
                SetImpossible("ExtractionConfiguration is released so cannot be executed");
                return this;
            }

            if(_extractionConfiguration.Cohort_ID == null)
            {
                SetImpossible("No cohort has been configured for ExtractionConfiguration");
                return this;
            }

            if (!_extractionConfiguration.GetAllExtractableDataSets().Any())
                SetImpossible("ExtractionConfiguration does not have an selected datasets");

            return this;
        }

        public override void Execute()
        {
            base.Execute();
            var ui = Activator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(_extractionConfiguration);
            
            if(_autoStart)
                ui.Start();

        }
    }
}
