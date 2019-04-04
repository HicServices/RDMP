// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using FAnsi.Discovery;
using FAnsi.Implementation;
using ReusableLibraryCode.Exceptions;

namespace ReusableLibraryCode.DataAccess
{
    /// <summary>
    /// Translation class for converting IDataAccessPoints into DiscoveredServer / DiscoveredDatabase / ConnectionStrings etc.  IDataAccessPoints are named 
    /// servers/databases which might have usernames/passwords associated with them (or might use Integrated Security).  Each IDataAccessPoint can have multiple 
    /// credentials that can be used with it depending on the DataAccessContext.  Therefore when using the DataAccessPortal you always have to specify the 
    /// Context of the activity you are doing e.g. DataAccessContext.DataLoad.
    /// </summary>
    public class DataAccessPortal
    {
        private static readonly object oLockInstance = new object();
        private static DataAccessPortal _instance;

        public static DataAccessPortal GetInstance()
        {
            lock (oLockInstance)
            {
                if (_instance == null)
                    _instance = new DataAccessPortal();
            }
            return _instance;
        }

        private DataAccessPortal()
        {
            
        }

        public DiscoveredServer ExpectServer(IDataAccessPoint dataAccessPoint, DataAccessContext context, bool setInitialDatabase=true)
        {
            var builder = GetConnectionStringBuilder(dataAccessPoint, context,setInitialDatabase);
            return new DiscoveredServer(builder);
        }
        public DiscoveredDatabase ExpectDatabase(IDataAccessPoint dataAccessPoint, DataAccessContext context)
        {
            return ExpectServer(dataAccessPoint, context).ExpectDatabase(dataAccessPoint.GetQuerySyntaxHelper().GetRuntimeName(dataAccessPoint.Database));
        }
        public DiscoveredServer ExpectDistinctServer(IDataAccessPoint[] collection, DataAccessContext context, bool setInitialDatabase)
        {
            var builder = GetDistinctConnectionStringBuilder(collection, context, setInitialDatabase);
            return new DiscoveredServer(builder);
        }

        private DbConnectionStringBuilder GetConnectionStringBuilder(IDataAccessPoint dataAccessPoint, DataAccessContext context, bool setInitialDatabase=true)
        {
            IDataAccessCredentials credentials = dataAccessPoint.GetCredentialsIfExists(context);
            
            if(string.IsNullOrWhiteSpace(dataAccessPoint.Server))
                throw new NullReferenceException("Could not get connection string because Server was null on dataAccessPoint '" + dataAccessPoint +"'");

            return DatabaseCommandHelper.For(dataAccessPoint.DatabaseType).GetConnectionStringBuilder(
                dataAccessPoint.Server,
                setInitialDatabase ? dataAccessPoint.GetQuerySyntaxHelper().GetRuntimeName(dataAccessPoint.Database) : "",
                credentials != null?credentials.Username:null,
                credentials != null ? credentials.GetDecryptedPassword() : null);
        }

        private DbConnectionStringBuilder GetDistinctConnectionStringBuilder(IDataAccessPoint[] collection, DataAccessContext context, bool setInitialDatabase)
        {
            ///////////////////////Exception handling///////////////////////////////////////////////
            if(!collection.Any())
                throw new Exception("No IDataAccessPoints were passed, so no connection string builder can be created");

            IDataAccessPoint first = collection.First();

            //There can be only one - server
            foreach (IDataAccessPoint accessPoint in collection)
            {
                if (!first.Server.Equals(accessPoint.Server))
                    throw new ExpectedIdenticalStringsException("There was a mismatch in server names for data access points " + first + " and " + accessPoint + " server names must match exactly", first.Server, accessPoint.Server);
                
                if(setInitialDatabase)
                {
                    if(string.IsNullOrWhiteSpace(first.Database))
                        throw new Exception("DataAccessPoint '" + first +"' does not have a Database specified on it");

                    var querySyntaxHelper = accessPoint.GetQuerySyntaxHelper();

                    var firstDbName = querySyntaxHelper.GetRuntimeName(first.Database);
                    var currentDbName = querySyntaxHelper.GetRuntimeName(accessPoint.Database);

                    if (!firstDbName.Equals(currentDbName))
                        throw new ExpectedIdenticalStringsException("All data access points must be into the same database, access points '" + first + "' and '" + accessPoint + "' are into different databases", firstDbName, currentDbName);    
                }
            }
            
            //There can be only one - credentials (but there might not be any)
            var credentials = collection.Select(t => t.GetCredentialsIfExists(context)).ToArray();

            //if there are credentials
            if(credentials.Any(c => c != null)) 
                if (credentials.Any(c=>c == null))//all objects in collection must have a credentials if any of them do
                    throw new Exception("IDataAccessPoint collection could not agree whether to use Credentials or not "+Environment.NewLine
                        +"Objects wanting to use Credentials" + string.Join(",",collection.Where(c=>c.GetCredentialsIfExists(context)!=null).Select(s=>s.ToString())) + Environment.NewLine
                        + "Objects not wanting to use Credentials" + string.Join(",", collection.Where(c => c.GetCredentialsIfExists(context) == null).Select(s => s.ToString())) + Environment.NewLine
                        );
                else
                //There can be only one - Username
                if(credentials.Select(c=>c.Username).Distinct().Count() != 1)
                    throw new Exception("IDataAccessPoint collection could not agree on a single Username to use to access the data under context " + context + " (Servers were " + string.Join("," + Environment.NewLine, collection.Select(c => c + " = " + c.Database + " - " + c.DatabaseType)) + ")");
                else
                //There can be only one - Password
                if(credentials.Select(c=>c.GetDecryptedPassword()).Distinct().Count() != 1)
                    throw new Exception("IDataAccessPoint collection could not agree on a single Password to use to access the data under context " + context + " (Servers were " + string.Join("," + Environment.NewLine, collection.Select(c => c + " = " + c.Database + " - " + c.DatabaseType)) + ")");
                
                
                
            ///////////////////////////////////////////////////////////////////////////////

            //the bit that actually matters:
            return GetConnectionStringBuilder(first, context, setInitialDatabase);
            
        }

    }
}
