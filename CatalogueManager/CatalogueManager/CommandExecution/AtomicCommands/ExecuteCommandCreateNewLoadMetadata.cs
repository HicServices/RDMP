using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewLoadMetadata : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private Catalogue[] _availableCatalogues;
        private Catalogue _catalogue;

        public ExecuteCommandCreateNewLoadMetadata(IActivateItems activator):base(activator)
        {
            _availableCatalogues = activator.CoreChildProvider.AllCatalogues.Where(c => c.LoadMetadata_ID == null).ToArray();
            
            if(!_availableCatalogues.Any())
                SetImpossible("There are no Catalogues that are not associated with another Load already");
        }

        public override string GetCommandHelp()
        {
            return "Create a new data load configuration for loading data into a given set of datasets through RAW=>STAGING=>LIVE migration / adjustment";
        }

        public override void Execute()
        {
            base.Execute();

            var catalogueBefore = _catalogue;
            try
            {
                //if we don't have an explicit one picked yet
                if (_catalogue == null)
                    if(!SelectOne(_availableCatalogues, out _catalogue)) //get user to pick one
                        return; //user cancelled
            
                //create the load
                var cataRepository = (CatalogueRepository)_catalogue.Repository;

                var lmd = new LoadMetadata(cataRepository, "Loading " + _catalogue.Name);

                lmd.EnsureLoggingWorksFor(_catalogue);

                _catalogue.LoadMetadata_ID = lmd.ID;
                _catalogue.SaveToDatabase();

                Publish(lmd);

                Activator.WindowArranger.SetupEditAnything(this,lmd);
                
            }
            finally
            {
                _catalogue = catalogueBefore;
            }
        }


        public override string GetCommandName()
        {
            return "Create New Data Load Configuration";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.LoadMetadata, OverlayKind.Add);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }
    }
}