using CatalogueLibrary.Data;

namespace CatalogueManager.ExtractionUIs.FilterUIs.Options
{
    public class ExtractionFilterUIOptions : FilterUIOptions
    {
        private ISqlParameter[] _globals;
        private TableInfo[] _tables;
        private IColumn[] _columns;

        public ExtractionFilterUIOptions(ExtractionFilter masterCatalogueFilter) : base(masterCatalogueFilter)
        {
            var c = masterCatalogueFilter.ExtractionInformation.CatalogueItem.Catalogue;

            _globals = masterCatalogueFilter.GetColumnInfoIfExists().TableInfo.GetAllParameters();
            _tables = c.GetTableInfoList(false);
            _columns = c.GetAllExtractionInformation(ExtractionCategory.Any);

        }

        public override TableInfo[] GetTableInfos()
        {
            return _tables;
        }

        public override ISqlParameter[] GetGlobalParametersInFilterScope()
        {
            return _globals;
        }

        public override IColumn[] GetIColumnsInFilterScope()
        {
            return _columns;
        }
    }
}