// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Repositories
{
    public interface IRowVerCache
    {
        T[] GetAllObjects<T>();
    }

    public class RowVerCache<T>: IRowVerCache where T : IMapsDirectlyToDatabaseTable
    {
        private readonly TableRepository _repository;

        private List<T> _cachedObjects = new List<T>();
        private object _oLockCachedObjects = new object();

        private byte[] _maxRowVer;
        public long _changeTracking;

        public RowVerCache(TableRepository repository)
        {
            _repository = repository;
        }

        public List<T> GetAllObjects(out List<T> deleted, out List<T> newObjects, out List<T> changedObjects)
        {
            lock (_oLockCachedObjects)
            {
                deleted = new List<T>();
                changedObjects = new List<T>();

                //cache is empty
                if (!_cachedObjects.Any() || _maxRowVer == null)
                {
                    _cachedObjects.AddRange(_repository.GetAllObjectsNoCache<T>());
                    newObjects = _cachedObjects;
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
                    string sql = $@"SELECT  
                    CT.ID
                        FROM  
    CHANGETABLE(CHANGES {typeof(T).Name}, {_changeTracking}) AS CT  
	WHERE SYS_CHANGE_OPERATION = 'D'";

                    using(var cmd = _repository.DiscoveredServer.GetCommand(sql, con))
                        using(var r = cmd.ExecuteReader())
                            while (r.Read())
                            {
                                var d = _cachedObjects.Single(o => o.ID == (int)r["ID"]);
                                deleted.Add(d);
                                _cachedObjects.Remove(d);
                            }
                }

                //get new objects
                var maxId = _cachedObjects.Any() ? _cachedObjects.Max(o => o.ID) : 0;
                newObjects = new List<T>(_repository.GetAllObjects<T>("WHERE ID > " + maxId));
                _cachedObjects.AddRange(newObjects);

                // Get updated objects
                changedObjects = new List<T>(_repository.GetAllObjects<T>("WHERE RowVer > " + ByteArrayToString(_maxRowVer)));
                //I'm hoping Union prefers references in the LHS of this since they will be fresher!
                _cachedObjects = changedObjects.Union(_cachedObjects).ToList();

                UpdateMaxRowVer();

                return _cachedObjects;
            }
        }

        private void UpdateMaxRowVer()
        {
            //get the earliest RowVer
            using (var con = _repository.GetConnection())
            {
                using (var cmd = _repository.DiscoveredServer.GetCommand("select max(RowVer) from " + typeof(T).Name, con))
                    _maxRowVer = (byte[])cmd.ExecuteScalar();

                using (var cmd =
                    _repository.DiscoveredServer.GetCommand("select CHANGE_TRACKING_CURRENT_VERSION()", con))
                {
                    
                    object result = cmd.ExecuteScalar();
                    if(result != DBNull.Value)
                        _changeTracking = Convert.ToInt64(result);
                }
                    
            }
        }
        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return "0x" + hex;
        }

        public T1[] GetAllObjects<T1>()
        {
            return GetAllObjects(out _, out _, out _).Cast<T1>().ToArray();
        }
    }
}