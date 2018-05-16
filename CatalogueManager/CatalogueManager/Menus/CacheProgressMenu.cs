using System;
using System.Windows.Forms;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        private readonly CacheProgress _cacheProgress;

        public CacheProgressMenu(RDMPContextMenuStripArgs args, CacheProgress cacheProgress)
            : base(args, cacheProgress)
        {
            _cacheProgress = cacheProgress;
            
            Add(new ExecuteCommandExecuteCacheProgress(_activator).SetTarget(cacheProgress));
            Add(new ExecuteCommandSetPermissionWindow(_activator,cacheProgress));

            //this will be used as design time fetch request date, set it to min dt to avoid issues around caches not having progress dates etc
            var fetchRequest = new SingleDayCacheFetchRequestProvider(new CacheFetchRequest(RepositoryLocator.CatalogueRepository,DateTime.MinValue));
            try
            {
                Items.Add(new ChoosePipelineMenuItem(
                    _activator,
                    new PipelineUser(_cacheProgress),
                    new CachingPipelineUseCase(_cacheProgress,false, fetchRequest,false),
                    "Set Caching Pipeline")
                    );
            }
            catch (Exception e)
            {
                _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Could not assemble CacheProgress Pipeline Options", CheckResult.Fail, e));
            }
        }
    }
}