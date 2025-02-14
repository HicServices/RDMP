// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Creates a new set of parameter values that model a concept (e.g. dementia
/// ICD codes).  The filter must have parameters defined in it and the value
/// set must provide accurate values for those parameters to model the concept
/// </summary>
public class ExecuteCommandAddNewExtractionFilterParameterSet : BasicCommandExecution
{
    private readonly ExtractionFilter _filter;

    public ExecuteCommandAddNewExtractionFilterParameterSet(IBasicActivateItems activator, ExtractionFilter filter) :
        base(activator)
    {
        _filter = filter;

        if (!_filter.GetAllParameters().Any()) SetImpossible("Filter has no parameters");
    }

    public override void Execute()
    {
        base.Execute();

        var parameterSet =
            new ExtractionFilterParameterSet(BasicActivator.RepositoryLocator.CatalogueRepository, _filter);
        parameterSet.CreateNewValueEntries();
        Publish(_filter);
        Activate(parameterSet);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractionFilterParameterSet, OverlayKind.Add);
}