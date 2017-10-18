using System;
using System.Windows.Forms;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    internal class CacheProgressMenu : RDMPContextMenuStrip
    {
        private readonly CacheProgress _cacheProgress;

        public CacheProgressMenu(IActivateItems activator, CacheProgress cacheProgress) : base(activator,cacheProgress)
        {
            _cacheProgress = cacheProgress;
            Items.Add(
                "Execute Cache",
                CatalogueIcons.ExecuteArrow,
                (s,e)=>_activator.ExecuteCacheProgress(this,cacheProgress)
                );


            var setWindow = new ToolStripMenuItem("Set PermissionWindow", null);

            foreach (var window in activator.CoreChildProvider.AllPermissionWindows)
                setWindow.DropDownItems.Add(AtomicCommandUIFactory.CreateMenuItem(new ExecuteCommandSetPermissionWindow(activator, cacheProgress,window)));

            setWindow.DropDownItems.Add(new ToolStripSeparator());
            
            setWindow.DropDownItems.Add("Create New Permission Window",_activator.CoreIconProvider.GetImage(RDMPConcept.PermissionWindow,OverlayKind.Add),AddNewPermissionWindow);

            Items.Add(setWindow);

            //this will be used as design time fetch request date, set it to min dt to avoid issues around caches not having progress dates etc
            var fetchRequest = new SingleDayCacheFetchRequestProvider(new CacheFetchRequest(RepositoryLocator.CatalogueRepository,DateTime.MinValue));
            try
            {
                Items.Add(new ChoosePipelineMenuItem(
                    activator,
                    new PipelineUser(_cacheProgress),
                    new CachingPipelineUseCase(_cacheProgress,false, fetchRequest,false),
                    "Set Caching Pipeline")
                    );
            }
            catch (Exception e)
            {
                _activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Could not assemble CacheProgress Pipeline Options", CheckResult.Fail, e));
            }
            AddCommonMenuItems();
        }

        private void AddNewPermissionWindow(object sender, EventArgs e)
        {

            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Permission Window Name","Enter name for the PermissionWindow e.g. 'Nightly Loads'",1000);

            if(dialog.ShowDialog() == DialogResult.OK)
            {
                
                string windowText = dialog.ResultText;
                var newWindow = new PermissionWindow(_activator.RepositoryLocator.CatalogueRepository);
                newWindow.Name = windowText;
                newWindow.SaveToDatabase();

                new ExecuteCommandSetPermissionWindow(_activator, _cacheProgress, newWindow).Execute();
            }
        }
    }

    
}