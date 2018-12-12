using CatalogueLibrary.Data;
using DataExportLibrary.Data.DataTables;

namespace CatalogueManager.Menus
{
    class FilterContainerMenu : ContainerMenu
    {
        private readonly FilterContainer _filterContainer;
        private ExtractionFilter[] _importableFilters;

        public FilterContainerMenu(RDMPContextMenuStripArgs args, FilterContainer filterContainer)
            : base(
            new DeployedExtractionFilterFactory(args.ItemActivator.RepositoryLocator.DataExportRepository),
            args, filterContainer)
        {
            
        }

        protected override IContainer GetNewFilterContainer()
        {
            return new FilterContainer(RepositoryLocator.DataExportRepository);
        }
    }
}