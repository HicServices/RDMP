// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandMakeCatalogueItemExtractable : BasicCommandExecution, IAtomicCommand
{
    private readonly CatalogueItem _catalogueItem;

    public ExecuteCommandMakeCatalogueItemExtractable(IBasicActivateItems activator, CatalogueItem catalogueItem) :
        base(activator)
    {
        _catalogueItem = catalogueItem;

        if (_catalogueItem.ColumnInfo_ID == null)
            SetImpossible("There is no underlying ColumnInfo");

        if (_catalogueItem.ExtractionInformation != null)
            SetImpossible("CatalougeItem is already extractable");
    }

    public override string GetCommandHelp()
    {
        return "Make the column/transform available for extraction to researchers";
    }

    public override void Execute()
    {
        base.Execute();

        //Create a new ExtractionInformation (contains the transform sql / column name)
        var newExtractionInformation = new ExtractionInformation(BasicActivator.RepositoryLocator.CatalogueRepository,
            _catalogueItem, _catalogueItem.ColumnInfo, _catalogueItem.ColumnInfo.Name);

        //it will be Core but if the Catalogue is ProjectSpecific then instead we should make our new ExtractionInformation ExtractionCategory.ProjectSpecific
        if (_catalogueItem.Catalogue.IsProjectSpecific(BasicActivator.RepositoryLocator.DataExportRepository))
        {
            newExtractionInformation.ExtractionCategory = ExtractionCategory.ProjectSpecific;
            newExtractionInformation.SaveToDatabase();
        }

        Publish(_catalogueItem);
        Activate(newExtractionInformation);
    }

    public override string GetCommandName()
    {
        return "Make Extractable";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractionInformation, OverlayKind.Add);
    }
}