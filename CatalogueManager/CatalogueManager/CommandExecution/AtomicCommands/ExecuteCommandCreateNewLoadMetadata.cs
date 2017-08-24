using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using HIC.Logging;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewLoadMetadata : BasicCommandExecution, IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private Catalogue[] _availableCatalogues;

        public ExecuteCommandCreateNewLoadMetadata(IActivateItems activator)
        {
            _activator = activator;
            _availableCatalogues = activator.CoreChildProvider.AllCatalogues.Where(c => c.LoadMetadata_ID == null).ToArray();
            
            if(!_availableCatalogues.Any())
                SetImpossible("There are no Catalogues that are not associated with another Load already");
        }

        public override void Execute()
        {
            base.Execute();

            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_availableCatalogues,false,false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //create the load
                var cata = dialog.Selected as Catalogue;
                
                if(cata != null)
                {
                    var cataRepository = (CatalogueRepository) cata.Repository;

                    var lmd = new LoadMetadata(cataRepository, "Loading " + cata.Name);

                    //if theres no logging task / logging server set them up with the same name as the lmd
                    IExternalDatabaseServer loggingServer;

                    if (cata.LiveLoggingServer_ID == null)
                    {
                        loggingServer = new ServerDefaults(cataRepository).GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

                        if (loggingServer != null)
                            cata.LiveLoggingServer_ID = loggingServer.ID;
                        else
                            throw new NotSupportedException("You do not yet have any logging servers configured so cannot create data loads");
                    }
                    else
                        loggingServer = cataRepository.GetObjectByID<ExternalDatabaseServer>(cata.LiveLoggingServer_ID.Value);

                    //if theres no logging task yet and theres a logging server
                    if (string.IsNullOrWhiteSpace(cata.LoggingDataTask))
                    {
                        var lm = new LogManager(loggingServer);
                        var loggingTaskName = lmd.Name;
                        
                        lm.CreateNewLoggingTaskIfNotExists(loggingTaskName);
                        cata.LoggingDataTask = loggingTaskName;
                    }

                    cata.LoadMetadata_ID = lmd.ID;
                    cata.SaveToDatabase();

                    _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(lmd));

                    var arrangeEditting = new ExecuteCommandEditExistingLoadMetadata(_activator);
                    arrangeEditting.LoadMetadata = lmd;
                    arrangeEditting.Execute();
                }
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
    }
}