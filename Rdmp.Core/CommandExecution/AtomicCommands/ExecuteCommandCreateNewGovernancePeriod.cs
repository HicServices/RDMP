// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewGovernancePeriod : BasicCommandExecution, IAtomicCommand
{
    private readonly string _name;

    public ExecuteCommandCreateNewGovernancePeriod(IBasicActivateItems activator, string name = null) : base(activator)
    {
        _name = name;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.GovernancePeriod, OverlayKind.Add);
    }

    public override void Execute()
    {
        base.Execute();

        var name = _name;

        if (name == null && BasicActivator.IsInteractive)
            if (!BasicActivator.TypeText(new DialogArgs
                {
                    WindowTitle = "Governance Period Name",
                    TaskDescription = "Enter a name that describes the Governance required to hold the Catalogue(s).",
                    EntryLabel = "Name"
                }, 255, null, out name, false))
                // user cancelled typing a name
                return;

        var period = new GovernancePeriod(BasicActivator.RepositoryLocator.CatalogueRepository, name);
        Publish(period);
        Emphasise(period);
        Activate(period);
    }
}