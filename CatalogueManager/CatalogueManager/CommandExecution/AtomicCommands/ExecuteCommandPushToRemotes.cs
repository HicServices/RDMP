using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Remoting;
using CatalogueManager.ItemActivation;
using Newtonsoft.Json;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Serialization;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Progress;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandPushToRemotes : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandPushToRemotes(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            var allSlots = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().ToArray();

            var barsUI = new ProgressBarsUI("Pushing to remotes",true);

            var service = new RemotePushingService(Activator.RepositoryLocator.CatalogueRepository, barsUI);
            var f = new SingleControlForm(barsUI);
            f.Show();

            service.SendCollectionToAllRemotes(allSlots, () => barsUI.Done());
        }
    }
}