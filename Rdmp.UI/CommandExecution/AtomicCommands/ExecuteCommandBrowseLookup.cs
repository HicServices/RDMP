// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ExtractionUIs.JoinsAndLookups;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

internal sealed class ExecuteCommandBrowseLookup : BasicUICommandExecution
{
    private readonly Lookup _lookup;

    public ExecuteCommandBrowseLookup(IActivateItems activator, Lookup lookup) : base(activator)
    {
        _lookup = lookup;
    }

    public ExecuteCommandBrowseLookup(IActivateItems activator, IFilter filter) : base(activator)
    {
        var colInfo = filter.GetColumnInfoIfExists();

        if (colInfo != null)
            _lookup =
                colInfo.GetAllLookupForColumnInfoWhereItIsA(LookupType.AnyKey).FirstOrDefault() ??
                colInfo.GetAllLookupForColumnInfoWhereItIsA(LookupType.Description).FirstOrDefault();

        if (_lookup == null)
            SetImpossible("No Lookups found");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Lookup, OverlayKind.Shortcut);

    public override void Execute()
    {
        base.Execute();

        Activator.Activate<LookupBrowserUI, Lookup>(_lookup);
    }
}