using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewCatalogueExtractionSql:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        [ImportingConstructor]
        public ExecuteCommandViewCatalogueExtractionSql(IActivateItems activator,Catalogue catalogue): base(activator)
        {
            _catalogue = catalogue;
        }

        public ExecuteCommandViewCatalogueExtractionSql(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            
            //if the catalogue has no extractable columns
            if(!_catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Any())
                SetImpossible("Catalogue has no ExtractionInformations");

            return this;
        }

        public override void Execute()
        {
            Activator.Activate<ViewExtractionSql, Catalogue>(_catalogue);
        }
    }
}
