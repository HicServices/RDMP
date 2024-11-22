// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Menus;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandShowKeywordHelp : BasicUICommandExecution
{
    private readonly RDMPContextMenuStripArgs _args;

    public ExecuteCommandShowKeywordHelp(IActivateItems activator, RDMPContextMenuStripArgs args) : base(activator)
    {
        _args = args;

        Weight = 100.6f;
    }

    public override string GetCommandName() => "What is this?";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.Help);

    public override string GetCommandHelp() =>
        "Displays the code documentation for the menu object or the Type name of the object if it has none";

    public override void Execute()
    {
        base.Execute();

        string title = null;
        string docs = null;

        //get docs from masquerader if it has any
        if (_args.Masquerader != null)
        {
            title = GetTypeName(_args.Masquerader.GetType());
            docs = Activator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(
                _args.Masquerader.GetType());
        }

        //if not get them from the actual class
        if (docs == null)
        {
            title = GetTypeName(_args.Model.GetType());

            //does the class have state dependent alternative to xmldoc?
            if (_args.Model is IKnowWhatIAm knows)
                docs = knows.WhatIsThis(); //yes
            else
                docs = Activator.RepositoryLocator.CatalogueRepository.CommentStore.GetTypeDocumentationIfExists(
                    _args.Model.GetType());
        }

        //if we have docs show them otherwise just the Type name
        if (docs != null)
            WideMessageBox.ShowKeywordHelp(title, docs);
        else
            MessageBox.Show(title);
    }

    private static string GetTypeName(Type t)
    {
        try
        {
            return MEF.GetCSharpNameForType(t);
        }
        catch (NotSupportedException)
        {
            return t.Name;
        }
    }
}