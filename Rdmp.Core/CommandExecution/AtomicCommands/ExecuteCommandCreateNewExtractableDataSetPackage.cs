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

public class ExecuteCommandCreateNewExtractableDataSetPackage : BasicCommandExecution, IAtomicCommand
{
    public ExecuteCommandCreateNewExtractableDataSetPackage(IBasicActivateItems activator) : base(activator)
    {
        if (BasicActivator.RepositoryLocator.DataExportRepository == null)
            SetImpossible("Data export database is not setup");

        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp()
    {
        return "Creates a new grouping of dataset which are commonly extracted together e.g. 'Core datasets on offer'";
    }

    public override void Execute()
    {
        base.Execute();

        if (TypeText("Name for package", "Name", 500, null, out var name))
        {
            var p = new ExtractableDataSetPackage(BasicActivator.RepositoryLocator.DataExportRepository, name);
            Publish(p);
            Emphasise(p);
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Add);
    }
}