using System.Drawing;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewFilterFromCatalogue : BasicUICommandExecution, IAtomicCommand
    {
        private readonly IContainer _container;
        private ExtractionFilter[] _filters;

        public ExecuteCommandCreateNewFilterFromCatalogue(IActivateItems itemActivator, IContainer container):base(itemActivator)
        {
            _container = container;
            var catalogue = container.GetCatalogueIfAny();

            if(catalogue == null)
            {
                SetImpossible("No Catalogue found for filter container:" + container);
                return;
            }

            _filters = catalogue.GetAllFilters();

            if(!_filters.Any())
                SetImpossible("There are Filters declard in Catalogue '" + catalogue +"'");
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Filter, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var wizard = new FilterImportWizard();
            var import = wizard.ImportOneFromSelection(_container, _filters);

            if (import != null)
            {
                _container.AddChild(import);
                Publish((DatabaseEntity) import);
                Activate((DatabaseEntity)import);
            }
        }
    }
}