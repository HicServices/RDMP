// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ReusableLibraryCode;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Watches tables in the database for changes in order to determine whether old values/objects can be used for a given collection in
    /// </summary>
    public class SqlDependencyTableMonitor
    {
        static object oSubscriptionsLock = new object();
        static HashSet<string> _subscriptions = new HashSet<string>();

        private readonly Dictionary<Type,SqlDependency> _registeredDependencies = new Dictionary<Type, SqlDependency>();
        private readonly Dictionary<Type,object>  _cachedAnswers = new Dictionary<Type, object>();

        public bool HasValidCache<T>()
        {
            return _registeredDependencies.ContainsKey(typeof(T)) && !_registeredDependencies[typeof(T)].HasChanges;
        }

        public T[] RegisterTableMonitor<T>(TableRepository repository, Func<SqlDataReader,T> func)
        {
            lock (oSubscriptionsLock)
            {
                if (!_subscriptions.Contains(repository.ConnectionString))
                {
                    SqlDependency.Start(repository.ConnectionString);
                    _subscriptions.Add(repository.ConnectionString);
                }
            }
            
            var t = typeof (T);
            List<T> toReturn = new List<T>();
            
            using (var con = repository.GetConnection())
            {
                string columns = string.Join(",", TableRepository.GetPropertyInfos(t).Select(p =>"["+ p.Name+"]"));

                using (var cmd = (SqlCommand)DatabaseCommandHelper.GetCommand("SELECT " + columns + " FROM dbo." + t.Name, con.Connection,con.Transaction))
                {
                    SqlDependency dependency = new SqlDependency(cmd);
                    
                    if (!_registeredDependencies.ContainsKey(t))
                        _registeredDependencies.Add(t, dependency);
                    else
                        _registeredDependencies[t] = dependency;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            toReturn.Add(func(reader));

                        reader.Close();
                    }
                }
            }

            if(!_cachedAnswers.ContainsKey(t))
                _cachedAnswers.Add(t,null);

            _cachedAnswers[t] = toReturn.ToArray();

            return (T[]) _cachedAnswers[t];
        }

        public T[] GetCached<T>()
        {
            return (T[])_cachedAnswers[typeof(T)];
        }

        public static void Stop()
        {
            lock (oSubscriptionsLock)
                foreach (var subscription in _subscriptions)
                    SqlDependency.Stop(subscription);
        }
    }
}