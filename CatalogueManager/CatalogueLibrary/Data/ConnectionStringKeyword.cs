using System;
using System.Collections.Generic;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.ConnectionStringDefaults;

namespace CatalogueLibrary.Data
{
    public class ConnectionStringKeyword : DatabaseEntity, INamed, ICheckable
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

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                var accumulator = new ConnectionStringKeywordAccumulator(DatabaseType);
                accumulator.AddOrUpdateKeyword(Name, Value, ConnectionStringKeywordPriority.SystemDefaultLow);
                notifier.OnCheckPerformed(new CheckEventArgs("Integrity of keyword is ok according to ConnectionStringKeywordAccumulator", CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
            }
        }
    }
}
