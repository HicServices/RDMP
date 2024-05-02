// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCloneExtractionConfiguration : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;

    public ExecuteCommandCloneExtractionConfiguration(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;

        if (!_extractionConfiguration.SelectedDataSets.Any())
            SetImpossible("ExtractionConfiguration does not have any selected datasets");
    }

    public override string GetCommandHelp()
    {
        return
            "Creates an exact copy of the Extraction Configuration including the cohort selection, all selected datasets, parameters, filter containers, filters etc";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.CloneExtractionConfiguration);
    }

    public override void Execute()
    {
        base.Execute();

        var clone = _extractionConfiguration.DeepCloneWithNewIDs();

        Publish((DatabaseEntity)clone.Project);
        Emphasise(clone);
    }
}