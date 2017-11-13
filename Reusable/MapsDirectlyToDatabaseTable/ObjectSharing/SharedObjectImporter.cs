using System;
using System.Data.Common;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace MapsDirectlyToDatabaseTable.ObjectSharing
{
    public class SharedObjectImporter
    {
        public TableRepository TargetRepository { get; private set; }

        public SharedObjectImporter(TableRepository targetRepository)
        {
            TargetRepository = targetRepository;
        }

        public IMapsDirectlyToDatabaseTable ImportObject(MapsDirectlyToDatabaseTableStatelessDefinition definition)
        {
            using (var con = TargetRepository.GetConnection())
            {
                var cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM " + definition.Type.Name, con.Connection);
                var cmdbuilder = new DiscoveredServer(TargetRepository.ConnectionStringBuilder).Helper.GetCommandBuilder(cmd);

                DbCommand cmdInsert = cmdbuilder.GetInsertCommand(true);
                cmdInsert.CommandText += ";SELECT @@IDENTITY;";

                TargetRepository.PrepareCommand(cmdInsert, definition.Properties);

                var id = Convert.ToInt32(cmdInsert.ExecuteScalar());

                return TargetRepository.GetObjectByID(definition.Type, id);
            }
        }
    }
}
