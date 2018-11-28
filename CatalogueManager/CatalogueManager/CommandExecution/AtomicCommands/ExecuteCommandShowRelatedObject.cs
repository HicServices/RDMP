using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowRelatedObject : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private ObjectImport _import;
        private ObjectExport _export;
        private string _commandName;

        public ExecuteCommandShowRelatedObject(IActivateItems activator, ObjectImport node) : base(activator)
        {
            _import = node;
            _export = null;
            _commandName = "View the object that relates to this import definition";
        }

        public ExecuteCommandShowRelatedObject(IActivateItems activator, ObjectExport node) : base(activator)
        {
            _export = node;
            _import = null;
            _commandName = "View the object that relates to this export definition";
        }

        public override string GetCommandHelp()
        {
            return _commandName;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.AllObjectSharingNode);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            return this;
        }

        public override void Execute()
        {
            if (_import != null)
                Emphasise((DatabaseEntity)_import.GetReferencedObject(Activator.RepositoryLocator));
            
            if (_export != null)
                Emphasise((DatabaseEntity) _export.GetReferencedObject(Activator.RepositoryLocator));
        }
    }
}