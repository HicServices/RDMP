// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.EntityNaming;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;

/// <summary>
///     Wrapper for StandardDatabaseHelper (which tells you where RAW, STAGING and LIVE databases are during data load
///     execution).  This class exists for two reasons
///     <para>
///         Firstly to decide (based on IAttachers) whether RAW tables need to be scripted or whether they will appear
///         magically during DLE execution (e.g. by attaching
///         an MDF file).
///     </para>
///     <para>
///         Secondly to allow for overriding the RAW database server (which defaults to localhost).  It is a good idea to
///         have RAW on a different server to LIVE/STAGING
///         in order to reduce the risk incorrectly referencing tables in LIVE in Adjust RAW scripts etc.
///     </para>
/// </summary>
public class HICDatabaseConfiguration
{
    public StandardDatabaseHelper DeployInfo { get; set; }
    public bool RequiresStagingTableCreation { get; set; }

    public INameDatabasesAndTablesDuringLoads DatabaseNamer => DeployInfo.DatabaseNamer;

    /// <summary>
    ///     Optional Regex for fields which will be ignored at migration time between STAGING and LIVE (e.g. hic_ columns).
    ///     This prevents incidental fields like
    ///     valid from, data load run id etc from resulting in live table UPDATEs.
    ///     <para>hic_ columns will always be ignored regardless of this setting</para>
    /// </summary>
    public Regex UpdateButDoNotDiff { get; set; }

    /// <summary>
    ///     Optional Regex for fields which will be completedly ignored at STAGING=>LIVE migration
    /// </summary>
    public Regex IgnoreColumns { get; internal set; }

    /// <summary>
    ///     Preferred Constructor, creates RAW, STAGING, LIVE connection strings based on the data access points in the
    ///     LoadMetadata, also respects the ServerDefaults for RAW override (if any)
    /// </summary>
    /// <param name="lmd"></param>
    /// <param name="namer"></param>
    public HICDatabaseConfiguration(ILoadMetadata lmd, INameDatabasesAndTablesDuringLoads namer = null) :
        this(lmd.GetDistinctLiveDatabaseServer(), namer, lmd.CatalogueRepository, lmd.OverrideRAWServer)
    {
        var globalIgnorePattern = GetGlobalIgnorePatternIfAny(lmd.CatalogueRepository);

        if (globalIgnorePattern != null && !string.IsNullOrWhiteSpace(globalIgnorePattern.Regex))
            IgnoreColumns = new Regex(globalIgnorePattern.Regex);
    }

    public static StandardRegex GetGlobalIgnorePatternIfAny(ICatalogueRepository repository)
    {
        return repository.GetAllObjects<StandardRegex>().OrderBy(r => r.ID)
            .FirstOrDefault(r => r.ConceptName == StandardRegex.DataLoadEngineGlobalIgnorePattern);
    }

    /// <summary>
    ///     Constructor for use in tests, if possible use the LoadMetadata constructor instead
    /// </summary>
    /// <param name="liveServer">The live server where the data is held, IMPORTANT: this must contain InitialCatalog parameter</param>
    /// <param name="namer">optionally lets you specify how to pick database names for the temporary bubbles STAGING and RAW</param>
    /// <param name="defaults">optionally specifies the location to get RAW default server from</param>
    /// <param name="overrideRAWServer">optionally specifies an explicit server to use for RAW</param>
    public HICDatabaseConfiguration(DiscoveredServer liveServer, INameDatabasesAndTablesDuringLoads namer = null,
        IServerDefaults defaults = null, IExternalDatabaseServer overrideRAWServer = null)
    {
        //respects the override of LIVE server
        var liveDatabase = liveServer.GetCurrentDatabase() ??
                           throw new Exception("Cannot load live without having a unique live named database");

        // Default namer
        if (namer == null)
            if (liveServer.DatabaseType == DatabaseType.PostgreSql)
                //create the DLE tables on the live database because postgres can't handle cross database references
                namer = new FixedStagingDatabaseNamer(liveDatabase.GetRuntimeName(), liveDatabase.GetRuntimeName());
            else
                namer = new FixedStagingDatabaseNamer(liveDatabase.GetRuntimeName());

        //if there are defaults
        if (overrideRAWServer == null && defaults != null)
            overrideRAWServer =
                defaults.GetDefaultFor(PermissableDefaults.RAWDataLoadServer); //get the raw default if there is one

        var rawServer =
            //if there was defaults and a raw default server
            overrideRAWServer != null
                ? DataAccessPortal.ExpectServer(overrideRAWServer, DataAccessContext.DataLoad, false)
                : //get the raw server connection
                liveServer; //there is no raw override so we will have to use the live server for RAW too.

        //populates the servers -- note that an empty rawServer value passed to this method makes it the localhost
        DeployInfo = new StandardDatabaseHelper(liveServer.GetCurrentDatabase(), namer, rawServer);

        RequiresStagingTableCreation = true;
    }


    /// <summary>
    ///     Returns all tables in the load as they would be named in the given <paramref name="stage" />
    /// </summary>
    /// <param name="job">DLE job</param>
    /// <param name="stage"></param>
    /// <param name="includeLookups"></param>
    /// <returns></returns>
    public IEnumerable<DiscoveredTable> ExpectTables(IDataLoadJob job, LoadBubble stage, bool includeLookups)
    {
        var db = DeployInfo.DatabaseInfoList[stage];

        foreach (var t in job.RegularTablesToLoad)
            yield return db.ExpectTable(t.GetRuntimeName(stage, DatabaseNamer));

        if (includeLookups)
            foreach (var t in job.LookupTablesToLoad)
                yield return db.ExpectTable(t.GetRuntimeName(stage, DatabaseNamer));
    }
}