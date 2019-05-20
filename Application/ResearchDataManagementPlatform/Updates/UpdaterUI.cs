using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Squirrel;
using ReusableUIComponents.Dialogs;
using ReusableLibraryCode;
using System.Diagnostics;
using System.IO;

namespace ResearchDataManagementPlatform.Updates
{
    public partial class UpdaterUI : UserControl
    {
        private const string Url = "https://github.com/HicServices/RDMP";
        private UpdateInfo _updateInfo;

        public UpdaterUI()
        {
            InitializeComponent();

            GetUpdatesAsync();

            olvVersion.AspectGetter = (m)=>((ReleaseEntry)m).Version.ToString();
            olvInstall.AspectGetter = (m)=>{return "Install";};
            olvType.AspectGetter =(m)=>((ReleaseEntry)m).Version.SpecialVersion.Any() ? "Alpha":"Stable";
                        
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
                    using (var mgr = UpdateManager.GitHubUpdateManager(Url,prerelease:true).Result)
                    {
                         mgr.DownloadReleases(new []{entry },OnProgress).Wait();
                
                        var updatePath = mgr.ApplyReleases(_updateInfo,OnProgress).Result;

                        var task = Task.Run(() => Process.Start(new ProcessStartInfo(Path.Combine(updatePath, "ResearchDataManagementPlatform.exe"))));
                        Application.Exit();
                
                    }
                }catch(Exception ex)
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
                    using (var mgr = UpdateManager.GitHubUpdateManager(Url,prerelease:true).Result)
                    {
                        var entry = mgr.CheckForUpdate().Result;
                        _updateInfo = entry;
                        AddReleases(entry.ReleasesToApply);
                    }
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
            
            if(releasesToApply.Any())
                objectListView1.AddObjects(releasesToApply);
            else
                lblStatus.Text = "No Updates Available";

        }
    }
}
