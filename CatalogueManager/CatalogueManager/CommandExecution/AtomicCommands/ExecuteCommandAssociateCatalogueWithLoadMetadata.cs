using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandAssociateCatalogueWithLoadMetadata:BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly LoadMetadata _loadMetadata;
        private Catalogue[] _availableCatalogues;

        public ExecuteCommandAssociateCatalogueWithLoadMetadata(IActivateItems activator, LoadMetadata loadMetadata)
        {
            _activator = activator;
            _loadMetadata = loadMetadata;

            _availableCatalogues = _activator.CoreChildProvider.AllCatalogues.Where(c => c.LoadMetadata_ID == null).ToArray();
            
            if(!_availableCatalogues.Any())
                SetImpossible("There are no Catalogues that are not associated with another Load already");
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_availableCatalogues, false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //associate them
                var cata = dialog.Selected as Catalogue;
                if (cata != null)
                {
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
                                var testServers = otherCatalogues.Where(c => c.TestLoggingServer_ID != null).Select(c => c.TestLoggingServer_ID).Distinct().ToArray();

                                //AND if there is agreement on what logging server to use!
                                if (liveServers.Count() <= 1 && testServers.Count() <= 1)
                                {
                                    
                                    //if there is no current logging task for the Catalogue
                                    if(string.IsNullOrWhiteSpace(cata.LoggingDataTask)

                                    //or if the user wants to switch to the new one
                                    || MessageBox.Show("Do you want to set Catalogue '" + cata.Name + "' to use shared logging task '" + task + "' isntead of it's current Logging Task '"+cata.LoggingDataTask+"' (All Catalogues in a load must share the same task and logging servers)?", "Synchronise Logging Tasks", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {

                                        //switch Catalogue to use that logging task (including servers)
                                        cata.LoggingDataTask = task;
                                        cata.LiveLoggingServer_ID = liveServers.SingleOrDefault();
                                        cata.TestLoggingServer_ID = testServers.SingleOrDefault();
                                        cata.SaveToDatabase();
                                    }
                                }
                            }
                        }
                    }

                    cata.LoadMetadata_ID = _loadMetadata.ID;
                    cata.SaveToDatabase();
                    _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_loadMetadata));
                }
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
        }
    }
}
