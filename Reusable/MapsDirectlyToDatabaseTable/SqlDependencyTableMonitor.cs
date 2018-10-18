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