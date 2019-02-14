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
            
            string title = null;
            string docs = null;
            
            //get docs from masquerader if it has any
            if (_args.Masquerader != null)
            {
                title = MEF.GetCSharpNameForType(_args.Masquerader.GetType());
                docs = Activator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(_args.Masquerader.GetType());
            }
            
            //if not get them from the actual class
            if(docs == null)
            {
                title = MEF.GetCSharpNameForType(_args.Model.GetType());
                docs = Activator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(_args.Model.GetType());
            }

            //if we have any extra text to append
            if (!string.IsNullOrWhiteSpace(_args.ExtraKeywordHelpText))
                docs += Environment.NewLine + _args.ExtraKeywordHelpText;

            //if we have docs show them otherwise just the Type name
            if (docs != null)
                WideMessageBox.ShowKeywordHelp(title, docs);
            else
                MessageBox.Show(title);
        }
    }
}