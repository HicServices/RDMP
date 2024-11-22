// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandLinkCatalogueItemToColumnInfo : BasicCommandExecution
{
    private readonly CatalogueItem _catalogueItem;
    private ColumnInfo _columnInfo;


    public ExecuteCommandLinkCatalogueItemToColumnInfo(IBasicActivateItems activator, CatalogueItem catalogueItem) :
        base(activator)
    {
        _catalogueItem = catalogueItem;

        if (_catalogueItem.ColumnInfo_ID != null)
            SetImpossible("ColumnInfo is already set");
    }

    public override string GetCommandHelp() =>
        "Resolve an orphaned virtual column by matching it up to an actual column in the underlying database";

    public ExecuteCommandLinkCatalogueItemToColumnInfo(IBasicActivateItems activator, ColumnInfoCombineable cmd,
        CatalogueItem catalogueItem) : base(activator)
    {
        _catalogueItem = catalogueItem;

        if (catalogueItem.ColumnInfo_ID != null)
            SetImpossible($"CatalogueItem {catalogueItem} is already associated with a different ColumnInfo");

        if (cmd.ColumnInfos.Length > 1)
        {
            SetImpossible("Only one ColumnInfo can be associated with a CatalogueItem at a time");
            return;
        }

        _columnInfo = cmd.ColumnInfos[0];
    }

    public override void Execute()
    {
        base.Execute();

        _columnInfo ??=
            SelectOne<ColumnInfo>(BasicActivator.RepositoryLocator.CatalogueRepository, _catalogueItem.Name);

        if (_columnInfo == null)
            return;

        _catalogueItem.SetColumnInfo(_columnInfo);

        //if it did not have a name before
        if (_catalogueItem.Name.StartsWith("New CatalogueItem"))
        {
            //give it one
            _catalogueItem.Name = _columnInfo.GetRuntimeName();
            _catalogueItem.SaveToDatabase();
        }

        //Either way refresh the catalogue item
        Publish(_catalogueItem);
    }

    public override string GetCommandName() =>
        $"Set Column Info{(_catalogueItem.ColumnInfo_ID == null ? "(Currently MISSING)" : "")}";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ColumnInfo, OverlayKind.Problem);
}