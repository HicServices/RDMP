// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     Split the contents of the OS clipboard by newlines and add the content of each
///     line as new <see cref="CatalogueItem" /> to the <see cref="Catalogue" />.  This
///     lets user copy and paste a SELECT sql query column set to create objects in RDMP.
/// </summary>
public class ExecuteCommandPasteClipboardAsNewCatalogueItems : BasicCommandExecution
{
    private readonly Catalogue _catalogue;
    private readonly string _clipboardContents;
    private readonly Func<string> _clipboardContentGetter;

    [UseWithObjectConstructor]
    public ExecuteCommandPasteClipboardAsNewCatalogueItems(IBasicActivateItems activator,
        [DemandsInitialization("The Catalogue to add the new CatalogueItems to")]
        Catalogue catalogue,
        [DemandsInitialization("The contents of the OS clipboard or null to prompt user with a message box at runtime")]
        string clipboardContents) : base(activator)
    {
        _catalogue = catalogue;
        _clipboardContents = clipboardContents;
    }

    public ExecuteCommandPasteClipboardAsNewCatalogueItems(IBasicActivateItems activator, Catalogue catalogue,
        Func<string> clipboardContentGetter) : base(activator)
    {
        _catalogue = catalogue;
        _clipboardContentGetter = clipboardContentGetter;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Clipboard, OverlayKind.Import);
    }

    public override void Execute()
    {
        base.Execute();

        var clipboard = _clipboardContents;

        if (_clipboardContentGetter != null)
        {
            clipboard = _clipboardContentGetter?.Invoke();
        }
        else
        {
            if (clipboard == null)
                if (!BasicActivator.TypeText(new DialogArgs
                    {
                        WindowTitle = "Paste Columns"
                    }, int.MaxValue, null, out clipboard, false))
                    // user cancelled search
                    return;
        }

        if (clipboard == null)
            return;

        var toImport = UsefulStuff.GetArrayOfColumnNamesFromStringPastedInByUser(clipboard).ToArray();

        if (toImport.Any())
        {
            foreach (var name in toImport)
                _ = new CatalogueItem(BasicActivator.RepositoryLocator.CatalogueRepository, _catalogue, name);

            Publish(_catalogue);
        }
    }
}