using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowKeywordHelp : BasicUICommandExecution, IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;
        private string _className;

        public ExecuteCommandShowKeywordHelp(IActivateItems activator, DatabaseEntity databaseEntity) : base(activator)
        {
            _databaseEntity = databaseEntity;
            _className = databaseEntity.GetType().Name;

            if(!KeywordHelpTextListbox.ContainsKey(_className))
                SetImpossible("No keyword help exists for '" + _className+"'");

        }

        public override string GetCommandName()
        {
            return "What is this?";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Help);
        }

        public override void Execute()
        {
            base.Execute();
        
            KeywordHelpTextListbox.ShowKeywordHelp(_className);
        }
    }
}