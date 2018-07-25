using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAssociateCatalogueWithLoadMetadata:BasicUICommandExecution,IAtomicCommand
    {
        private readonly LoadMetadata _loadMetadata;
        private Catalogue[] _availableCatalogues;

        public ExecuteCommandAssociateCatalogueWithLoadMetadata(IActivateItems activator, LoadMetadata loadMetadata) : base(activator)
        {
            _loadMetadata = loadMetadata;

            _availableCatalogues = Activator.CoreChildProvider.AllCatalogues.Where(c => c.LoadMetadata_ID == null).ToArray();
            
            if(!_availableCatalogues.Any())
                SetImpossible("There are no Catalogues that are not associated with another Load already");
        }

        public override string GetCommandHelp()
        {
            return "Specifies that the table(s) underlying the dataset are loaded by the load configuration.  The union of all catalogue(s) table(s) will be used for RAW=>STAGING=>LIVE migration during DLE execution";
        }

        public override void Execute()
        {
            base.Execute();

            Catalogue cata;

            if (!SelectOne(_availableCatalogues,out cata))
                return;
            
            //Ensure logging task is correct
            var otherCatalogues = _loadMetadata.GetAllCatalogues().ToArray();

            //if there are other catalogues
            if (otherCatalogues.Any())
            {
                var tasks = otherCatalogues.Select(c => c.LoggingDataTask).Distinct().ToArray();
                //if the other catalogues have an agreed logging task
                if (tasks.Length == 1)
                {
                    string task = tasks.Single();

                    //and that logging task is not blank!, and differs from this Catalogue
                    if (!string.IsNullOrWhiteSpace(task) && !task.Equals(cata.LoggingDataTask))
                    {
                        var liveServers = otherCatalogues.Where(c => c.LiveLoggingServer_ID != null).Select(c => c.LiveLoggingServer_ID).Distinct().ToArray();

                        //AND if there is agreement on what logging server to use!
                        if (liveServers.Count() <= 1)
                        {
                            //if there is no current logging task for the Catalogue
                            if(string.IsNullOrWhiteSpace(cata.LoggingDataTask)

                            //or if the user wants to switch to the new one
                            || MessageBox.Show("Do you want to set Catalogue '" + cata.Name + "' to use shared logging task '" + task + "' isntead of it's current Logging Task '"+cata.LoggingDataTask+"' (All Catalogues in a load must share the same task and logging servers)?", "Synchronise Logging Tasks", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                //switch Catalogue to use that logging task (including servers)
                                cata.LoggingDataTask = task;
                                cata.LiveLoggingServer_ID = liveServers.SingleOrDefault();
                                cata.SaveToDatabase();
                            }
                        }
                    }
                }
            }

            //associate them
            cata.LoadMetadata_ID = _loadMetadata.ID;
            cata.SaveToDatabase();
            Publish(_loadMetadata);
            
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
        }
    }
}
