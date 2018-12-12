using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewFilter : BasicUICommandExecution,IAtomicCommand
    {
        private IFilterFactory _factory;
        private readonly IContainer _container;

        public ExecuteCommandCreateNewFilter(IActivateItems activator, IFilterFactory factory, IContainer container = null):base(activator)
        {
            _factory = factory;
            _container = container;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var f = (DatabaseEntity)_factory.CreateNewFilter("New Filter " + Guid.NewGuid());

            if (_container != null)
                _container.AddChild((IFilter)f);
            
            Publish(f);
            Activate(f);
        }
    }
}