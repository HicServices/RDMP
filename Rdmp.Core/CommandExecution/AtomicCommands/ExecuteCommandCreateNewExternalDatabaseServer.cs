// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateNewExternalDatabaseServer : BasicCommandExecution, IAtomicCommand
{
    private readonly PermissableDefaults _defaultToSet;
    private readonly ExternalDatabaseServerStateBasedIconProvider _databaseIconProvider;
    private readonly IPatcher _patcher;
    private readonly DiscoveredDatabase _database;

    public ExternalDatabaseServer ServerCreatedIfAny { get; private set; }


    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewExternalDatabaseServer(IBasicActivateItems activator,
        PermissableDefaults defaultToSet, DiscoveredDatabase toCreate)
        : this(activator, defaultToSet == PermissableDefaults.None ? null : defaultToSet.ToTier2DatabaseType(),
            defaultToSet)
    {
        _database = toCreate;
    }

    public ExecuteCommandCreateNewExternalDatabaseServer(IBasicActivateItems activator, IPatcher patcher,
        PermissableDefaults defaultToSet) : base(activator)
    {
        _patcher = patcher;
        _defaultToSet = defaultToSet;

        _databaseIconProvider = new ExternalDatabaseServerStateBasedIconProvider();

        //do we already have a default server for this?
        var existingDefault = BasicActivator.ServerDefaults.GetDefaultFor(_defaultToSet);

        if (existingDefault != null)
            SetImpossible($"There is already an existing {_defaultToSet} database");
    }

    public override string GetCommandName()
    {
        if (_defaultToSet != PermissableDefaults.None)
            return
                $"Create New {UsefulStuff.PascalCaseStringToHumanReadable(_defaultToSet.ToString().Replace("_ID", "").Replace("Live", "").Replace("ANO", "Anonymisation"))} Server...";

        return _patcher != null ? $"Create New {_patcher.Name} Server..." : base.GetCommandName();
    }

    public override string GetCommandHelp()
    {
        return _defaultToSet switch
        {
            PermissableDefaults.LiveLoggingServer_ID =>
                "Creates a database for auditing all flows of data (data load, extraction etc) including tables for errors, progress tables/record count loaded etc",
            PermissableDefaults.IdentifierDumpServer_ID =>
                "Creates a database for storing the values of intercepted columns that are discarded during data load because they contain identifiable data",
            PermissableDefaults.DQE =>
                "Creates a database for storing the results of data quality engine runs on your datasets over time.",
            PermissableDefaults.WebServiceQueryCachingServer_ID => "Defines a new server that can be accessed by RDMP",
            PermissableDefaults.RAWDataLoadServer =>
                "Defines which database server should be used for the RAW data in the RAW=>STAGING=>LIVE model of the data load engine",
            PermissableDefaults.ANOStore =>
                "Creates a new anonymisation database which contains mappings of identifiable values to anonymous representations",
            PermissableDefaults.CohortIdentificationQueryCachingServer_ID =>
                "Creates a new Query Cache database which contains the indexed results of executed subqueries in a CohortIdentificationConfiguration",
            _ => "Defines a new server that can be accessed by RDMP"
        };
    }

    public override void Execute()
    {
        base.Execute();

        //user wants to create a new server e.g. a new Logging server
        if (_patcher == null)
        {
            ServerCreatedIfAny = new ExternalDatabaseServer(BasicActivator.RepositoryLocator.CatalogueRepository,
                $"New ExternalDatabaseServer {Guid.NewGuid()}", _patcher);
            if (_database is not null)
                ServerCreatedIfAny.SetProperties(_database);
        }
        else
            //create the new server
        {
            ServerCreatedIfAny = BasicActivator.CreateNewPlatformDatabase(
                BasicActivator.RepositoryLocator.CatalogueRepository,
                _defaultToSet,
                _patcher,
                _database);
        }

        //user cancelled creating a server
        if (ServerCreatedIfAny == null)
            return;

        Publish(ServerCreatedIfAny);
        Activate(ServerCreatedIfAny);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        if (_patcher == null) return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);

        var basicIcon = _databaseIconProvider.GetIconForAssembly(_patcher.GetDbAssembly());
        return IconOverlayProvider.GetOverlay(basicIcon, OverlayKind.Add);
    }
}