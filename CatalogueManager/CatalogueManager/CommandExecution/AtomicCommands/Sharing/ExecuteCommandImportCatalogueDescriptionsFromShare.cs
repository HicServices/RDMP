using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands.Sharing
{
    internal class ExecuteCommandImportCatalogueDescriptionsFromShare : ExecuteCommandImportShare, IAtomicCommand
    {
        private readonly Catalogue _targetCatalogue;

        public ExecuteCommandImportCatalogueDescriptionsFromShare(IActivateItems activator, FileCollectionCommand sourceFileCollection, Catalogue targetCatalogue): base(activator, sourceFileCollection)
        {
            _targetCatalogue = targetCatalogue;
            UseTripleDotSuffix = true;
        }

        [ImportingConstructor]
        public ExecuteCommandImportCatalogueDescriptionsFromShare(IActivateItems activator, Catalogue targetCatalogue): base(activator,null)
        {
            _targetCatalogue = targetCatalogue;
            UseTripleDotSuffix = true;
        }

        protected override void ExecuteImpl(ShareManager shareManager, List<ShareDefinition> shareDefinitions)
        {
            var first = shareDefinitions.First();

            if(first.Type != typeof(Catalogue))
                throw new Exception("ShareDefinition was not for a Catalogue");
            
            if(_targetCatalogue.Name != (string) first.Properties["Name"])
                if(MessageBox.Show("Catalogue Name is '"+_targetCatalogue.Name + "' but ShareDefinition is for, '" + first.Properties["Name"] +"'.  Import Anyway?","Import Anyway?",MessageBoxButtons.YesNo) == DialogResult.No)
                    return;

            shareManager.ImportPropertiesOnly(_targetCatalogue, first,true);
            _targetCatalogue.SaveToDatabase();

            var liveCatalogueItems = _targetCatalogue.CatalogueItems;
            
            foreach (ShareDefinition sd in shareDefinitions.Skip(1))
            {
                if(sd.Type != typeof(CatalogueItem))
                    throw new Exception("Unexpected shared object of Type " + sd.Type + " (Expected ShareDefinitionList to have 1 Catalogue + N CatalogueItems)");

                var shareName =(string) sd.Properties["Name"];

                var existingMatch = liveCatalogueItems.FirstOrDefault(ci => ci.Name.Equals(shareName));

                if(existingMatch == null)
                    existingMatch = new CatalogueItem(Activator.RepositoryLocator.CatalogueRepository,_targetCatalogue,shareName);

                shareManager.ImportPropertiesOnly(existingMatch, sd, true);
                existingMatch.SaveToDatabase();
            }

            Publish(_targetCatalogue);
        }
    }
}