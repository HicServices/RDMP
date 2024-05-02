// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

internal sealed class ExecuteCommandCreateNewANOTable : BasicCommandExecution, IAtomicCommand
{
    private readonly IExternalDatabaseServer _anoStoreServer;

    internal ExecuteCommandCreateNewANOTable(IBasicActivateItems activator) : base(activator)
    {
        _anoStoreServer = BasicActivator.ServerDefaults.GetDefaultFor(PermissableDefaults.ANOStore);

        if (_anoStoreServer == null)
            SetImpossible("No default ANOStore has been set");
    }

    public override string GetCommandHelp()
    {
        return
            "Create a table for storing anonymous identifier mappings for a given type of code e.g. 'PatientId' / 'GP Codes' etc";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ANOTable, OverlayKind.Add);
    }

    public override void Execute()
    {
        base.Execute();

        if (!TypeText("ANO Concept Name", "Name", 500, null, out var name)) return;
        if (!TypeText("Type Concept Suffix", "Suffix", 5, null, out var suffix)) return;

        if (!name.StartsWith("ANO", StringComparison.Ordinal))
            name = $"ANO{name}";
        suffix = suffix.Trim('_');

        var anoTable = new ANOTable(BasicActivator.RepositoryLocator.CatalogueRepository,
            (ExternalDatabaseServer)_anoStoreServer, name, suffix);
        Publish(anoTable);
        Activate(anoTable);
    }
}