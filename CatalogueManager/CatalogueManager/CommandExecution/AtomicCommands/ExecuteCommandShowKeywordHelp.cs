using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowKeywordHelp : BasicCommandExecution, IAtomicCommand
    {
        private readonly DatabaseEntity _databaseEntity;
        private string _className;

        public ExecuteCommandShowKeywordHelp(IActivateItems activator, DatabaseEntity databaseEntity)
        {
            _databaseEntity = databaseEntity;
            _className = databaseEntity.GetType().Name;

            if(!KeywordHelpTextListbox.ContainsKey(_className))
                SetImpossible("No keyword help exists for '" + _className+"'");

        }

        public override string GetCommandName()
        {
            return "Keyword Help:" + _className;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(_databaseEntity,OverlayKind.Help);
        }

        public override void Execute()
        {
            base.Execute();
        
            KeywordHelpTextListbox.ShowKeywordHelp(_className);
        }
    }
}