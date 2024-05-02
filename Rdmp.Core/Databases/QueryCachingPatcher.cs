// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using TypeGuesser;

namespace Rdmp.Core.Databases;

public sealed class QueryCachingPatcher : Patcher
{
    public QueryCachingPatcher() : base(2, "Databases.QueryCachingDatabase")
    {
        LegacyName = "QueryCaching.Database";
        SqlServerOnly = false;
    }

    public override Patch GetInitialCreateScriptContents(DiscoveredDatabase db)
    {
        var header = GetHeader(db.Server.DatabaseType, InitialScriptName, new Version(1, 0, 0));

        var body = db.Helper.GetCreateTableSql(db, "CachedAggregateConfigurationResults", new[]
        {
            new DatabaseColumnRequest("Committer", new DatabaseTypeRequest(typeof(string), 500)),
            new DatabaseColumnRequest("Date", new DatabaseTypeRequest(typeof(DateTime)))
                { Default = MandatoryScalarFunctions.GetTodaysDate },
            new DatabaseColumnRequest("AggregateConfiguration_ID", new DatabaseTypeRequest(typeof(int)))
                { IsPrimaryKey = true },
            new DatabaseColumnRequest("SqlExecuted",
                new DatabaseTypeRequest(typeof(string), int.MaxValue) { Unicode = true }),
            new DatabaseColumnRequest("Operation", new DatabaseTypeRequest(typeof(string), 50)) { IsPrimaryKey = true },

            new DatabaseColumnRequest("TableName", new DatabaseTypeRequest(typeof(string), 500) { Unicode = true })
        }, null, false);

        return new Patch(InitialScriptName, header + body);
    }

    public override SortedDictionary<string, Patch> GetAllPatchesInAssembly(DiscoveredDatabase db)
    {
        var basePatches = base.GetAllPatchesInAssembly(db);
        if (basePatches.Count > 1)
            throw new NotImplementedException(
                "Someone has added some patches, we need to think about how we handle those in MySql and Oracle! i.e. don't add them in '/QueryCachingDatabase/up' please");

        //this is empty because the only patch is already accounted for
        return new SortedDictionary<string, Patch>();
    }
}