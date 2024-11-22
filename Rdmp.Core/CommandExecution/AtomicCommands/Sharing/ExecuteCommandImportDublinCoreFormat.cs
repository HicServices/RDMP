// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Xml.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports.DublinCore;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Sharing;

public class ExecuteCommandImportDublinCoreFormat : BasicCommandExecution
{
    private Catalogue _target;
    private FileInfo _toImport;
    private readonly DublinCoreTranslater _translater = new();

    public ExecuteCommandImportDublinCoreFormat(IBasicActivateItems activator, Catalogue catalogue) : base(activator)
    {
        _target = catalogue;
        UseTripleDotSuffix = true;
    }

    public override void Execute()
    {
        base.Execute();

        if ((_toImport ??= BasicActivator.SelectFile("Enter Dublin Core Xml File Path:", "Dublin Core Xml", "*.xml")) ==
            null)
            return;

        var dc = new DublinCoreDefinition();
        var doc = XDocument.Load(_toImport.FullName);
        dc.LoadFrom(doc.Root);

        DublinCoreTranslater.Fill(_target, dc);
        _target.SaveToDatabase();

        Publish(_target);
    }
}