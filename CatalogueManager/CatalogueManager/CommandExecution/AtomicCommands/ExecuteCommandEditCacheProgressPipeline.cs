using System;
using System.Collections.Generic;
using System.Drawing;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandEditCacheProgressPipeline:BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly CacheProgress _cacheProgress;

        public ExecuteCommandEditCacheProgressPipeline(IActivateItems activator, CacheProgress cacheProgress)
        {
            _activator = activator;
            _cacheProgress = cacheProgress;
        }

        public override void Execute()
        {
            base.Execute();

            var cataRepo = _activator.RepositoryLocator.CatalogueRepository;
            var mef = cataRepo.MEF;

            var context = CachingPipelineEngineFactory.Context;
            var loadMetadata = _cacheProgress.GetLoadProgress().GetLoadMetadata();

            IPipeline pipeline;
            if (_cacheProgress.Pipeline_ID == null)
            {
                pipeline = new Pipeline(cataRepo, "CachingPipeline_" + Guid.NewGuid());
                _cacheProgress.Pipeline_ID = pipeline.ID;
                _cacheProgress.SaveToDatabase();
            }
            else
                pipeline = _cacheProgress.Pipeline;

            var uiFactory = new ConfigurePipelineUIFactory(mef, cataRepo);
            var permissionWindow = _cacheProgress.GetPermissionWindow() ?? new PermissionWindow();
            var pipelineForm = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName,
                pipeline, null, null, context,
                new List<object>
                {
                    new CacheFetchRequestProvider(new CacheFetchRequest(cataRepo)),
                    permissionWindow,
                    new HICProjectDirectory(loadMetadata.LocationOfFlatFiles, false),
                    mef
                });
            
            pipelineForm.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress,OverlayKind.Edit);
        }
    }
}