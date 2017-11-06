using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LoadExecutionUIs;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteCacheProgress:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private CacheProgress _cp;

        public ExecuteCommandExecuteCacheProgress(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CacheProgress, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _cp = (CacheProgress) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            Activator.Activate<ExecuteCacheProgressUI, CacheProgress>(_cp);
        }
    }
}
