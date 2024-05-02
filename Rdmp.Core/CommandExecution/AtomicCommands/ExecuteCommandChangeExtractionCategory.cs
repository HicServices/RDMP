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
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandChangeExtractionCategory : BasicCommandExecution
{
    private readonly ExtractionInformation[] _extractionInformations;
    private readonly bool _isProjectSpecific;
    private readonly ExtractionCategory? _category;

    [UseWithObjectConstructor]
    public ExecuteCommandChangeExtractionCategory(IBasicActivateItems activator, ExtractionInformation[] eis,
        ExtractionCategory? category = null) : base(activator)
    {
        eis = eis?.Where(static e => e != null).ToArray() ?? Array.Empty<ExtractionInformation>();

        if (eis.Length == 0)
            SetImpossible("No ExtractionInformations found");

        _extractionInformations = eis;
        _category = category;

        _isProjectSpecific = false;

        var cata = _extractionInformations.Select(static ei => ei.CatalogueItem.Catalogue).Distinct().ToArray();
        if (cata.Length == 1)
            _isProjectSpecific = cata[0].IsProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository);
    }

    public override string GetCommandName()
    {
        return _extractionInformations is not { Length: > 1 }
            ? "Set ExtractionCategory"
            : "Set ALL to ExtractionCategory";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractionInformation);
    }

    public override void Execute()
    {
        base.Execute();

        var c = _category;
        if (c == null && BasicActivator.SelectValueType("New Extraction Category", typeof(ExtractionCategory),
                ExtractionCategory.Core, out var category))
            c = (ExtractionCategory)category;

        if (c == null)
            return;
        if (_isProjectSpecific && c == ExtractionCategory.Core)
        {
            // Don't allow project specific catalogue items to become core
            c = ExtractionCategory.ProjectSpecific;
            Show(
                "Cannot set the Extraction Category to 'Core' for a  Project Specific Catalogue item. It will be saved as 'Project Specific'.");
        }

        if (ExecuteWithCommit(() => ExecuteImpl(c.Value), $"Set ExtractionCategory to '{c}'", _extractionInformations))
            //publish the root Catalogue
            Publish(_extractionInformations.First());
    }

    private void ExecuteImpl(ExtractionCategory category)
    {
        foreach (var ei in _extractionInformations)
        {
            ei.ExtractionCategory = category;
            ei.SaveToDatabase();
        }
    }
}