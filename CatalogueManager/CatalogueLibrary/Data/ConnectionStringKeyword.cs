using System;
using System.Collections.Generic;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    public class ConnectionStringKeyword : DatabaseEntity, INamed
    {
        #region Database Properties

        private DatabaseType _databaseType;
        private string _name;
        private string _value;
        #endregion

        public DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set { SetField(ref _databaseType, value); }
        }
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }
        public string Value
        {
            get { return _value; }
            set { SetField(ref _value, value); }
        }
        public ConnectionStringKeyword(IRepository repository,DatabaseType databaseType, string keyword, string value)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"DatabaseType",databaseType},
                {"Name",keyword},
                {"Value",value},
            });

            if (ID == 0 || Repository != repository)
                throw new ArgumentException("Repository failed to properly hydrate this class");
        }
        public ConnectionStringKeyword(IRepository repository, DbDataReader r)
            : base(repository, r)
        {
            DatabaseType = (DatabaseType) Enum.Parse(typeof(DatabaseType),r["DatabaseType"].ToString());
            Name = r["Name"].ToString();
            Value = r["Value"] as string;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
