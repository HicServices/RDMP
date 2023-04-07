// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewRemoteRDMP : BasicCommandExecution, IAtomicCommand
{
    public ExecuteCommandCreateNewRemoteRDMP(IBasicActivateItems activator) : base(activator)
    {
    }

    public override void Execute()
    {
        base.Execute();
        var remote = new RemoteRDMP(BasicActivator.RepositoryLocator.CatalogueRepository);
        Publish(remote);
        Emphasise(remote);
        Activate(remote);
    }

    public override string GetCommandHelp()
    {
        return "Tell RDMP about another instance of RDMP that is available for communication with via a web service";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.RemoteRDMP, OverlayKind.Add);
    }
}