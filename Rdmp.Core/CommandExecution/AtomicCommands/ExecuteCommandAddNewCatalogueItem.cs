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

public class ExecuteCommandAddNewCatalogueItem : BasicCommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private readonly ColumnInfo[] _columnInfos;
    private readonly HashSet<int> _existingColumnInfos;
    private readonly IBasicActivateItems _activator;

    /// <summary>
    /// The category to assign for newly created <see cref="ExtractionInformation"/>.
    /// Defaults to <see cref="ExtractionCategory.Core"/>.  Set to null to make them non extractable
    /// </summary>
    public ExtractionCategory? Category { get; set; } = ExtractionCategory.Core;

    public ExecuteCommandAddNewCatalogueItem(IBasicActivateItems activator, Catalogue catalogue,
        ColumnInfoCombineable colInfo) : this(activator, catalogue, colInfo.ColumnInfos)
    {
        _activator = activator;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandAddNewCatalogueItem(IBasicActivateItems activator, Catalogue catalogue,
        params ColumnInfo[] columnInfos) : base(activator)
    {
        _activator = activator;
        _catalogue = catalogue;
        _existingColumnInfos = GetColumnInfos(_catalogue);
        _columnInfos = columnInfos;

        if (_existingColumnInfos != null && _columnInfos.Length > 0 &&
            _columnInfos.All(c => AlreadyInCatalogue(c, _existingColumnInfos)))
            SetImpossible("ColumnInfo(s) are already in Catalogue");
    }

    private HashSet<int> GetColumnInfos(ICatalogue catalogue)
    {
        return catalogue == null
            ? null
            : new HashSet<int>(_activator.CoreChildProvider.AllCatalogueItems.Where(ci => ci.Catalogue_ID == catalogue.ID).Select(static ci => ci.ColumnInfo_ID)
                .Where(static col => col.HasValue).Select(static v => v.Value).Distinct());
    }

    public override string GetCommandHelp() =>
        "Creates a new virtual column in the dataset, this is the first stage to making a new column extractable or defining a new extraction transform";

    public override void Execute()
    {
        base.Execute();

        var c = _catalogue;
        var existingColumnInfos = _existingColumnInfos;

        var repo = BasicActivator.RepositoryLocator.CatalogueRepository;

        if (c == null)
        {
            if (!BasicActivator.SelectObject(new DialogArgs
            {
                WindowTitle = "Add CatalogueItem",
                TaskDescription = "Select which Catalogue you want to add the CatalogueItem to."
            }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>(), out var selected))
                // user cancelled selecting a Catalogue
                return;

            c = selected;
            existingColumnInfos = GetColumnInfos(c);
        }

        //if we have not got an explicit one to import let the user pick one
        if (_columnInfos.Length == 0)
        {
            //get them to pick a column info
            var columnInfo = (ColumnInfo)BasicActivator.SelectOne(new DialogArgs
            {
                TaskDescription = "Select which column the new CatalogueItem will describe/extract",
                WindowTitle = "Choose underlying Column"
            }, BasicActivator.CoreChildProvider.AllColumnInfos);

            if (columnInfo == null)
                return;

            //get them to type a name for it (based on the ColumnInfo if picked)
            if (!TypeText("Name", "Type a name for the new CatalogueItem", 500, columnInfo?.GetRuntimeName(),
                    out var text)) return;

            var ci = new CatalogueItem(BasicActivator.RepositoryLocator.CatalogueRepository, c,
                $"New CatalogueItem {Guid.NewGuid()}")
            {
                Name = text
            };

            //set the associated column if they did pick it
            ci.SetColumnInfo(columnInfo);
            CreateExtractionInformation(repo, ci, columnInfo);

            ci.SaveToDatabase();

            Publish(c);
            Emphasise(ci, int.MaxValue);
        }
        else
        {
            foreach (var columnInfo in _columnInfos)
            {
                if (AlreadyInCatalogue(columnInfo, existingColumnInfos))
                    continue;

                var ci = new CatalogueItem(repo, c, columnInfo.GetRuntimeName());
                ci.SetColumnInfo(columnInfo);
                ci.SaveToDatabase();

                // also make extractable
                CreateExtractionInformation(repo, ci, columnInfo);
            }

            Publish(c);
        }
    }

    private void CreateExtractionInformation(ICatalogueRepository repo, CatalogueItem ci, ColumnInfo columnInfo)
    {
        // also make extractable
        if (Category == null) return;

        var ei = new ExtractionInformation(repo, ci, columnInfo, columnInfo.GetFullyQualifiedName());
        if (ei.ExtractionCategory == Category) return;

        ei.ExtractionCategory = Category.Value;
        ei.SaveToDatabase();
    }

    private static bool AlreadyInCatalogue(ColumnInfo candidate, IReadOnlySet<int> existingColumnInfos) =>
        existingColumnInfos.Contains(candidate.ID);

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CatalogueItem, OverlayKind.Add);
}