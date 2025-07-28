// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandViewCatalogueExtractionSqlUI : BasicUICommandExecution, IAtomicCommandWithTarget
{
    private Catalogue _catalogue;

    [UseWithObjectConstructor]
    public ExecuteCommandViewCatalogueExtractionSqlUI(IActivateItems activator, Catalogue catalogue) : this(activator)
    {
        _catalogue = catalogue;
    }

    public ExecuteCommandViewCatalogueExtractionSqlUI(IActivateItems activator) : base(activator)
    {
    }

    public override string GetCommandHelp() =>
        "View the query that would be executed during extraction of the dataset with the current extractable columns/transforms";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.SQL);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _catalogue = (Catalogue)target;


        //if the catalogue has no extractable columns
        if(!_catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Any())
            SetImpossible("Catalogue has no ExtractionInformations");

        return this;
    }

    public override void Execute()
    {
        Activator.Activate<ViewExtractionSqlUI, Catalogue>(_catalogue);
    }
}