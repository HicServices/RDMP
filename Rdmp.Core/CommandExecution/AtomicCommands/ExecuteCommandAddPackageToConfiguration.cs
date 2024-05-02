// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAddPackageToConfiguration : BasicCommandExecution
{
    private readonly ExtractionConfiguration _extractionConfiguration;
    private readonly ExtractableDataSetPackage[] _packages;

    public ExecuteCommandAddPackageToConfiguration(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;

        if (extractionConfiguration.IsReleased)
            SetImpossible("Extraction is Frozen because it has been released and is readonly, try cloning it instead");

        if (activator.CoreChildProvider is DataExportChildProvider childProvider)
        {
            if (childProvider.AllPackages.Any())
                _packages = childProvider.AllPackages;
            else
                SetImpossible("There are no ExtractableDatasetPackages configured");
        }
        else
        {
            SetImpossible("CoreChildProvider is not DataExportIconProvider");
        }
    }

    public override void Execute()
    {
        base.Execute();

        if (SelectOne(_packages, out var package))
            new ExecuteCommandAddDatasetsToConfiguration(BasicActivator, new ExtractableDataSetCombineable(package),
                _extractionConfiguration).Execute();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Import);
    }
}