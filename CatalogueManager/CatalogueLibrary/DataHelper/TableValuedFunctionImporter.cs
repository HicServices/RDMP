using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.DataHelper
{
    public class TableValuedFunctionImporter : ITableInfoImporter
    {
        private readonly CatalogueRepository _repository;
        private readonly string _server;
        private readonly string _database;
        private readonly string _tableValuedFunctionName;
        private readonly string _username;
        private readonly string _password;
        private readonly DataAccessContext _usageContext;

        private DiscoveredTableValuedFunction _tableValuedFunction;
        private DiscoveredParameter[] _parameters;


        public TableValuedFunctionImporter(CatalogueRepository repository,string connection, string server, string database, string tableValuedFunctionName, string username = null, string password = null, DataAccessContext usageContext=DataAccessContext.Any)
        {
            _repository = repository;
            _server = server;
            _database = database;
            _tableValuedFunctionName = tableValuedFunctionName;

            _username = string.IsNullOrWhiteSpace(username)?null:username;
            _password = string.IsNullOrWhiteSpace(password) ? null : password;
            _usageContext = usageContext;

            //should be ok because this only does microsoft sql stuff.... for now
            DiscoveredServer discoveredServer = new DiscoveredServer(new SqlConnectionStringBuilder(connection));
            _tableValuedFunction = discoveredServer.ExpectDatabase(_database).ExpectTableValuedFunction(tableValuedFunctionName);

            if(!_tableValuedFunction.Exists())
                throw new Exception("Could not find tableValuedFunction with name '" + _tableValuedFunctionName +"' (.Exists() returned false)");

            _parameters = _tableValuedFunction.DiscoverParameters();

            ParametersCreated = new List<AnyTableSqlParameter>();
        }

        public List<AnyTableSqlParameter> ParametersCreated { get; private set; }

        public void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated)
        {
            string finalName = "[" + _database + "].." + _tableValuedFunctionName + "(";

            foreach (DiscoveredParameter parameter in _parameters)
                finalName += parameter.ParameterName + ",";

            finalName = finalName.Trim(',') + ") AS " + _tableValuedFunctionName;//give it an alias so all the children ColumnInfos can be fully specified
            
            tableInfoCreated = new TableInfo(_repository,finalName);
            tableInfoCreated.Server = _server;
            tableInfoCreated.Database = _database;
            tableInfoCreated.IsTableValuedFunction = true;
            tableInfoCreated.SaveToDatabase();

            columnInfosCreated = CreateColumnInfosBasedOnReturnColumnsOfFunction(tableInfoCreated);


            if(_username != null)
            {
                var credentialsFactory = new DataAccessCredentialsFactory(_repository);
                credentialsFactory.Create(tableInfoCreated, _username, _password, _usageContext);
            }
        }

        public ColumnInfo CreateNewColumnInfo(TableInfo parent, DiscoveredColumn discoveredColumn)
        {
            var toAdd =
                    new ColumnInfo((ICatalogueRepository) parent.Repository,discoveredColumn.GetFullyQualifiedName(),
                            discoveredColumn.DataType.SQLType, parent);

            toAdd.Format = discoveredColumn.Format;
            toAdd.SaveToDatabase();

            return toAdd;
        }

        private ColumnInfo[] CreateColumnInfosBasedOnReturnColumnsOfFunction(TableInfo parent)
        {
            List<ColumnInfo> newColumnInfosToReturn = new List<ColumnInfo>();

            foreach (DiscoveredColumn discoveredColumn in _tableValuedFunction.DiscoverColumns())
            {
                var toAdd = CreateNewColumnInfo(parent, discoveredColumn);
                newColumnInfosToReturn.Add(toAdd);
            }
            
            foreach (DiscoveredParameter discoveredParameter in _tableValuedFunction.DiscoverParameters())
                CreateParameter(parent,discoveredParameter);
                
            return newColumnInfosToReturn.ToArray();
        }

        public void CreateParameter(TableInfo parent, DiscoveredParameter discoveredParameter)
        {
            ParametersCreated.Add(new AnyTableSqlParameter(_repository, parent, GetParamaterDeclarationSQL(discoveredParameter)));
        }


        public string GetParamaterDeclarationSQL(DiscoveredParameter parameter)
        {
            return "DECLARE " + parameter.ParameterName + " AS " + parameter.DataType.SQLType;
        }
   
    }
}
