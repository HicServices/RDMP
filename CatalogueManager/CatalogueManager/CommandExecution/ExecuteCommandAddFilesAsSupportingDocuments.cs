using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using RDMPObjectVisualisation.Copying;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddFilesAsSupportingDocuments : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly FileCollectionCommand _fileCollectionCommand;
        private readonly Catalogue _targetCatalogue;

        public ExecuteCommandAddFilesAsSupportingDocuments(IActivateItems activator, FileCollectionCommand fileCollectionCommand, Catalogue targetCatalogue)
        {
            _activator = activator;
            _fileCollectionCommand = fileCollectionCommand;
            _targetCatalogue = targetCatalogue;
            var allExisting = targetCatalogue.GetAllSupportingDocuments(FetchOptions.AllGlobalsAndAllLocals);

            foreach (var doc in allExisting)
            {
                string filename = doc.GetFileName();
                
                if(filename == null)
                    continue;

                var collisions = _fileCollectionCommand.Files.FirstOrDefault(f => f.Name.Equals(filename));
                
                if(collisions != null)
                    SetImpossible("File '" + collisions.Name +"' is already a SupportingDocument (ID=" + doc.ID + " - '"+doc.Name+"')");
            }
        }

        public override void Execute()
        {
            base.Execute();

            foreach (var f in _fileCollectionCommand.Files)
            {
                var doc = new SupportingDocument((ICatalogueRepository)_targetCatalogue.Repository, _targetCatalogue, f.Name);
                doc.URL = new Uri(f.FullName); 
                doc.SaveToDatabase();
            }

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_targetCatalogue));
        }
    }
}