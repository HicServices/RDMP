// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup;
using Rdmp.Core.Startup.Events;
using Rdmp.UI.LocationsMenu;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.Versioning;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.TestsAndSetup;

/// <summary>
/// Shows every when RDMP application is first launched.  Tells you whether there are any problems with your current platform databases.
/// If you get an error (Red face) then clicking it will show a log of the startup process.
/// </summary>
public partial class StartupUI : Form, ICheckNotifier
{
    /// <summary>
    /// True if we failed to reach the catalogue database
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CouldNotReachTier1Database { get; private set; }


    private readonly Startup _startup;

    //Constructor
    public StartupUI(Startup startup)
    {
        _startup = startup;

        InitializeComponent();

        if (_startup == null)
            return;

        Text = $"RDMP - v{GetVersion()}";

        _startup.DatabaseFound += StartupDatabaseFound;
        _startup.PluginPatcherFound += StartupPluginPatcherFound;

        pbDisconnected.Image = CatalogueIcons.ExternalDatabaseServer.ImageToBitmap();

        Icon = IconFactory.Instance.GetIcon(Image.Load<Rgba32>(CatalogueIcons.Main));
    }


    public static string GetVersion()
    {
        try
        {
            return typeof(StartupUI).Assembly.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(AssemblyInformationalVersionAttribute))
                ?.ToString().Split('"')[1];
        }
        catch (Exception)
        {
            return "unknown version";
        }
    }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DoNotContinue { get; set; }

    private void StartupDatabaseFound(object sender, PlatformDatabaseFoundEventArgs eventArgs)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            Invoke(new System.Windows.Forms.MethodInvoker(() => StartupDatabaseFound(sender, eventArgs)));
            return;
        }

        HandleDatabaseFoundOnSimpleUI(eventArgs);
    }


    private void StartupPluginPatcherFound(object sender, PluginPatcherFoundEventArgs eventArgs)
    {
        if (InvokeRequired)
        {
            Invoke(new System.Windows.Forms.MethodInvoker(() => StartupPluginPatcherFound(sender, eventArgs)));
            return;
        }

        pbLoadProgress.Value = 800; //80% done
    }

    private bool escapePressed;
    private int countDownToClose = 5;

    private void StartupComplete()
    {
        if (InvokeRequired)
        {
            Invoke(new System.Windows.Forms.MethodInvoker(StartupComplete));
            return;
        }

        if (_startup is { RepositoryLocator.CatalogueRepository: not null })
            WideMessageBox.CommentStore = _startup.RepositoryLocator.CatalogueRepository.CommentStore;

        //when things go badly leave the form
        if (ragSmiley1.IsFatal() || CouldNotReachTier1Database)
            return;

        var t = new Timer
        {
            Interval = 1000
        };
        t.Tick += TimerTick;
        t.Start();

        pbLoadProgress.Value = 1000;
    }

    private void TimerTick(object sender, EventArgs e)
    {
        var t = (Timer)sender;

        if (escapePressed)
        {
            t.Stop();
            return;
        }

        countDownToClose--;

        lblProgress.Text = $"Startup Complete... Closing in {countDownToClose}s (Esc to cancel)";

        if (UserSettings.Wait5SecondsAfterStartupUI && countDownToClose != 0) return;
        t.Stop();
        Close();
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
            try
            {
                lblProgress.Text = "Constructing UserSettingsRepositoryFinder";
                var finder = new UserSettingsRepositoryFinder();
                _startup.RepositoryLocator = finder;
            }
            catch (Exception ex)
            {
                lblProgress.Text = "Constructing UserSettingsRepositoryFinder Failed";
                ragSmiley1.Fatal(ex);
            }

        escapePressed = false;
        countDownToClose = 5;
        lastStatus = RDMPPlatformDatabaseStatus.Healthy;

        //10% progress because we connected to user settings
        pbLoadProgress.Value = 100;

        lblProgress.Text = "Awaiting Platform Database Discovery...";

        var t = new Task(
            () =>
            {
                try
                {
                    _startup.DoStartup(this);
                    StartupComplete();
                }
                catch (Exception ex)
                {
                    if (IsDisposed || !IsHandleCreated)
                        ExceptionViewer.Show(ex);
                    else
                        Invoke(new System.Windows.Forms.MethodInvoker(() => ragSmiley1.Fatal(ex)));
                }
            }
        );
        t.Start();
    }

    private RDMPPlatformDatabaseStatus lastStatus = RDMPPlatformDatabaseStatus.Healthy;
    private ChoosePlatformDatabasesUI _choosePlatformsUI;
    private ChooseLocalFileSystemLocationUI _chooseLocalFileSystem;
    private bool _haveWarnedAboutOutOfDate;

    private void HandleDatabaseFoundOnSimpleUI(PlatformDatabaseFoundEventArgs eventArgs)
    {
        //if status got worse
        if (eventArgs.Status < lastStatus)
            lastStatus = eventArgs.Status;

        //if we are unable to reach a tier 1 database don't report anything else
        if (CouldNotReachTier1Database)
            return;

        lblProgress.Text = $"{eventArgs.Patcher.Name} database status was {eventArgs.Status}";

        switch (eventArgs.Status)
        {
            case RDMPPlatformDatabaseStatus.Unreachable:

                if (eventArgs.Patcher.Tier == 1)
                {
                    pbDisconnected.Visible = true;

                    lblProgress.Text = eventArgs.Repository == null
                        ? "RDMP Platform Databases are not set"
                        : $"Could not reach {eventArgs.Patcher.Name}";

                    CouldNotReachTier1Database = true;

                    ragSmiley1.Fatal(new Exception(
                        $"Core Platform Database was {eventArgs.Status} ({eventArgs.Patcher.Name})",
                        eventArgs.Exception));
                }
                else
                {
                    ragSmiley1.Warning(new Exception(
                        $"Tier {eventArgs.Patcher.Tier} Database was {eventArgs.Status} ({eventArgs.Patcher.Name})",
                        eventArgs.Exception));
                }

                break;

            case RDMPPlatformDatabaseStatus.Broken:
                if (eventArgs.Patcher.Tier == 1)
                    ragSmiley1.Fatal(new Exception(
                        $"Core Platform Database was {eventArgs.Status} ({eventArgs.Patcher.Name})",
                        eventArgs.Exception));
                else
                    ragSmiley1.Warning(new Exception(
                        $"Tier {eventArgs.Patcher.Tier} Database was {eventArgs.Status} ({eventArgs.Patcher.Name})",
                        eventArgs.Exception));
                break;

            case RDMPPlatformDatabaseStatus.RequiresPatching:

                if (MessageBox.Show($"Patching Required on database of type {eventArgs.Patcher.Name}", "Patch RDMP",
                        MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    PatchingUI.ShowIfRequired(
                        eventArgs.Repository.DiscoveredServer.GetCurrentDatabase(),
                        eventArgs.Repository, eventArgs.Patcher);
                    DoNotContinue = true;
                }
                else
                {
                    MessageBox.Show("Patching was cancelled. Apply Patch to use the latest version of RDMP. Application will exit."); 
                    Environment.Exit(0);
                }

                break;
            case RDMPPlatformDatabaseStatus.Healthy:
                ragSmiley1.OnCheckPerformed(new CheckEventArgs(eventArgs.SummariseAsString(), CheckResult.Success));
                return;
            case RDMPPlatformDatabaseStatus.SoftwareOutOfDate:
                if (_haveWarnedAboutOutOfDate) return;

                MessageBox.Show(
                    "The RDMP database you are connecting to is running a newer schema to your software, please consider updating the software to the latest version");
                _haveWarnedAboutOutOfDate = true;

                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventArgs), $"Invalid status {eventArgs.Status}");
        }
    }

    //MEF only!
    public bool OnCheckPerformed(CheckEventArgs args)
    {
        if (InvokeRequired)
        {
            Invoke(new System.Windows.Forms.MethodInvoker(() => OnCheckPerformed(args)));
            return false;
        }

        //if the message starts with a percentage translate it into the progress bars movement
        var progressHackMessage = Percentage();
        var match = progressHackMessage.Match(args.Message);

        if (match.Success)
        {
            var percent = float.Parse(match.Groups[1].Value);
            pbLoadProgress.Value = (int)(500 + percent * 2.5); //500-750
        }

        switch (args.Result)
        {
            case CheckResult.Success:
                break;
            case CheckResult.Warning:
            case CheckResult.Fail:

                //MEF failures are only really warnings
                args.Result = CheckResult.Warning;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args), $"Invalid result {args.Result}");
        }

        lblProgress.Text = args.Message;

        return ragSmiley1.OnCheckPerformed(args);
    }


    private void StartupUIMainForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
            escapePressed = true;
    }

    private void StartupUIMainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_choosePlatformsUI is { ChangesMade: true })
            DoNotContinue = true;

        if (e.CloseReason == CloseReason.UserClosing)
            if (ragSmiley1.IsFatal())
                DoNotContinue = true;
    }

    private void BtnChoosePlatformDatabases_Click(object sender, EventArgs e)
    {
        _choosePlatformsUI = new ChoosePlatformDatabasesUI(_startup.RepositoryLocator);
        _choosePlatformsUI.ShowDialog();
    }

    private void pbDisconnected_Click(object sender, EventArgs e)
    {
        ragSmiley1.ShowMessagesIfAny();
    }

    [GeneratedRegex("^(\\d+)%")]
    private static partial Regex Percentage();

    private void btnLocalFileSystem_Click(object sender, EventArgs e)
    {
        DoNotContinue = true;
        _chooseLocalFileSystem = new ChooseLocalFileSystemLocationUI();
        _chooseLocalFileSystem.ShowDialog();
    }
}