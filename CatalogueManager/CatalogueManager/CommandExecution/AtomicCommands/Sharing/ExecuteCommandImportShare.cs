using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueManager.Copying.Commands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.Sharing
{
    internal abstract class ExecuteCommandImportShare:BasicUICommandExecution,IAtomicCommand
    {
        private FileInfo _shareDefinitionFile;

        /// <summary>
        /// Sets up the base command to read ShareDefinitions from the selected <paramref name="sourceFileCollection"/> (pass null to have the user pick at Execute)
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="sourceFileCollection"></param>
        protected ExecuteCommandImportShare(IActivateItems activator, FileCollectionCommand sourceFileCollection): base(activator)
        {
            if (sourceFileCollection != null)
            {
                if (!sourceFileCollection.IsShareDefinition)
                    SetImpossible("Only ShareDefinition files can be imported");

                _shareDefinitionFile = sourceFileCollection.Files.Single();
            }
        }

        public virtual Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_get;
        }
        
        public sealed override void Execute()
        {
            base.Execute();
            
            //ensure file selected
            if ((_shareDefinitionFile = _shareDefinitionFile ?? SelectOpenFile("Share Definition|*.sd")) == null)
                return;

            var json = File.ReadAllText(_shareDefinitionFile.FullName);
            var shareManager = new ShareManager(Activator.RepositoryLocator);

            List<ShareDefinition> shareDefinitions = shareManager.GetShareDefinitionList(json);

            ExecuteImpl(shareManager, shareDefinitions);
        }

        protected abstract void ExecuteImpl(ShareManager shareManager,List<ShareDefinition> shareDefinitions);
    }
}