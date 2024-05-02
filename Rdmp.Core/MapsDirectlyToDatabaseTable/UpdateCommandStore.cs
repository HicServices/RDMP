// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using FAnsi.Implementation;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Stores DbCommands for saving IMapsDirectlyToDatabaseTable objects to the database.
///     <para>
///         Each time a novel IMapsDirectlyToDatabaseTable object Type is encountered an UPDATE sql command (DbCommand) is
///         created for saving the object back to
///         the database (using DbCommandBuilder).  Since this operation (figuring out the UPDATE command) is slow and we
///         might be saving lots of objects we cache
///         the command so that we can apply it to all objects of that Type as they are saved.
///     </para>
/// </summary>
public class UpdateCommandStore
{
    public Dictionary<Type, DbCommand> UpdateCommands { get; }

    public UpdateCommandStore()
    {
        UpdateCommands = new Dictionary<Type, DbCommand>();
    }

    public DbCommand this[IMapsDirectlyToDatabaseTable o] => UpdateCommands[o.GetType()];

    public DbCommand this[Type t] => UpdateCommands[t];

    public void Add(Type o, DbConnectionStringBuilder builder, DbConnection connection, DbTransaction transaction)
    {
        var syntax = ImplementationManager.GetImplementation(builder).GetQuerySyntaxHelper();

        var command = DatabaseCommandHelper.GetCommand($"UPDATE {syntax.EnsureWrapped(o.Name)} SET {{0}} WHERE ID=@ID;",
            connection, transaction);

        var props = TableRepository.GetPropertyInfos(o);

        foreach (var p in props)
            command.Parameters.Add(DatabaseCommandHelper.GetParameter($"@{p.Name}", command));

        command.CommandText = string.Format(command.CommandText,
            string.Join(",",
                props.Where(p => p.Name != "ID").Select(p => $"{syntax.EnsureWrapped(p.Name)}=@{p.Name}")));

        UpdateCommands.Add(o, command);
    }

    public bool ContainsKey(IMapsDirectlyToDatabaseTable toCreate)
    {
        return UpdateCommands.ContainsKey(toCreate.GetType());
    }

    public bool ContainsKey(Type toCreate)
    {
        return UpdateCommands.ContainsKey(toCreate);
    }

    public void Clear()
    {
        UpdateCommands.Clear();
    }
}