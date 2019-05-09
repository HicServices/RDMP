// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTableUI;
using Rdmp.Core.Databases;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Icons.IconProvision;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace Rdmp.UI.TestsAndSetup.StartupUI
{
    /// <summary>
    /// Shows every time an RDMP application is launched.  The 'User Friendly' view tells you whether there are any problems with your current platform databases / plugins by way of a large
    /// smiley face.  If you get an error (Red face) then there may be a hyperlink to resolve the problem (e.g. if a platform database needs patching or you have not yet configured your 
    /// platform databases (See ChoosePlatformDatabases).
    /// 
    /// <para>Green means that everything is working just fine.</para>
    /// 
    /// <para>Yellow means that something non-critical is not working e.g. a specific plugin is not working correctly</para>
    /// 
    /// <para>Red means that something critical is not working (Check for a fix hyperlink or look at the 'Technical' view to see the exact nature of the problem).</para>
    /// 
    /// <para>The 'Technical' view shows the progress of the discovery / version checking of all tiers of platform databases.  This includes checking that the software version matches the database
    /// schema version  (See ManagedDatabaseUI) and that plugins have loaded correctly (See MEFStartupUI).</para>
    /// </summary>
    public partial class StartupUIMainForm : Form, ICheckNotifier
    {
        private readonly Startup _startup;

        private readonly Icon _red;
        private readonly Icon _yellow;
        private readonly Icon _green;
        
        //Constructor
        public StartupUIMainForm(Startup startup)
        {
            _startup = startup;
            
            InitializeComponent();
            
            if(_startup == null)
                return;
            
            if(Screen.PrimaryScreen.WorkingArea.Width < 768 || Screen.PrimaryScreen.WorkingArea.Height < 1024)
                WindowState = FormWindowState.Maximized;
            
            _startup.DatabaseFound += StartupDatabaseFound;
            _startup.MEFFileDownloaded += StartupMEFFileDownloaded;
            _startup.PluginPatcherFound += StartupPluginPatcherFound;

            var factory = new IconFactory();
            _red = factory.GetIcon(FamFamFamIcons.RedFace);
            _green = factory.GetIcon(FamFamFamIcons.GreenFace);
            _yellow = factory.GetIcon(FamFamFamIcons.YellowFace);

            this.Icon = _green;
            Catalogue.RequestRestart += ()=> StartOrRestart(true);
            DataExport.RequestRestart += () => StartOrRestart(true);
        }

        public bool AppliedPatch { get; set; }

        void StartupDatabaseFound(object sender, PlatformDatabaseFoundEventArgs eventArgs)
        {
            if(IsDisposed || !IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => StartupDatabaseFound(sender, eventArgs)));
                return;
            }

            HandleDatabaseFoundOnSimpleUI(eventArgs);

            //now we are on the correct UI thread.
            if (eventArgs.Patcher is CataloguePatcher)
            {
                Catalogue.Visible = true;
                Catalogue.HandleDatabaseFound(eventArgs);

                if (eventArgs.Status == RDMPPlatformDatabaseStatus.Broken ||
                    eventArgs.Status == RDMPPlatformDatabaseStatus.Unreachable)
                {
                    btnSetupPlatformDatabases.Visible = true;
                    pbLoadProgress.Visible = false;
                    pbWhereIsDatabase.Visible = true;
                    this.Icon = _red;
                }
                else

                    pbWhereIsDatabase.Visible = false;
            }
            
            if (eventArgs.Patcher is DataExportPatcher)
            {
                DataExport.Visible = true;
                DataExport.HandleDatabaseFound(eventArgs);
            }

            if (eventArgs.Patcher.Tier == 2)
            {
                var ctrl = new ManagedDatabaseUI();
                flpTier2Databases.Controls.Add(ctrl);
                ctrl.HandleDatabaseFound(eventArgs);
                ctrl.RequestRestart += () => StartOrRestart(true);
            }

            if (eventArgs.Patcher.Tier == 3)
            {
                var ctrl = new ManagedDatabaseUI();
                flpTier3Databases.Controls.Add(ctrl);
                ctrl.HandleDatabaseFound(eventArgs);
                ctrl.RequestRestart += () => StartOrRestart(true);

                pbLoadProgress.Value = 900;//90% done
            }
        }

        private void StartupMEFFileDownloaded(object sender, MEFFileDownloadProgressEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => StartupMEFFileDownloaded(sender, eventArgs)));
                return;
            }

            mefStartupUI1.Visible = true;
            
            //25% to 50% is downloading MEF
            pbLoadProgress.Value = (int) (250 + ((float)eventArgs.CurrentDllNumber / (float)eventArgs.DllsSeenInCatalogue * 250f));

            lblProgress.Text = "Downloading MEF File " + eventArgs.FileBeingProcessed;

            mefStartupUI1.HandleDownload(eventArgs);

            if (eventArgs.Status == MEFFileDownloadEventStatus.OtherError)
                Fatal(eventArgs.Exception);
        }

        private void StartupPluginPatcherFound(object sender, PluginPatcherFoundEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => StartupPluginPatcherFound(sender, eventArgs)));
                return;
            }

            pbPluginPatchersArrow.Visible = true;


            var ctrl = new PluginPatcherUI();
            flowLayoutPanel1.Controls.Add(ctrl);
            ctrl.HandlePatcherFound(eventArgs);
            
            pbLoadProgress.Value = 800;//80% done
        }

        private bool escapePressed = false;
        private int countDownToClose = 5;

        private void StartupComplete()
        {
            if(InvokeRequired)
            {
                this.Invoke(new MethodInvoker(StartupComplete));
                return;
            }
            
            if (_startup != null && _startup.RepositoryLocator != null && _startup.RepositoryLocator.CatalogueRepository != null)
                WideMessageBox.CommentStore = _startup.RepositoryLocator.CatalogueRepository.CommentStore;

            if (pbRed.Visible || pbRedDead.Visible)
                return;

            Timer t = new Timer
            {
                Interval = 1000
            };
            t.Tick += TimerTick;
            t.Start();

            pbLoadProgress.Value = 1000;
        }

        void TimerTick(object sender, EventArgs e)
        {
            var t = (Timer) sender;
            
            if(escapePressed)
            {
                t.Stop();
                lblStartupComplete1.Visible = false;
                lblStartupComplete2.Visible = false;
                return;
            }

            countDownToClose --;

            string message = string.Format("Startup Complete... Closing in {0}s (Esc to cancel)",countDownToClose);
            lblStartupComplete1.Text = message;
            lblStartupComplete2.Text = message;

            lblStartupComplete1.Visible = true;
            lblStartupComplete2.Visible = true;

            if (countDownToClose == 0)
            {
                t.Stop();
                Close();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_startup == null)
                return;

            StartOrRestart(false);
            
        }

        private void StartOrRestart(bool forceClearRepositorySettings)
        {
            pbLoadProgress.Maximum = 1000;

            if (_startup.RepositoryLocator == null || forceClearRepositorySettings)
            {
                try
                {
                    lblProgress.Text = "Constructing UserSettingsRepositoryFinder";
                    UserSettingsRepositoryFinder finder = new UserSettingsRepositoryFinder();
                    _startup.RepositoryLocator = finder;
                }
                catch (Exception ex)
                {
                    lblProgress.Text = "Constructing UserSettingsRepositoryFinder Failed";
                    Fatal(ex);
                }
            }
            //else
            //    if(!(_startup.RepositoryLocator is UserSettingsRepositoryFinder))
            //        throw new NotSupportedException("You created Startup with an existing repository finder so we were going to reuse that one but it wasn't a UserSettingsRepositoryFinder (it was a " + _startup.RepositoryLocator.GetType().Name + "!)");
            
            Catalogue.Visible = false;
            Catalogue.Reset();

            DataExport.Visible = false;
            DataExport.Reset();

            mefStartupUI1.Visible = false;
            mefStartupUI1.Reset();
            flpTier2Databases.Controls.Clear();

            flpTier3Databases.Controls.Clear();

            escapePressed = false;
            countDownToClose = 5;
            lastStatus = RDMPPlatformDatabaseStatus.Healthy;
            
            pbGreen.Visible = true;
            pbRed.Visible = false;
            pbYellow.Visible = false;
            llException.Visible = false;
            btnSetupPlatformDatabases.Visible = false;

            if (_startup.RepositoryLocator is UserSettingsRepositoryFinder)
                repositoryFinderUI1.SetRepositoryFinder((UserSettingsRepositoryFinder)_startup.RepositoryLocator);
            else
                repositoryFinderUI1.SetRepositoryFinder(_startup.RepositoryLocator);

            lblStartupComplete1.Visible = false;
            lblStartupComplete2.Visible = false;

            //10% progress because we connected to user settings
            pbLoadProgress.Value = 100;

            lblProgress.Text = "Awaiting Platform Database Discovery...";

            Task t = new Task(
                () =>
                    {
                        try
                        {
                            _startup.DoStartup(this);
                            StartupComplete();
                        }
                        catch (Exception ex)
                        {
                            if(IsDisposed || !IsHandleCreated)
                                ExceptionViewer.Show(ex);
                            else
                                Invoke(new MethodInvoker(() => Fatal(ex)));
                        }

                    }
                );
            t.Start();
        }


        private void Fatal(Exception exception)
        {
            lastStatus = RDMPPlatformDatabaseStatus.Broken;
            
            pbGreen.Visible = false;
            pbYellow.Visible = false;
            pbRed.Visible = true;

            if(exception == null)
                pbRed.Visible = true;
            else
            {
                pbRedDead.Visible = true;

                llException.Visible = true;
                llException.Text = exception.Message;
                llException.LinkClicked += (s,e) => ExceptionViewer.Show(exception);
            }
        }

        RDMPPlatformDatabaseStatus lastStatus = RDMPPlatformDatabaseStatus.Healthy;
        

        private void HandleDatabaseFoundOnSimpleUI(PlatformDatabaseFoundEventArgs eventArgs)
        {

            //if status got worse
            if (eventArgs.Status < lastStatus )
                lastStatus = eventArgs.Status;
            else
                return;//we are broken and found more broken stuff!

            lblProgress.Text = eventArgs.Patcher.Name + " database status was " + eventArgs.Status;

            switch (eventArgs.Status)
            {
                case RDMPPlatformDatabaseStatus.Unreachable:

                    if (eventArgs.Patcher.Tier == 1)
                        Angry();
                    else
                        Warning();

                    break;
                case RDMPPlatformDatabaseStatus.Broken:
                    Angry();
                    break;
                case RDMPPlatformDatabaseStatus.RequiresPatching:
                    Warning();

                    if (MessageBox.Show("Patching Required on database of type " + eventArgs.Patcher.Name, "Patch",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        PatchingUI.ShowIfRequired(
                            (SqlConnectionStringBuilder) eventArgs.Repository.ConnectionStringBuilder,
                            eventArgs.Repository, eventArgs.Patcher);
                        AppliedPatch = true;
                    }
                    else
                    {
                        llException.Visible = true;
                        llException.Text = "User rejected patching";
                        Angry();
                    }

                    break;
                case RDMPPlatformDatabaseStatus.Healthy:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //MEF only!
        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if(InvokeRequired)
            {

                Invoke(new MethodInvoker(() => OnCheckPerformed(args)));
                return false;
            }


            //if the message starts with a percentage translate it into the progress bars movement
            Regex progressHackMessage = new Regex("^(\\d+)%");
            var match = progressHackMessage.Match(args.Message);

            if (match.Success)
            {
                var percent = float.Parse(match.Groups[1].Value);
                pbLoadProgress.Value = (int) (500 + (percent*2.5));//500-750
            }
             
            switch (args.Result)
            {
                case CheckResult.Success:
                    break;
                case CheckResult.Warning:
                case CheckResult.Fail:
                    Warning();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            lblProgress.Text = args.Message;

            return mefStartupUI1.OnCheckPerformed(args);
        }

        private void Angry()
        {
            if (pbRed.Visible || pbRedDead.Visible)
                return;


            if (pbGreen.Visible)
                pbGreen.Visible = false;

            if (pbYellow.Visible)
                pbYellow.Visible = false;

            this.Icon = _red;
            pbRed.Visible = true;
        }
        private void Warning()
        {
            if (pbRed.Visible || pbRedDead.Visible)
                return;

            if (pbGreen.Visible)
                pbGreen.Visible = false;

            if (!pbYellow.Visible)
            {
                this.Icon = _yellow;
                pbYellow.Visible = true;
            }
        }

        private void StartupUIMainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                escapePressed = true;
        }

        private void StartupUIMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                if (pbRed.Visible || pbRedDead.Visible)
                {
                    bool loadAnyway = 
                    MessageBox.Show(
                        "Setup failed in a serious way, do you want to try to load the rest of the program anyway?",
                        "Try to load anyway?", MessageBoxButtons.YesNo) == DialogResult.Yes;

                    if(!loadAnyway)
                        Process.GetCurrentProcess().Kill();
                }
        }
        
        private void BtnSetupPlatformDatabases_Click(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandChoosePlatformDatabase(new UserSettingsRepositoryFinder());
            cmd.Execute();
            StartOrRestart(true);
        }
    }
}
