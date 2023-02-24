// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.QueryCaching.Aggregation.Arguments;

public abstract class CacheCommitArguments
{
    protected readonly int Timeout;
    public AggregateOperation Operation { get; private set; }
    public AggregateConfiguration Configuration { get; set; }
    public string SQL { get; private set; }
    public DataTable Results { get; private set; }
    public DatabaseColumnRequest[] ExplicitColumns { get; private set; }

    protected CacheCommitArguments(AggregateOperation operation, AggregateConfiguration configuration, string sql, DataTable results, int timeout, DatabaseColumnRequest[] explicitColumns = null)
    {
        Timeout = timeout;
        Operation = operation;
        Configuration = configuration;
        SQL = sql;
        Results = results;
        ExplicitColumns = explicitColumns;

        if (results == null)
            throw new Exception("DataTable results must have a value");

    }

    public abstract void CommitTableDataCompleted(DiscoveredTable resultingTable);
}