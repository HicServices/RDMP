// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAddNewSupportingDocument : BasicCommandExecution
{
    private readonly FileCollectionCombineable _fileCollectionCombineable;
    private readonly Catalogue _targetCatalogue;

    [UseWithObjectConstructor]
    public ExecuteCommandAddNewSupportingDocument(IBasicActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _targetCatalogue = catalogue;
    }

    public ExecuteCommandAddNewSupportingDocument(IBasicActivateItems activator,
        FileCollectionCombineable fileCollectionCombineable, Catalogue targetCatalogue) : base(activator)
    {
        _fileCollectionCombineable = fileCollectionCombineable;
        _targetCatalogue = targetCatalogue;
        var allExisting = targetCatalogue.GetAllSupportingDocuments(FetchOptions.AllGlobalsAndAllLocals);

        foreach (var doc in allExisting)
        {
            var filename = doc.GetFileName();

            if (filename == null)
                continue;

            var collisions = _fileCollectionCombineable.Files.FirstOrDefault(f =>
                f.FullName.Equals(filename.FullName, StringComparison.CurrentCultureIgnoreCase));

            if (collisions != null)
                SetImpossible($"File '{collisions.Name}' is already a SupportingDocument (ID={doc.ID} - '{doc.Name}')");
        }
    }

    public override string GetCommandHelp() =>
        "Marks a file on disk as useful for understanding the dataset and (optionally) copies into project extractions";

    public override void Execute()
    {
        base.Execute();

        var c = _targetCatalogue;

        if (c == null)
        {
            if (BasicActivator.SelectObject(new DialogArgs
                {
                    WindowTitle = "Add SupportingDocument",
                    TaskDescription = "Select which Catalogue you want to add the SupportingDocument to."
                }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected))
                c = selected;
            else
                // user cancelled selecting a Catalogue
                return;
        }

        var files = _fileCollectionCombineable != null
            ? _fileCollectionCombineable.Files
            : [BasicActivator.SelectFile("File to add")];

        if (files == null || files.All(static f => f == null))
            return;

        var created = new List<SupportingDocument>();
        foreach (var doc in files.Select(f => new SupportingDocument((ICatalogueRepository)c.Repository, c, f.Name)
                 {
                     URL = new Uri(f.FullName)
                 }))
        {
            doc.SaveToDatabase();
            created.Add(doc);
        }

        Publish(c);

        Emphasise(created.Last());

        foreach (var doc in created)
            Activate(doc);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.SupportingDocument, OverlayKind.Add);
}