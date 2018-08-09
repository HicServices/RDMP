using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowKeywordHelp : BasicUICommandExecution, IAtomicCommand
    {
        private RDMPContextMenuStripArgs _args;

        public ExecuteCommandShowKeywordHelp(IActivateItems activator,  RDMPContextMenuStripArgs args) : base(activator)
        {
            _args = args;
        }

        public override string GetCommandName()
        {
            return "What is this?";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Help);
        }

        public override string GetCommandHelp()
        {
            return "Displays the code documentation for the menu object or the Type name of the object if it has none";
        }

        public override void Execute()
        {
            base.Execute();
            
            var modelType = _args.Model.GetType();

            if (_args.Masquerader != null)
            {
                var masqueradingAs = _args.Masquerader.MasqueradingAs();

                if(MessageBox.Show("Node is '" + MEF.GetCSharpNameForType(_args.Masquerader.GetType()) + "'.  Show help for '" +
                    MEF.GetCSharpNameForType(masqueradingAs.GetType()) + "'?", "Show Help", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;

                modelType = masqueradingAs.GetType();
            }

            if (KeywordHelpTextListbox.ContainsKey(modelType.Name))
                KeywordHelpTextListbox.ShowKeywordHelp(modelType.Name);
            else
                MessageBox.Show(MEF.GetCSharpNameForType(modelType));
        }
    }
}