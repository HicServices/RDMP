using System.Collections.Generic;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.Data
{
    /// <summary>
    /// String based properties that are configured once per Data Export Database.  This includes how to implement Hashing and any text to appear in the Release 
    /// Document that is provided to researchers (and anything else we might want to configure globally for extraction in future).
    /// 
    /// <para>Values are stored in the ConfigurationProperties table in the Data Export Database.</para>
    /// </summary>
    public class ConfigurationProperties
    {
        public enum ExpectedProperties
        {
            HashingAlgorithmPattern,
            ReleaseDocumentDisclaimer
        }

        private readonly bool _allowCaching;
        private readonly IRepository _repository;
        private bool _cacheOutOfDate = true;
        private readonly Dictionary<string,string> _cacheDictionary = new Dictionary<string, string>();

        public ConfigurationProperties(bool allowCaching, IRepository repository)
        {
            _allowCaching = allowCaching;
            _repository = repository;
        }

        public string GetValue(string property)
        {

            //if we do not allow caching then we effectively pull all values every time.  We also pull every value if cache has not been built yet (is out of date)
            if (!_allowCaching || _cacheOutOfDate)
                RefreshCache();

            if (_cacheDictionary.ContainsKey(property))
                return _cacheDictionary[property];

            throw new KeyNotFoundException("Could not find property called " + property + " in ConfigurationProperties table in Data Export database");
        }
        //overload
        public string GetValue(ExpectedProperties property)
        {
            return GetValue(property.ToString());
        }

        //wrapped in exception
        public string TryGetValue(string property)
        {
            try
            {
                return GetValue(property);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        //overload
        public string TryGetValue(ExpectedProperties property)
        {
            string value = TryGetValue(property.ToString());

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }

      
        //overload
        public void SetValue(ExpectedProperties property, string value)
        {
            SetValue(property.ToString(), value);
        }

        public void SetValue(string property, string value)
        {
            if(_cacheOutOfDate)
                RefreshCache();

            if (_cacheDictionary.ContainsKey(property))
                IssueUpdateCommand(property, value);
            else
                IssueInsertCommand(property, value);
            
            //a value has been set, reset cache (we could update cache manually in memory but a round trip to database is safer)
            _cacheOutOfDate = true;
        }

        public int DeleteValue(string property)
        {
            return IssueDeleteCommand(property);
        }

        #region read/write to database
        private int IssueDeleteCommand(string property)
        {
            return _repository.Delete("DELETE FROM [ConfigurationProperties] WHERE Property=@property",
                new Dictionary<string, object>
                {
                    {"property", property}
                });
        }

        private void IssueInsertCommand(string property, string value)
        {
            _repository.Insert("INSERT INTO [ConfigurationProperties](Property,Value) VALUES (@property,@value)",
                new Dictionary<string, object>
                {
                    {"value", value},
                    {"property", property}
                });
        }

        private void IssueUpdateCommand(string property, string value)
        {
            _repository.Update("UPDATE [ConfigurationProperties] set Value=@value where Property=@property",
                new Dictionary<string, object>
                {
                    {"value", value},
                    {"property", property}
                });
        }

        private void RefreshCache()
        {
            var repo = (TableRepository)_repository;
            using (var con = repo.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand("SELECT * from [ConfigurationProperties]",con.Connection, con.Transaction);
                using (var reader = cmd.ExecuteReader())
                {
                    _cacheDictionary.Clear();

                    //get cache of all answers
                    while (reader.Read())
                        _cacheDictionary.Add(reader["Property"].ToString(), reader["Value"] as string);
                }
            }

            _cacheOutOfDate = false;
        }
        #endregion
    }
}
