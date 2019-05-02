// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using FAnsi.Discovery;
using ReusableLibraryCode;

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
