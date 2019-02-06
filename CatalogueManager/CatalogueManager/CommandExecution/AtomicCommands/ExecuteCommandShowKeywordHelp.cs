// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
using ReusableUIComponents.Dialogs;

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

            var docs = Activator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(modelType);

            if (!string.IsNullOrWhiteSpace(_args.ExtraKeywordHelpText))
                docs += Environment.NewLine + _args.ExtraKeywordHelpText;

            if (docs != null)
                WideMessageBox.ShowKeywordHelp(modelType.Name, docs);
            else
                MessageBox.Show(MEF.GetCSharpNameForType(modelType));
        }
    }
}