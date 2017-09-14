using System.Collections.Generic;
using System.Drawing;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
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

            var mef = _activator.RepositoryLocator.CatalogueRepository.MEF;

            var context = CachingPipelineEngineFactory.Context;
            var loadMetadata = _cacheProgress.GetLoadProgress().GetLoadMetadata();

            var uiFactory = new ConfigurePipelineUIFactory(mef, _activator.RepositoryLocator.CatalogueRepository);
            var permissionWindow = _cacheProgress.GetPermissionWindow() ?? new PermissionWindow();
            var pipelineForm = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName,
                _cacheProgress.Pipeline, null, null, context,
                new List<object>
                {
                    new CacheFetchRequestProvider(new CacheFetchRequest(_activator.RepositoryLocator.CatalogueRepository)),
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