using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Stores DbCommands for saving IMapsDirectlyToDatabaseTable objects to the database.
    /// 
    /// <para>Each time a novel IMapsDirectlyToDatabaseTable object Type is encountered an UPDATE sql command (DbCommand) is created for saving the object back to
    /// the database (using DbCommandBuilder).  Since this operation (figuring out the UPDATE command) is slow and we might be saving lots of objects we cache
    /// the command so that we can apply it to all objects of that Type as they are saved.</para>
    /// </summary>
    public class UpdateCommandStore
    {
        public Dictionary<Type, DbCommand> UpdateCommands { get; private set; }

        public UpdateCommandStore()
        {
            UpdateCommands = new Dictionary<Type, DbCommand>();
        }

        public DbCommand this[IMapsDirectlyToDatabaseTable o]
        {
            get
            {
                return UpdateCommands[o.GetType()];
            }
        }
        public DbCommand this[Type t]
        {
            get
            {
                return UpdateCommands[t];
            }
        }
        public void Add(Type o, DbConnectionStringBuilder builder, DbConnection connection, DbTransaction transaction)
        {
            var cmd = DatabaseCommandHelper.GetCommand("SELECT * FROM " + o.Name + " WHERE ID=0", connection, transaction);
            var cmdbuilder = new DiscoveredServer(builder).Helper.GetCommandBuilder(cmd);
            
            DbCommand cmdUpdate = cmdbuilder.GetUpdateCommand(true);

            //throw away addititional UPDATE crud and add WHERE ID=@ID 
            string adjustedcmd = cmdUpdate.CommandText;
            adjustedcmd = adjustedcmd.Substring(0, adjustedcmd.IndexOf("WHERE"));
            adjustedcmd += "WHERE ID=@ID;";

            cmdUpdate.CommandText = adjustedcmd;
            AdjustUpdateCommand(o, cmdUpdate);

            UpdateCommands.Add(o, cmdUpdate);
        }

        /// <summary>
        /// Removes unwanted _Orig values for the Where statement.  All properties in the destination database must be named the same as in this data class so that reflection can be used to 
        /// fill in the command.  The only WHERE condition after running this method is 'WHERE ID = @ID'
        /// </summary>
        private void AdjustUpdateCommand(Type objectType, DbCommand command)
        {
            List<DbParameter> toDiscard = new List<DbParameter>();

            foreach (DbParameter p in command.Parameters)
            {
                PropertyInfo prop = objectType.GetProperty(p.ParameterName.Trim('@'));

                //set parameters that exist
                if (prop == null)
                {
                    //if it is a set param
                    if (p.ParameterName.StartsWith("@Original_") || p.ParameterName.StartsWith("@IsNull") || p.ParameterName.Equals("@hic_validFrom"))
                        toDiscard.Add(p);
                    else
                        throw new Exception("Column " + p.ParameterName + " was present in UPDATE command but was not found in the data class " + objectType.Name);
                }
            }

            foreach (DbParameter p in toDiscard)
                command.Parameters.Remove(p);

            if (!command.Parameters.Contains("@ID"))
                DatabaseCommandHelper.AddParameterWithValueToCommand("@ID", command, null);       
        }

        public bool ContainsKey(IMapsDirectlyToDatabaseTable toCreate)
        {
            return UpdateCommands.ContainsKey(toCreate.GetType());
        }
        public bool ContainsKey(Type toCreate)
        {
            return UpdateCommands.ContainsKey(toCreate);
        }

        public void Clear()
        {
            UpdateCommands.Clear();
        }
    }
}
