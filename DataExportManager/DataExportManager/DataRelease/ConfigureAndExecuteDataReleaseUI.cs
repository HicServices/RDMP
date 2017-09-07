using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using DataExportLibrary.DataRelease.ReleasePipeline;
using RDMPObjectVisualisation.Pipelines;
using ReusableUIComponents;
using ReusableUIComponents.Progress;

namespace DataExportManager.DataRelease
{
    public partial class ConfigureAndExecuteDataReleaseUI : ConfigureAndExecuteDataReleaseUI_Design
    {
        public ConfigureAndExecuteDataReleaseUI()
        {
            InitializeComponent();
        }


        private bool _haveSetupRibbon = false;
        private Project _project;

        public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            _project = databaseObject;

            SetupPipeline();


            if (!_haveSetupRibbon)
            {
                _haveSetupRibbon = true;
                rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);
                rdmpObjectsRibbonUI1.Add(_project);
            }
            
        }

        private PipelineSelectionUI<ReleaseData> _pipelineUI;

        private void SetupPipeline()
        {
            if (_pipelineUI == null)
            {
                var cataRepository = _activator.RepositoryLocator.CatalogueRepository;
                _pipelineUI = new PipelineSelectionUI<ReleaseData>(null, null, cataRepository);
                _pipelineUI.Context = ReleaseEngine.Context;
                _pipelineUI.InitializationObjectsForPreviewPipeline.Add(_project);
                _pipelineUI.InitializationObjectsForPreviewPipeline.Add(_activator);

                _pipelineUI.Dock = DockStyle.Fill;
                pPipeline.Controls.Add(_pipelineUI);

            }
        }


        private void btnExecutePipeline_Click(object sender, EventArgs e)
        {
            if (_pipelineUI.Pipeline == null)
                return;

            var factory = new DataFlowPipelineEngineFactory<ReleaseData>(_activator.RepositoryLocator.CatalogueRepository.MEF, ReleaseEngine.Context);
            var engine = factory.Create(_pipelineUI.Pipeline, progressUI1);

            engine.Initialize(_project, _activator);
            engine.ExecutePipeline(new GracefulCancellationToken());
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ConfigureAndExecuteDataReleaseUI_Design, UserControl>))]
    public abstract class ConfigureAndExecuteDataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>
    {
        
    }
}
