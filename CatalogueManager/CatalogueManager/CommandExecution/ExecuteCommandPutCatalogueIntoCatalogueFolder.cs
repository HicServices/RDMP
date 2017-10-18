using System.IO.Packaging;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandPutCatalogueIntoCatalogueFolder: BasicCommandExecution
    {
        private readonly IActivateItems _activator;
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

        private ExecuteCommandPutCatalogueIntoCatalogueFolder(IActivateItems activator, Catalogue[] catalogues, CatalogueFolder targetModel)
        {
            _activator = activator;
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

                //Catalogue folder has changed so publish the change
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
            }
        }
    }
}