using Microsoft.Win32;
using Newtonsoft.Json;
using NuGet;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableUIComponents.Dialogs;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Squirrel.UpdateManager;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCheckForUpdates : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandCheckForUpdates(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();
            var updateTask = new Task(() => RunUpdate());
            
            updateTask.Start();
        }

        private void RunUpdate()
        {
            try
            {
                using (var mgr = GetGitHubUpdateManager("https://github.com/HicServices/RDMP"))
                {
                    var entry = mgr.CheckForUpdate().Result;
                    if (entry.ReleasesToApply.Any() && MessageBox.Show("Got things for you: " + entry.ReleasesToApply.Count(), "UPDATE & RESTART!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var download = Task.Run(async () => { await mgr.DownloadReleases(entry.ReleasesToApply); });
                        download.Wait();
                        var updatePath = mgr.ApplyReleases(entry).Result;
                        if (MessageBox.Show("Got things for you: " + updatePath, "RESTART?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            var task = Task.Run(() => Process.Start(new ProcessStartInfo(Path.Combine(updatePath, "ResearchDataManagementPlatform"))));
                            Application.Exit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show(ex);
            }
        }

        private UpdateManager GetGitHubUpdateManager(string repoUrl, bool prerelease = true)
        {
            var repoUri = new Uri(repoUrl);
            var userAgent = new ProductInfoHeaderValue("Squirrel", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (repoUri.Segments.Length != 3)
            {
                throw new Exception("Repo URL must be to the root URL of the repo e.g. https://github.com/myuser/myrepo");
            }

            var releasesApiBuilder = new StringBuilder("repos")
                .Append(repoUri.AbsolutePath)
                .Append("/releases");

            Uri baseAddress = new Uri("https://api.github.com/");

            using (var client = new System.Net.Http.HttpClient() { BaseAddress = baseAddress })
            {
                client.DefaultRequestHeaders.UserAgent.Add(userAgent);
                var response = client.GetAsync(releasesApiBuilder.ToString()).Result;
                response.EnsureSuccessStatusCode();

                var releases = JsonConvert.DeserializeObject<List<Release>>(response.Content.ReadAsStringAsync().Result);
                var latestRelease = releases
                    .Where(x => prerelease || !x.Prerelease)
                    .OrderByDescending(x => x.PublishedAt)
                    .First();

                var latestReleaseUrl = latestRelease.HtmlUrl.Replace("/tag/", "/download/");

                return new UpdateManager(latestReleaseUrl);
            }
        }
    }
}
