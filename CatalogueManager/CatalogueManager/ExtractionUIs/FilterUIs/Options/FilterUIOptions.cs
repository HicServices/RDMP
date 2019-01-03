using CatalogueLibrary.Data;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public abstract class FilterUIOptions
    {
        protected IFilter Filter;

        protected FilterUIOptions(IFilter filter)
        {
            Filter = filter;
        }

        public abstract ITableInfo[] GetTableInfos();
        public abstract ISqlParameter[] GetGlobalParametersInFilterScope();
        public abstract IColumn[] GetIColumnsInFilterScope();
    }
}
