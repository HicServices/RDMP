using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewCatalogueExtractionSql:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

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
