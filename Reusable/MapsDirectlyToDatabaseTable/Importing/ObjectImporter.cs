using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Utilities;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace MapsDirectlyToDatabaseTable.Importing
{
    public class ObjectImporter
    {
        private readonly TableRepository _targetRepository;

        public ObjectImporter(TableRepository targetRepository)
        {
            _targetRepository = targetRepository;
        }

        public IMapsDirectlyToDatabaseTable ImportObject(Type t, Dictionary<string, object> properties)
        {
            if(!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t))
                throw new ArgumentEmptyException("Type must be IMapsDirectlyToDatabaseTable");

            using (var con = _targetRepository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM " + t.Name, con.Connection);
                var cmdbuilder = new DiscoveredServer(_targetRepository.ConnectionStringBuilder).Helper.GetCommandBuilder(cmd);

                DbCommand cmdInsert = cmdbuilder.GetInsertCommand(true);
                cmdInsert.CommandText += ";SELECT @@IDENTITY;";

                _targetRepository.PrepareCommand(cmdInsert, properties);

                var id = Convert.ToInt32(cmdInsert.ExecuteScalar());

                return _targetRepository.GetObjectByID(t, id);
            }
        }
    }
}
