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
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace CatalogueLibrary.DataHelper
{
    /// <summary>
    /// Generates TableInfo entries in the ICatalogueRepository based the Table Valued Function specified on the live database server.  Table Valued Functions are Microsoft
    /// Sql Server specific, they are like Scalar functions except they return data tables.  RDMP supports building Catalogues that refer to Table Valued Functions.  These
    /// act just like regular tables when it comes to aggregates, data extraction etc except that they can have ISqlParameters declared for them.  Table Valued Functions are
    /// really not nice, especailly if they are non deterministic (return different results when given the same parameters), therefore really you should just avoid using them 
    /// if at all possible.
    /// </summary>
    public class TableValuedFunctionImporter : ITableInfoImporter
    {
        private readonly CatalogueRepository _repository;
        private readonly string _server;
        private readonly string _database;
        private readonly DataAccessContext _usageContext;
        private readonly string _tableValuedFunctionName;

        private readonly DiscoveredTableValuedFunction _tableValuedFunction;
        private DiscoveredParameter[] _parameters;
        private string _schema;

        /// <summary>
        /// List of parameters belonging to the <see cref="DiscoveredTableValuedFunction"/> being imported.  Each parameter will result in an RDMP object <see cref="AnyTableSqlParameter"/> 
        /// which records the default value to send when fetching data etc as well as to facilitate the population of parameters in data extract / cohort generation etc.
        /// </summary>
        public List<AnyTableSqlParameter> ParametersCreated { get; private set; }

        /// <summary>
        /// Prepares to import the given table valued function <paramref name="tableValuedFunction"/> as <see cref="TableInfo"/> / <see cref="ColumnInfo"/> references in the
        /// <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="tableValuedFunction"></param>
        /// <param name="usageContext"></param>
        public TableValuedFunctionImporter(CatalogueRepository repository, DiscoveredTableValuedFunction tableValuedFunction, DataAccessContext usageContext = DataAccessContext.Any)
        {
            _repository = repository;
            _tableValuedFunction = tableValuedFunction;
            _server = _tableValuedFunction.Database.Server.Name;
            _database = _tableValuedFunction.Database.GetRuntimeName();
            _schema = tableValuedFunction.Schema;

            _usageContext = usageContext;

            if (!_tableValuedFunction.Exists())
                throw new Exception("Could not find tableValuedFunction with name '" + _tableValuedFunction.GetRuntimeName() + "' (.Exists() returned false)");

            _tableValuedFunctionName = _tableValuedFunction.GetRuntimeName();
                 
            _parameters = _tableValuedFunction.DiscoverParameters();

            ParametersCreated = new List<AnyTableSqlParameter>();
            
        }


        /// <inheritdoc/>
        public void DoImport(out TableInfo tableInfoCreated, out ColumnInfo[] columnInfosCreated)
        {
            
            string finalName = "[" + _database + "]."+_schema+"." + _tableValuedFunctionName + "(";

            foreach (DiscoveredParameter parameter in _parameters)
                finalName += parameter.ParameterName + ",";

            finalName = finalName.Trim(',') + ") AS " + _tableValuedFunctionName;//give it an alias so all the children ColumnInfos can be fully specified
            
            tableInfoCreated = new TableInfo(_repository,finalName);
            tableInfoCreated.Server = _server;
            tableInfoCreated.Database = _database;
            tableInfoCreated.IsTableValuedFunction = true;
            tableInfoCreated.Schema = _schema;
            tableInfoCreated.SaveToDatabase();

            columnInfosCreated = CreateColumnInfosBasedOnReturnColumnsOfFunction(tableInfoCreated);

            var server = _tableValuedFunction.Database.Server;
            if (server.ExplicitUsernameIfAny != null)
            {
                var credentialsFactory = new DataAccessCredentialsFactory(_repository);
                credentialsFactory.Create(tableInfoCreated, server.ExplicitUsernameIfAny, server.ExplicitPasswordIfAny, _usageContext);
            }
        }

        /// <inheritdoc/>
        public ColumnInfo CreateNewColumnInfo(TableInfo parent, DiscoveredColumn discoveredColumn)
        {
            var toAdd =
                    new ColumnInfo((ICatalogueRepository) parent.Repository,discoveredColumn.GetFullyQualifiedName(),
                            discoveredColumn.DataType.SQLType, parent);

            toAdd.Format = discoveredColumn.Format;
            toAdd.Collation = discoveredColumn.Collation;
            toAdd.IsPrimaryKey = discoveredColumn.IsPrimaryKey;
            toAdd.IsAutoIncrement = discoveredColumn.IsAutoIncrement;
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

        /// <summary>
        /// Creates a new <see cref="AnyTableSqlParameter"/> for describing a parameter of the table valued function <paramref name="parent"/>.  This is public so that
        /// it can be used for later synchronization as well as initial import.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="discoveredParameter"></param>
        internal AnyTableSqlParameter CreateParameter(TableInfo parent, DiscoveredParameter discoveredParameter)
        {
            var created = new AnyTableSqlParameter(_repository, parent, GetParamaterDeclarationSQL(discoveredParameter));
            ParametersCreated.Add(created);
            return created;
        }

        /// <summary>
        /// Creates a parameter declaration SQL for the given <paramref name="parameter"/> e.g. if the parameter is @myVar varchar(10) then the declare SQL might be
        /// DECLARE @myVar as varchar(10);.  
        /// 
        /// <para><seealso cref="IQuerySyntaxHelper.GetParameterDeclaration(string,string)"/></para>
        /// </summary>
        internal string GetParamaterDeclarationSQL(DiscoveredParameter parameter)
        {
            var syntaxHelper = _tableValuedFunction.Database.Server.GetQuerySyntaxHelper();

            return syntaxHelper.GetParameterDeclaration(parameter.ParameterName, parameter.DataType.SQLType);
        }
   
    }
}
