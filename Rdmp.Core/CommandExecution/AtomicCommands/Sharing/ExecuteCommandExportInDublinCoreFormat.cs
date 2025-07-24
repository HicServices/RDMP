// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports.DublinCore;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

public class ExecuteCommandExportInDublinCoreFormat : BasicCommandExecution, IAtomicCommand
{
    private readonly Catalogue _catalogue;
    private FileInfo _toExport;

    public ExecuteCommandExportInDublinCoreFormat(IBasicActivateItems activator, Catalogue catalogue) : base(activator)
    {
            _catalogue = catalogue;
            UseTripleDotSuffix = true;
    }

    public override void Execute()
    {
        base.Execute();
        DublinCoreDefinition _definition = DublinCoreTranslater.GenerateFrom(_catalogue);

        if ((_toExport ??= BasicActivator.SelectFile("Dublin Core Xml|*.xml")) == null)
            return;

        using var stream = File.OpenWrite(_toExport.FullName);
        _definition.WriteXml(stream);
    }
}