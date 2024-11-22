// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandCreateNewQueryCacheDatabase : BasicUICommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;

    public ExecuteCommandCreateNewQueryCacheDatabase(IActivateItems activator,
        CohortIdentificationConfiguration configuration) : base(activator)
    {
        _cic = configuration;
        if (_cic.QueryCachingServer_ID != null)
            SetImpossible("CohortIdentificationConfiguration already has a Query Cache configured");
    }

    public override void Execute()
    {
        base.Execute();

        var p = new QueryCachingPatcher();

        var createPlatform = new CreatePlatformDatabase(p);
        createPlatform.ShowDialog();

        var db = createPlatform.DatabaseCreatedIfAny;
        if (db != null)
        {
            var newServer =
                new ExternalDatabaseServer(Activator.RepositoryLocator.CatalogueRepository, "Caching Database", p);
            newServer.SetProperties(db);

            _cic.QueryCachingServer_ID = newServer.ID;
            _cic.SaveToDatabase();

            SetDefaultIfNotExists(newServer, PermissableDefaults.CohortIdentificationQueryCachingServer_ID, true);

            Publish(_cic);
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);
}