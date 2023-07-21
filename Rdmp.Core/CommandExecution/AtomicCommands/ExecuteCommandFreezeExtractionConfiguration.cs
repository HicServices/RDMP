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

internal class ExecuteCommandFreezeExtractionConfiguration : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;

    public ExecuteCommandFreezeExtractionConfiguration(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;

        if (extractionConfiguration.IsReleased)
            SetImpossible("ExtractionConfiguration is already released)");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.FrozenExtractionConfiguration);
    }

    public override void Execute()
    {
        base.Execute();

        _extractionConfiguration.IsReleased = true;
        _extractionConfiguration.SaveToDatabase();
        Publish(_extractionConfiguration);
        Emphasise(_extractionConfiguration);
    }
}