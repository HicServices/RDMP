using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using Newtonsoft.Json;
using ReusableLibraryCode.Serialization;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    public class AutomationServerSlotsMenu : RDMPContextMenuStrip
    {
        public AutomationServerSlotsMenu(IActivateItems activator, AllAutomationServerSlotsNode databaseEntity)
            : base(activator, null)
        {
            Add(new ExecuteCommandCreateNewAutomationSlot(activator));

            Add(new ExecuteCommandPushToRemotes(activator));
        }
    }

    public class ExecuteCommandPushToRemotes : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandPushToRemotes(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override async void Execute()
        {
            var allSlots =
                Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<AutomationServiceSlot>().ToArray();

            foreach (var remote in Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RemoteRDMP>())
            {

                var ignoreRepoResolver = new IgnorableSerializerContractResolver();
                ignoreRepoResolver.Ignore(typeof(DatabaseEntity), new[] { "Repository" });

                var settings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = ignoreRepoResolver,
                };
                var json = JsonConvert.SerializeObject(allSlots, Formatting.Indented, settings);

                var handler = new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(remote.Username, remote.Password)
                };
                HttpResponseMessage result;

                var apiUrl = remote.GetUrlFor<AutomationServiceSlot>(isarray: true);

                using (var client = new HttpClient(handler))
                {
                    try
                    {
                        result = await client.PostAsync(new Uri(apiUrl), new StringContent(json, Encoding.UTF8, "application/json"));
                    }
                    catch (Exception ex)
                    {
                        ExceptionViewer.Show(ex);
                        result = new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = "Error!" };
                    }
                }

                MessageBox.Show(result.ReasonPhrase, result.StatusCode.ToString());
            }

            //if (result.IsSuccessStatusCode)
            //{
            //    lblRemoteResult.Visible = true;
            //    lblRemoteResult.Text = result.ReasonPhrase;
            //    lblRemoteResult.ForeColor = Color.Green;
            //    barRemoteSave.Style = ProgressBarStyle.Continuous;
            //    barRemoteSave.MarqueeAnimationSpeed = 0;
            //}
            //else
            //{
            //    lblRemoteResult.Visible = true;
            //    lblRemoteResult.Text = result.ReasonPhrase;
            //    lblRemoteResult.ForeColor = Color.Red;
            //    barRemoteSave.Style = ProgressBarStyle.Continuous;
            //    barRemoteSave.MarqueeAnimationSpeed = 0;
            //}

            //btnSaveToRemote.Enabled = true;

        }
    }
}