// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandSetQueryCachingDatabase : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    private readonly ExternalDatabaseServer[] _caches;

    public ExecuteCommandSetQueryCachingDatabase(IBasicActivateItems activator, CohortIdentificationConfiguration cic) :
        base(activator)
    {
        _cic = cic;
        _caches = activator.CoreChildProvider.AllExternalServers.Where(es => es.WasCreatedBy(new QueryCachingPatcher())).ToArray();
        if (!_caches.Any())
            SetImpossible("There are no Query Caching databases set up");
    }

    public override void Execute()
    {
        base.Execute();

        if (!SelectOne(_caches.ToList(), out var selected)) return;

        _cic.QueryCachingServer_ID = selected?.ID;
        _cic.SaveToDatabase();
        Publish(_cic);
    }

    public override string GetCommandName() =>
        _cic.QueryCachingServer_ID == null ? "Set Query Cache" : "Change Query Cache";

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Link);
}