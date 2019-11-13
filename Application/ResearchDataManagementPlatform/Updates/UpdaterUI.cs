// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Squirrel;

using ReusableLibraryCode;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using BrightIdeasSoftware;
using Newtonsoft.Json;
using NuGet;
using Rdmp.UI.SimpleDialogs;
using TB.ComponentModel;

namespace ResearchDataManagementPlatform.Updates
{
    public partial class UpdaterUI : UserControl
    {
        private const string RepoUrl = "https://github.com/HicServices/RDMP";
        private const string ApiUrl = "https://api.github.com/";
        private UpdateInfo _updateInfo;
        private Version _currentVersion;
        public List<GHRelease> Entries { get; set; }
        public List<ReleaseEntry> SquirrelEntries { get; set; }

        public UpdaterUI()
        {
            InitializeComponent();
            Entries = new List<GHRelease>();
            SquirrelEntries = new List<ReleaseEntry>();

            GetUpdatesAsync();

            _currentVersion = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);

            olvVersion.AspectGetter = (m) => ((ReleaseEntry) m).Version.ToString();
            olvInstall.AspectGetter = (m) => "Install";
            olvType.AspectGetter = (m) => ((ReleaseEntry) m).Version.SpecialVersion; //.Any() ? "Alpha" : "Stable";

            objectListView1.ButtonClick += ObjectListView1_ButtonClick;
        }

        private void ObjectListView1_ButtonClick(object sender, BrightIdeasSoftware.CellClickEventArgs e)
        {
            var entry = e.Item.RowObject as ReleaseEntry;

            if(entry != null)
                InstallAsync(entry);
        }

        private void InstallAsync(ReleaseEntry entry)
        {
            pbLoading.Visible = true;

            var t = new Task(()=>
            {
                try
                {
                    using (var mgr = UpdateManager.GitHubUpdateManager(RepoUrl, prerelease:true).Result)
                    {
                        var rootDir = UpdateManager.GetLocalAppDataDirectory();
                        var current =
                            ReleaseEntry.ParseReleaseFile(
                                File.ReadAllLines(Path.Combine(rootDir, "packages", "RELEASES"))[0]);
                        _updateInfo = UpdateInfo.Create(current.First(), new []{ entry }, Path.Combine(rootDir, "packages"));
                        mgr.DownloadReleases(new []{ entry },OnProgress).Wait();
                        
                        var updatePath = mgr.ApplyReleases(_updateInfo, OnProgress).Result;

                        var task = Task.Run(() => Process.Start(new ProcessStartInfo(Path.Combine(updatePath, "ResearchDataManagementPlatform.exe"))));
                        Application.Exit();
                
                    }
                }
                catch(Exception ex)
                {
                    ExceptionViewer.Show(ex);
                }

                FinishedLoading();
            });

            t.Start();
            
        }

        private void OnProgress(int obj)
        {
        }

        private void GetUpdatesAsync()
        {
            Task t = new Task(() =>
            {
                try
                {
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient()
                    {
                        BaseAddress = new Uri(ApiUrl)
                    })
                    {
                        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RDMP", _currentVersion.ToString()));
                        HttpResponseMessage async = client.GetAsync("repos/HicServices/RDMP/releases").Result;
                        async.EnsureSuccessStatusCode();
                        Entries = JsonConvert.DeserializeObject<List<GHRelease>>(async.Content.ReadAsStringAsync().Result);
                    }
                    Parallel.ForEach(
                        Entries,
                        release =>
                        {
                            var releaseFile = release.assets.FirstOrDefault(a => a.name == "RELEASES");
                            if (releaseFile != null)
                            {
                                SquirrelEntries.Add(DownloadRelease(releaseFile));
                            }
                        });
                    AddReleases(SquirrelEntries);

                }
                catch(AggregateException ex)
                {
                    var invalid = ex.GetExceptionIfExists<InvalidOperationException>();

                    if(invalid != null && invalid.Message.Contains("Sequence contains no elements"))
                        AddReleases(new List<ReleaseEntry>());
                    else
                        ExceptionViewer.Show(ex);
                }
                catch(InvalidOperationException ex)
                {
                    if(ex.Message.Contains("Sequence contains no elements"))
                        AddReleases(new List<ReleaseEntry>());
                    else
                        ExceptionViewer.Show(ex);
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show(ex);
                }
                
                FinishedLoading();
            });

            t.Start();
        }

        private ReleaseEntry DownloadRelease(Asset releaseFile)
        {
            var content = new WebClient().DownloadString(releaseFile.browser_download_url);
            content = content.Replace(
                "ResearchDataManagementPlatform",
                releaseFile.browser_download_url.Replace("RELEASES", "") + "ResearchDataManagementPlatform");
            var result = ReleaseEntry.ParseReleaseFile(content).First();
            return result;
        }

        private void FinishedLoading()
        {
            if(InvokeRequired)
            {
                Invoke(new MethodInvoker(FinishedLoading));
                return;
            }
            
            pbLoading.Visible = false;
        }

        private void AddReleases(List<ReleaseEntry> releasesToApply)
        {
            if(InvokeRequired)
            {
                Invoke(new MethodInvoker(()=>AddReleases(releasesToApply)));
                return;
            }
            
            releasesToApply = releasesToApply
                .Where(v => CanShow(v.Version))
                .OrderByDescending(x => x.Version)
                .ToList();
            
            if(releasesToApply.Any())
                objectListView1.AddObjects(releasesToApply);
            else
                lblStatus.Text = "No Updates Available";
        }

        private bool CanShow(SemanticVersion v)
        {
            if (cbShowOlderVersions.Checked && cbShowPrerelease.Checked)
                return true;

            var cv = new SemanticVersion(_currentVersion);

            if (cbShowOlderVersions.Checked && !cbShowPrerelease.Checked)
                return String.IsNullOrEmpty(v.SpecialVersion);

            if (!cbShowOlderVersions.Checked && cbShowPrerelease.Checked)
                return (v > cv) && v.SpecialVersion.Any();

            return (v > cv) && String.IsNullOrEmpty(v.SpecialVersion);
        }

        private void CheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            objectListView1.ClearObjects();
            AddReleases(SquirrelEntries);
        }
    }
}
