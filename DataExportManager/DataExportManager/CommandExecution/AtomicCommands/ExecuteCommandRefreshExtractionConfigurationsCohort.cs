using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Repositories;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;
using ReusableUIComponents.Progress;
using ReusableUIComponents.SingleControlForms;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRefreshExtractionConfigurationsCohort : BasicUICommandExecution, IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;
        private Project _project;

        public ExecuteCommandRefreshExtractionConfigurationsCohort(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
            _project = (Project)_extractionConfiguration.Project;
            
            if(extractionConfiguration.Cohort_ID == null)
                SetImpossible("No Cohort Set");

            if (extractionConfiguration.CohortRefreshPipeline_ID == null)
                SetImpossible("No Refresh Pipeline Set");

            if(!_project.ProjectNumber.HasValue)
                SetImpossible("Project '"+_project+"' does not have a Project Number");
        }

        public override void Execute()
        {
            base.Execute();
            
            //show the ui
            var progressUi = new ProgressUI();
            progressUi.Text = "Refreshing Cohort (" + _extractionConfiguration + ")";
            Activator.ShowWindow(progressUi,true);

            var engine = new CohortRefreshEngine(progressUi, _extractionConfiguration);
            Task.Run(

                //run the pipeline in a Thread
                () => engine.Execute()).ContinueWith(s =>
            {
                //then on the UI thread 
                if(s.IsFaulted)
                    return;

                //issue save and refresh
                var newCohort = engine.Request.CohortCreatedIfAny;
                if (newCohort != null)
                {
                    _extractionConfiguration.Cohort_ID = newCohort.ID;
                    _extractionConfiguration.SaveToDatabase();
                    Publish(_extractionConfiguration);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
        }
    }
}
