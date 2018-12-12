using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    public abstract class ContainerMenu : RDMPContextMenuStrip
    {
        private readonly IContainer _container;
        private IFilter[] _importableFilters;

        protected ContainerMenu(IFilterFactory factory,RDMPContextMenuStripArgs args, IContainer container) : base(args, container)
        {
            _container = container;

            string operationTarget = container.Operation == FilterContainerOperation.AND ? "OR" : "AND";

            Items.Add("Set Operation to " + operationTarget, null, (s, e) => FlipContainerOperation());

            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewFilter(args.ItemActivator,factory,_container));
            Add(new ExecuteCommandCreateNewFilterFromCatalogue(args.ItemActivator, container));
            
            Items.Add(new ToolStripSeparator());
            Items.Add("Add SubContainer", _activator.CoreIconProvider.GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddSubcontainer());

        }
        private void FlipContainerOperation()
        {
            _container.Operation = _container.Operation == FilterContainerOperation.AND
                ? FilterContainerOperation.OR
                : FilterContainerOperation.AND;

            _container.SaveToDatabase();
            Publish((DatabaseEntity)_container);
        }

        private void AddSubcontainer()
        {
            var newContainer = GetNewFilterContainer();
            _container.AddChild(newContainer);
            Publish((DatabaseEntity)_container);
        }

        protected abstract IContainer GetNewFilterContainer();
    }
}