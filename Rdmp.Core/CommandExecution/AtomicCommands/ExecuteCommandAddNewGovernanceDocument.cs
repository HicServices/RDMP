// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAddNewGovernanceDocument(IBasicActivateItems activator, GovernancePeriod period)
    : BasicCommandExecution(activator)
{
    private readonly FileInfo _file;

    public ExecuteCommandAddNewGovernanceDocument(IBasicActivateItems activator, GovernancePeriod period, FileInfo file)
        : this(activator, period)
    {
        _file = file;
    }

    public override void Execute()
    {
        base.Execute();

        var p = period;
        var f = _file;

        if (p == null)
            if (!BasicActivator.SelectObject(new DialogArgs
                    {
                        WindowTitle = "Add Governance Document",
                        TaskDescription = "Select which GovernancePeriod you want to attach the document to."
                    }, BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<GovernancePeriod>(),
                    out p))
                // user cancelled selecting a Catalogue
                return;

        f ??= BasicActivator.SelectFile("Document to add");
        if (f == null)
            return;

        var doc = new GovernanceDocument(BasicActivator.RepositoryLocator.CatalogueRepository, p, f);
        Publish(period);
        Emphasise(doc);
        Activate(doc);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.GovernanceDocument, OverlayKind.Add);
}