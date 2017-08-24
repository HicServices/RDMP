using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;

namespace CachingDashboard
{
    public class CacheProgressTreeNode : TreeNode
    {
        public ICacheProgress CacheProgress { get; private set; }

        public CacheProgressTreeNode(ICacheProgress cacheProgress)
        {
            CacheProgress = cacheProgress;

            var loadProgress = CacheProgress.GetLoadProgress();
            Text = loadProgress.Name;

            if (loadProgress.LockedBecauseRunning)
            {
                ImageIndex = 1;
                SelectedImageIndex = 1;
            }
        }
    }
}