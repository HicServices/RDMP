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

        /// <summary>
        /// The default alias for unamed count columns
        /// </summary>
        public const string DefaultAliasName = "MyCount";

        /// <summary>
        /// Creates a new Aggregate Function (count / max etc) with the given line of SELECT SQL
        /// <para>Can include aliases e.g. count(*) as MyCount</para>
        /// </summary>
        /// <param name="sql"></param>
        public AggregateCountColumn(string sql)
        {
            _sql = sql;
        }


        /// <summary>
        /// Initializes the <see cref="IQuerySyntaxHelper"/> for the column and optionally ensures that it has an alias.  If no <see cref="Alias"/> has
        /// been specified or was found in the current sql then <see cref="DefaultAliasName"/> is set.
        /// </summary>
        /// <param name="syntaxHelper"></param>
        /// <param name="ensureAliasExists"></param>
        public void SetQuerySyntaxHelper(IQuerySyntaxHelper syntaxHelper, bool ensureAliasExists)
        {
            _syntaxHelper = syntaxHelper;
            string select;
            string alias;

            //if alias exists
            if (_syntaxHelper.SplitLineIntoSelectSQLAndAlias(_sql, out select, out alias))
                Alias = alias; //use the users explicit alias
            else
                Alias = ensureAliasExists ? DefaultAliasName : null;//set an alias of MyCount

            SelectSQL = select;

        }
        /// <inheritdoc/>
        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        /// <summary>
        /// Combines the <see cref="SelectSQL"/> with the <see cref="Alias"/> for use in SELECT Sql
        /// </summary>
        /// <returns></returns>
        public string GetFullSelectLineStringForSavingIntoAnAggregate()
        {
            if (string.IsNullOrWhiteSpace(Alias))
                return SelectSQL;

            return SelectSQL + _syntaxHelper.AliasPrefix + Alias;
        }

        /// <inheritdoc/>
        public ColumnInfo ColumnInfo { get { return null; } }

        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        [Sql]
        public string SelectSQL { get; set; }
        
        /// <inheritdoc/>
        public int ID { get { return -1; }}

        /// <inheritdoc/>
        public string Alias{get; private set; }

        /// <inheritdoc/>
        public bool HashOnDataRelease { get { return false; }}

        /// <inheritdoc/>
        public bool IsExtractionIdentifier { get { return false; } }

        /// <inheritdoc/>
        public bool IsPrimaryKey { get { return false; } }

        /// <inheritdoc/>
        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}
