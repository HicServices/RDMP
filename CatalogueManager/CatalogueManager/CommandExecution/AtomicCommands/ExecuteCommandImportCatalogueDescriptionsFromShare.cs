using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueManager.Copying.Commands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandImportCatalogueDescriptionsFromShare : BasicUICommandExecution, IAtomicCommand
    {
        private FileInfo _shareDefinitionFile;
        private readonly Catalogue _targetCatalogue;

        public ExecuteCommandImportCatalogueDescriptionsFromShare(IActivateItems activator, FileCollectionCommand sourceFileCollection, Catalogue targetCatalogue): this(activator,targetCatalogue)
        {
            if(!sourceFileCollection.IsShareDefinition)
                SetImpossible("Only ShareDefinition files can be imported");

            _shareDefinitionFile = sourceFileCollection.Files.Single();
        }

        [ImportingConstructor]
        public ExecuteCommandImportCatalogueDescriptionsFromShare(IActivateItems activator, Catalogue targetCatalogue): base(activator)
        {
            _targetCatalogue = targetCatalogue;
        }

        public override void Execute()
        {
            base.Execute();

            //ensure file selected
            if((_shareDefinitionFile = _shareDefinitionFile??SelectOpenFile("Share Definition|*.sd")) == null)
                return;

            var json = File.ReadAllText(_shareDefinitionFile.FullName);
            var sm = new ShareManager(Activator.RepositoryLocator);
            
            List<ShareDefinition> shareDefinitions = sm.GetShareDefinitionList(json);

            var first = shareDefinitions.First();

            if(first.Type != typeof(Catalogue))
                throw new Exception("ShareDefinition was not for a Catalogue");


            if(_targetCatalogue.Name != (string) first.Properties["Name"])
                if(MessageBox.Show("Catalogue Name is '"+_targetCatalogue.Name + "' but ShareDefinition is for, '" + first.Properties["Name"] +"'.  Import Anyway?","Import Anyway?",MessageBoxButtons.YesNo) == DialogResult.No)
                    return;

            sm.ImportPropertiesOnly(_targetCatalogue, first,true);
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

                sm.ImportPropertiesOnly(existingMatch, sd, true);
                existingMatch.SaveToDatabase();
            }

            Publish(_targetCatalogue);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_get;
        }
    }
}