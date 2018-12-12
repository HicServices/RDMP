using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.FilterImporting.Construction;

namespace CatalogueManager.Menus
{
    class AggregateFilterContainerMenu : ContainerMenu
    {

        public AggregateFilterContainerMenu(RDMPContextMenuStripArgs args, AggregateFilterContainer filterContainer)
            : base(
            new AggregateFilterFactory(args.ItemActivator.RepositoryLocator.CatalogueRepository),
            args, filterContainer)
        {

        }

        protected override IContainer GetNewFilterContainer()
        {
            return new AggregateFilterContainer(RepositoryLocator.CatalogueRepository,FilterContainerOperation.AND);
        }
    }
}