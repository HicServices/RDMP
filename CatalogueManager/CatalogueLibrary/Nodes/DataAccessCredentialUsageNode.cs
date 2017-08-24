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
    }
}
