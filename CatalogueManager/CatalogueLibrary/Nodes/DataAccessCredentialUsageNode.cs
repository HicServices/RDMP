using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Tree Node for documenting the allowed usage of a specific DataAccessCredentials (username / password) under a given DataAccessContext (loading, extracting etc).
    /// </summary>
    public class DataAccessCredentialUsageNode
    {
        public DataAccessCredentials Credentials { get; private set; }
        public TableInfo TableInfo { get; private set; }
        public DataAccessContext Context { get; private set; }

        public DataAccessCredentialUsageNode(DataAccessCredentials credentials,TableInfo tableInfo,DataAccessContext context)
        {
            Credentials = credentials;
            TableInfo = tableInfo;
            Context = context;
        }

        public override string ToString()
        {
            return Credentials.ToString() + " (Under Context:" + Context + ")";
        }

        protected bool Equals(DataAccessCredentialUsageNode other)
        {
            return Equals(Credentials, other.Credentials) && Equals(TableInfo, other.TableInfo) && Context == other.Context;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataAccessCredentialUsageNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Credentials != null ? Credentials.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (TableInfo != null ? TableInfo.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Context;
                return hashCode;
            }
        }
    }
}
