using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Watches tables in the database for changes in order to determine whether old values/objects can be used for a given collection in
    /// </summary>
    public class SqlDependencyTableMonitor
    {
        private static bool _started = false;

        private readonly Dictionary<Type,SqlDependency> _registeredDependencies = new Dictionary<Type, SqlDependency>();
        private readonly Dictionary<Type,object>  _cachedAnswers = new Dictionary<Type, object>();

        public bool HasValidCache<T>()
        {
            return _registeredDependencies.ContainsKey(typeof(T)) && !_registeredDependencies[typeof(T)].HasChanges;
        }

        public T[] RegisterTableMonitor<T>(TableRepository repository, Func<SqlDataReader,T> func)
        {
            if (!_started)
            {
                SqlDependency.Start(repository.ConnectionString);
                _started = true;
            }
            var t = typeof (T);
            List<T> toReturn = new List<T>();

            using (var con = (SqlConnection)repository.DiscoveredServer.GetConnection())
            {
                con.Open();

                string columns = string.Join(",", TableRepository.GetPropertyInfos(t).Select(p => p.Name));

                using (var cmd = new SqlCommand("SELECT " + columns + " FROM dbo." + t.Name, con))
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
    }
}