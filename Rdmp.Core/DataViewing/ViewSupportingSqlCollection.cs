using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using ReusableLibraryCode.DataAccess;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.DataViewing
{
    class ViewSupportingSqlCollection : PersistableObjectCollection, IViewSQLAndResultsCollection
    {

        public SupportingSQLTable SupportingSQLTable { get => DatabaseObjects.OfType<SupportingSQLTable>().FirstOrDefault(); }

        public ViewSupportingSqlCollection(SupportingSQLTable supportingSql)
        {
            DatabaseObjects.Add(supportingSql);
        }

        /// <summary>
        /// Persistence constructor 
        /// </summary>
        public ViewSupportingSqlCollection()
        {

        }

        public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
        {
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            return SupportingSQLTable.ExternalDatabaseServer;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var syntax = SupportingSQLTable.ExternalDatabaseServer?.DatabaseType ?? FAnsi.DatabaseType.MicrosoftSQLServer;
            return new QuerySyntaxHelperFactory().Create(syntax);
        }

        public string GetSql()
        {
            return SupportingSQLTable.SQL;
        }

        public string GetTabName()
        {
            return SupportingSQLTable.Name;
        }

        public IEnumerable<DatabaseEntity> GetToolStripObjects()
        {
            yield return SupportingSQLTable;
        }
    }
}
