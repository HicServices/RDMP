using System.IO.Packaging;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandPutCatalogueIntoCatalogueFolder: BasicUICommandExecution
    {
        private readonly Catalogue[] _catalogues;
        private readonly CatalogueFolder _targetModel;

        public ExecuteCommandPutCatalogueIntoCatalogueFolder(IActivateItems activator, CatalogueCommand cmd, CatalogueFolder targetModel)
            :this(activator,new []{cmd.Catalogue},targetModel)
        {
            
        }
        public ExecuteCommandPutCatalogueIntoCatalogueFolder(IActivateItems activator, ManyCataloguesCommand cmd, CatalogueFolder targetModel)
            : this(activator, cmd.Catalogues, targetModel)
        {
            
        }

        private ExecuteCommandPutCatalogueIntoCatalogueFolder(IActivateItems activator, Catalogue[] catalogues, CatalogueFolder targetModel) : base(activator)
        {
            _targetModel = targetModel;
            _catalogues = catalogues;
        }
        public override void Execute()
        {
            base.Execute();

            foreach (Catalogue c in _catalogues)
            {
                c.Folder = _targetModel;
                c.SaveToDatabase();
            }

            //Catalogue folder has changed so publish the change (but only change the last Catalogue so we don't end up subing a million global refreshes changes)
            Publish(_catalogues.Last());
        }
    }
}