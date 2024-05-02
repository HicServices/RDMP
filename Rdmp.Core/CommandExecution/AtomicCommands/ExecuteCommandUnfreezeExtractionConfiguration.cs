// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandUnfreezeExtractionConfiguration : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _configuration;

    public ExecuteCommandUnfreezeExtractionConfiguration(IBasicActivateItems activator,
        ExtractionConfiguration configuration) : base(activator)
    {
        _configuration = configuration;

        if (!_configuration.IsReleased)
            SetImpossible("Extraction Configuration is not Frozen");
    }

    public override string GetCommandHelp()
    {
        return "Reopens a released extraction configuration and deletes all record of it ever having been released";
    }

    public override void Execute()
    {
        base.Execute();

        if (YesNo(
                "This will mean deleting the Release Audit for the Configuration making it appear like it was never released in the first place.  If you just want to execute the Configuration again you can Clone it instead if you want.  Are you sure you want to Unfreeze?",
                "Confirm Unfreeze"))
        {
            _configuration.Unfreeze();
            Publish(_configuration);
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.UnfreezeExtractionConfiguration);
    }
}