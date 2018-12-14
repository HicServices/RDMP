using System.Drawing;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandEditCacheProgress : BasicUICommandExecution,IAtomicCommand
    {
        private readonly CacheProgress _cacheProgress;

        public ExecuteCommandEditCacheProgress(IActivateItems activator, CacheProgress cacheProgress):base(activator)
        {
            _cacheProgress = cacheProgress;
        }

        public override void Execute()
        {
            base.Execute();
            Activator.Activate<CacheProgressUI, CacheProgress>(_cacheProgress);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(_cacheProgress);
        }

        public override string GetCommandHelp()
        {
            return "Change which pipeline is used to fetch data, what date the cache has progressed to etc";
        }
    }
}