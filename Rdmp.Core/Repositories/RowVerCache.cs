// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Data.SqlClient;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Repositories;

/// <summary>
///     Caches the results of calls to GetAllObjects of <see cref="TableRepository" /> using a mixture of
///     Sql Server 'Change Tracking' and a RowVer column.  This reduces the number of objects that need to be
///     constructed / updated when an RDMP platform database stores hundreds of thousands of objects.
/// </summary>
/// <typeparam name="T"></typeparam>
public class RowVerCache<T> : IRowVerCache where T : IMapsDirectlyToDatabaseTable
{
    private readonly TableRepository _repository;

    private List<T> _cachedObjects = new();
    private readonly object _oLockCachedObjects = new();

    private byte[] _maxRowVer;
    public long _changeTracking;

    /// <summary>
    ///     True if the caching system is broken for some reason (e.g. user lacks certain permissions)
    /// </summary>
    public bool Broken => BrokenReason != null;

    /// <summary>
    ///     The error that triggered <see cref="Broken" /> (and ended use of the object caching strategy)
    /// </summary>
    public SqlException BrokenReason { get; set; }

    public RowVerCache(TableRepository repository)
    {
        _repository = repository;
    }

    public List<T> GetAllObjects()
    {
        if (Broken || !Monitor.TryEnter(_oLockCachedObjects)) return _repository.GetAllObjectsNoCache<T>().ToList();
        try
        {
            //cache is empty
            if (!_cachedObjects.Any() || _maxRowVer == null)
            {
                _cachedObjects.Clear();
                _cachedObjects.AddRange(_repository.GetAllObjectsNoCache<T>());
                UpdateMaxRowVer();
                return _cachedObjects;
            }

            // Get deleted objects
            /*
                SELECT
    CT.ID
FROM
    CHANGETABLE(CHANGES Catalogue, @last_synchronization_version) AS CT
    WHERE SYS_CHANGE_OPERATION = 'D'

    */

            using (var con = _repository.GetConnection())
            {
                var sql = $@"SELECT  
                        CT.ID
                            FROM  
        CHANGETABLE(CHANGES {typeof(T).Name}, {_changeTracking}) AS CT  
        WHERE SYS_CHANGE_OPERATION = 'D'";

                using var cmd = _repository.DiscoveredServer.GetCommand(sql, con);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    //it might have been added and deleted by someone else and we never saw it
                    var d = _cachedObjects.SingleOrDefault(o => o.ID == (int)r["ID"]);

                    if (d != null)
                        _cachedObjects.Remove(d);
                }
            }

            //get new objects
            var maxId = _cachedObjects.Any() ? _cachedObjects.Max(o => o.ID) : 0;
            _cachedObjects.AddRange(_repository.GetAllObjects<T>($"WHERE ID > {maxId}"));

            // Get updated objects
            var changedObjects =
                _repository.GetAllObjects<T>($"WHERE RowVer > {ByteArrayToString(_maxRowVer)}");
            //I'm hoping Union prefers references in the LHS of this since they will be fresher!
            _cachedObjects = changedObjects.Union(_cachedObjects).ToList();

            UpdateMaxRowVer();

            return _cachedObjects;
        }
        catch (SqlException ex)
        {
            BrokenReason = ex;
        }
        finally
        {
            Monitor.Exit(_oLockCachedObjects);
        }

        //we were unable to get a lock
        return _repository.GetAllObjectsNoCache<T>().ToList();
    }


    private void UpdateMaxRowVer()
    {
        //get the latest RowVer
        using var con = _repository.GetConnection();
        var table = _repository.DiscoveredServer.GetCurrentDatabase().ExpectTable(typeof(T).Name);
        if (table.Exists() && table.DiscoverColumns()
                .Any(c => c.GetRuntimeName().Equals("RowVer", StringComparison.InvariantCultureIgnoreCase)))
        {
            using var cmd = _repository.DiscoveredServer.GetCommand($"select max(RowVer) from {typeof(T).Name}", con);
            var result = cmd.ExecuteScalar();
            _maxRowVer = result == DBNull.Value ? null : (byte[])result;
        }


        using (var cmd =
               _repository.DiscoveredServer.GetCommand("select CHANGE_TRACKING_CURRENT_VERSION()", con))
        {
            var result = cmd.ExecuteScalar();
            if (result != DBNull.Value)
                _changeTracking = Convert.ToInt64(result);
        }
    }

    private static string ByteArrayToString(IReadOnlyCollection<byte> ba)
    {
        var hex = new StringBuilder("0x", 2 + ba.Count * 2);
        foreach (var b in ba)
            hex.Append($"{b:x2}");
        return hex.ToString();
    }

    public T1[] GetAllObjects<T1>()
    {
        return GetAllObjects().Cast<T1>().ToArray();
    }
}