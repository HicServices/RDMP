// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandExecuteAggregateGraph : BasicCommandExecution, IAtomicCommand
{
    private readonly AggregateConfiguration _aggregate;
    private readonly FileInfo _toFile;

    public ExecuteCommandExecuteAggregateGraph(IBasicActivateItems activator, AggregateConfiguration aggregate,
        FileInfo toFile = null) : base(activator)
    {
        _aggregate = aggregate;
        _toFile = toFile;
        if (aggregate.IsCohortIdentificationAggregate)
            SetImpossible("AggregateConfiguration is a Cohort aggregate");

        SetImpossibleIfFailsChecks(aggregate);

        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp()
    {
        return "Assembles and runs the graph query and renders the results as a graph";
    }

    public override void Execute()
    {
        base.Execute();

        if (_toFile != null)
        {
            var collection = new ViewAggregateExtractUICollection(_aggregate);
            ExtractTableVerbatim.ExtractDataToFile(collection, _toFile);
        }
        else
        {
            BasicActivator.ShowGraph(_aggregate);
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.Graph);
    }
}