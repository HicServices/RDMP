using System.Collections.Generic;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.AutoComplete;
using FAnsi.Discovery.QuerySyntax;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataViewing.Collections
{
    public interface IViewSQLAndResultsCollection:IPersistableObjectCollection, IHasQuerySyntaxHelper
    {
        IEnumerable<DatabaseEntity> GetToolStripObjects();

        IDataAccessPoint GetDataAccessPoint();
        string GetSql();
        string GetTabName();
        void AdjustAutocomplete(AutoCompleteProvider autoComplete);
    }
}
