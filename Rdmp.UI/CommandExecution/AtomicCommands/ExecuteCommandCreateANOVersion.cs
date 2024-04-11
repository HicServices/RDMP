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
using Rdmp.UI.ANOEngineeringUIs;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateANOVersion : BasicUICommandExecution, IAtomicCommandWithTarget
{
    private Catalogue _catalogue;

    [UseWithObjectConstructor]
    public ExecuteCommandCreateANOVersion(IActivateItems activator, Catalogue catalogue) : this(activator)
    {
        SetTarget(catalogue);
    }

    public ExecuteCommandCreateANOVersion(IActivateItems activator) : base(activator)
    {
        UseTripleDotSuffix = true;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => iconProvider.GetImage(RDMPConcept.ANOTable);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _catalogue = (Catalogue)target;

        if (!_catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Any())
            SetImpossible("Catalogue does not have any Extractable Columns");

        return this;
    }

    public override string GetCommandHelp() =>
        "Create an anonymous version of the dataset.  This will be an initially empty anonymous schema and a load configuration for migrating the data.";

    public override void Execute()
    {
        if (_catalogue == null)
            SetTarget(SelectOne<Catalogue>(Activator.CoreChildProvider.AllCatalogues.Value));

        if (_catalogue == null)
            return;

        base.Execute();

        Activator.Activate<ForwardEngineerANOCatalogueUI, Catalogue>(_catalogue);
    }
}