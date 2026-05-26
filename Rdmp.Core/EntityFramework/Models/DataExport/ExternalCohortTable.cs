using FAnsi;
using FAnsi.Connections;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.EntityFramework.Helpers;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.EntityFramework.Models.DataExport
{
    public class ExternalCohortTable: DatabaseObject, IExternalCohortTable
    {

        public string TableName { get; set; }
        public string PrivateIdentifierField { get; set; }
        public string DefinitionTableName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ReleaseIdentifierField { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string DefinitionTableForeignKeyField { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Server { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Database { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DatabaseType DatabaseType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Check(ICheckNotifier notifier)
        {
            throw new NotImplementedException();
        }

        public DiscoveredDatabase Discover()
        {
            throw new NotImplementedException();
        }

        public DiscoveredTable DiscoverCohortTable()
        {
            throw new NotImplementedException();
        }

        public DiscoveredTable DiscoverDefinitionTable()
        {
            throw new NotImplementedException();
        }

        public DiscoveredColumn DiscoverDefinitionTableForeignKey()
        {
            throw new NotImplementedException();
        }

        public bool DiscoverExistence(DataAccessContext context, out string reason)
        {
            throw new NotImplementedException();
        }

        public DiscoveredColumn DiscoverPrivateIdentifier()
        {
            throw new NotImplementedException();
        }

        public DiscoveredColumn DiscoverReleaseIdentifier()
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            throw new NotImplementedException();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            throw new NotImplementedException();
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            throw new NotImplementedException();
        }

        public RevertableObjectReport HasLocalChanges()
        {
            throw new NotImplementedException();
        }

        public bool IDExistsInCohortTable(int originID)
        {
            throw new NotImplementedException();
        }

        public void PushToServer(ICohortDefinition newCohortDefinition, IManagedConnection connection)
        {
            throw new NotImplementedException();
        }

        public void RevertToDatabaseState()
        {
            throw new NotImplementedException();
        }

    }
}
