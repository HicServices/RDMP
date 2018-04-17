using System;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// The count(*) column in an AggregateConfiguration, this is used by AggregateBuilder.  This can be any aggregate function such as 'sum', 'avg' etc.
    /// </summary>
    public class AggregateCountColumn:IColumn
    {
        private IQuerySyntaxHelper _syntaxHelper;
        private readonly string _sql;

        public AggregateCountColumn(string sql)
        {
            _sql = sql;
        }

        public void SetQuerySyntaxHelper(IQuerySyntaxHelper syntaxHelper, bool ensureAliasExists)
        {
            _syntaxHelper = syntaxHelper;
            string select;
            string alias;

            //if alias exists
            if (_syntaxHelper.SplitLineIntoSelectSQLAndAlias(_sql, out select, out alias))
                Alias = alias; //use the users explicit alias
            else
                Alias = ensureAliasExists?"MyCount":null;//set an alias of MyCount

            SelectSQL = select;

        }

        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        public string GetFullSelectLineStringForSavingIntoAnAggregate()
        {
            if (string.IsNullOrWhiteSpace(Alias))
                return SelectSQL;

            return SelectSQL + _syntaxHelper.AliasPrefix + Alias;
        }

        public ColumnInfo ColumnInfo { get { return null; } }
        public int Order { get; set; }

        [Sql]
        public string SelectSQL { get; set; }
        public int ID { get { return -1; }}

        public string Alias{get; private set; }
        public bool HashOnDataRelease { get { return false; }}
        public bool IsExtractionIdentifier { get { return false; } }
        public bool IsPrimaryKey { get { return false; } }
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}
