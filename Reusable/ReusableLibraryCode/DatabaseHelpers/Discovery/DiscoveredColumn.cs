using System;
using System.Runtime.Remoting.Messaging;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableLibraryCode.DatabaseHelpers.Discovery.TypeTranslation;

namespace ReusableLibraryCode.DatabaseHelpers.Discovery
{
    /// <summary>
    /// Cross database type reference to a Column in a Table
    /// </summary>
    public class DiscoveredColumn : IHasFullyQualifiedNameToo,ISupplementalColumnInformation
    {
        public IDiscoveredColumnHelper Helper;
        public DiscoveredTable Table { get; private set; }

        public bool AllowNulls { get; private set; }
        private readonly string _name;
        private readonly IQuerySyntaxHelper _querySyntaxHelper;

        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string Collation { get; set; }


        public DiscoveredColumn(DiscoveredTable table, string name,bool allowsNulls)
        {
            Table = table;
            Helper = table.Helper.GetColumnHelper();

            _name = name;
            _querySyntaxHelper = table.Database.Server.GetQuerySyntaxHelper();
            AllowNulls = allowsNulls;
        }

        public string GetRuntimeName()
        {
            return _querySyntaxHelper.GetRuntimeName(_name);
        }

        public string GetFullyQualifiedName()
        {
            return _querySyntaxHelper.EnsureFullyQualified(Table.Database.GetRuntimeName(),Table.Schema, Table.GetRuntimeName(), GetRuntimeName(), Table is DiscoveredTableValuedFunction);
        }


        public DiscoveredDataType DataType { get; set; }
        public string Format { get; set; }
        
        public string GetTopXSql(int topX, bool discardNulls)
        {
            return Helper.GetTopXSqlForColumn(Table.Database, Table, this, topX, discardNulls);
        }

        public override string ToString()
        {
            return _name;
        }

        public DataTypeComputer GetDataTypeComputer()
        {
            return Table.GetQuerySyntaxHelper().TypeTranslater.GetDataTypeComputerFor(this);
        }
    }
}