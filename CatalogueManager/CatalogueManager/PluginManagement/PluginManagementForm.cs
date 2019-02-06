// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;

using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using CatalogueManager.TestsAndSetup.StartupUI;
using MapsDirectlyToDatabaseTable;
using PluginPackager;
using RDMPStartup;
using RDMPStartup.PluginManagement;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Serialization;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.Progress;
using ReusableUIComponents.SingleControlForms;
using Sharing.Transmission;

namespace CatalogueManager.PluginManagement
{
    /// <summary>
    /// Shows all the currently configured Plugins you have uploaded into your Catalogue Database.  Plugins are .zip files which contain one or more dlls.  The name of the zip file is the name
    /// of the Plugin.  You can upload a plugin by dropping the zip file into left hand tree view (where it says 'Drop Here').  Once uploaded, all the contents of the zip file are saved in the
    /// LoadModuleAssembly table in your Catalogue Database.  Then when any user launches an RDMP program they will receive a copy of the plugin downloaded into their %appdata%\MEF directory.
    /// 
    /// <para>Clicking a plugin will expand to show all the dll files in the plugin.  Expanding a dll will show all the list of RDMP compatible (Exported) classes in that dll.  Clicking on one of the
    /// classes will open populate the dependencies and allow you to view the MISL of the plugin (See PluginDependencyVisualisation).</para>
    /// 
    /// <para>Pressing the 'Delete' key on your keyboard will delete the selected Plugin or Dll from the LoadModuleAssembly table in your Catalogue Database.  This will not immediately unload the 
    /// plugin locally because all the plugins will be currently read locked however the next time you restart the application (or start a new RDMP application) the local copies of the plugin
    /// will also be deleted. </para>
    /// </summary>
    public partial class PluginManagementForm : RDMPForm
    {
        private readonly IActivateItems _activator;

        public PluginManagementForm(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();

            var sink = new SimpleDropSink();

            sink.CanDrop += sink_CanDrop;
            sink.Dropped += sink_Dropped;
            
            treeListView.DropSink = sink;

            treeListView.CanExpandGetter += (m) => (m is Plugin || m is LoadModuleAssembly || m is String) && !m.ToString().Equals("src.zip");
            treeListView.ChildrenGetter += ChildrenGetter;
            treeListView.FormatRow += TreeListViewOnFormatRow;

            olvVersion.AspectGetter = (m) => (m is Plugin ? ((Plugin)m).PluginVersion : null);
            
            treeListView.SetNativeBackgroundWatermark(CatalogueIcons.DropHere);
        }

        private IEnumerable ChildrenGetter(object model)
        {
            var root = model as String;
            var plugin = model as Plugin;
            var lma = model as LoadModuleAssembly;

            if (root == "Loaded")
                return compatiblePlugins;
            if (root == "Not loaded")
                return wrongPlugins;

            if (plugin != null)
                return plugin.LoadModuleAssemblies;

            if (lma != null)
            {
                if (!analysers.ContainsKey(lma.Plugin))
                    return new object[0];

                var report = analysers[lma.Plugin].Reports[lma];

                if (report.Parts.Any())
                    return report.Parts;
                
                if(report.Status == PluginAssemblyStatus.BadAssembly)
                    return new[] { report.BadAssemblyException };
                    
                return new []{report.Status};
            }

            return new object[0];
        }

        private void TreeListViewOnFormatRow(object sender, FormatRowEventArgs formatRowEventArgs)
        {
            var plugin = formatRowEventArgs.Model as Plugin;
            var lma = formatRowEventArgs.Model as LoadModuleAssembly;
            var part = formatRowEventArgs.Model as PluginPart;
            var exception = formatRowEventArgs.Model as Exception;

            if(plugin != null)
            {
                if (!analysers.ContainsKey(plugin))
                {
                    formatRowEventArgs.Item.ForeColor = Color.DimGray;
                    return;
                }

                if (analysers[plugin].Reports.Any())
                {
                    var worstStatus = analysers[plugin].Reports.Min(kvp => kvp.Value.Status);
                    formatRowEventArgs.Item.ForeColor = worstStatus == PluginAssemblyStatus.Healthy ? Color.Green : Color.Red;
                }
                else
                {
                    formatRowEventArgs.Item.ForeColor = Color.IndianRed;
                }
            }
            
            if (lma != null)
            {
                if (!analysers.Keys.Any(p => p.ID == lma.Plugin_ID))
                {
                    formatRowEventArgs.Item.ForeColor = Color.DimGray;
                    return;
                }

                var report = analysers[lma.Plugin].Reports[lma];
                formatRowEventArgs.Item.ForeColor = report.Status == PluginAssemblyStatus.Healthy? Color.Green: Color.Red;

                if (report.Status == PluginAssemblyStatus.Healthy && report.Parts.Any())
                    formatRowEventArgs.Item.ForeColor = Color.LawnGreen;
            }

            if (part != null)
                formatRowEventArgs.Item.ForeColor = part.Dependencies.Any(d => d.Exception != null)? Color.Red: Color.Black;

            if(exception != null)
            {
                formatRowEventArgs.Item.Font = new Font(formatRowEventArgs.Item.Font, FontStyle.Underline);
                formatRowEventArgs.Item.ForeColor = Color.Blue;
            }
            
        }

        #region Drag and Drop To Create New Plugins
        void sink_CanDrop(object sender, OlvDropEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            DataObject dataObject = e.DataObject as DataObject;

            if (dataObject != null)
            {
                StringCollection files = dataObject.GetFileDropList();
                
                if(files.Count >= 0)
                {
                    string[] zipFiles = files.Cast<string>().Where(s => s.EndsWith(".zip")).ToArray();

                    if(zipFiles.Any())
                        e.Effect = DragDropEffects.Copy;
                }
            }
        }
        void sink_Dropped(object sender, OlvDropEventArgs e)
        {
            var zipFiles = ((DataObject)e.DataObject).GetFileDropList().Cast<string>().Where(s => s.EndsWith(".zip")).ToArray();

            if (!zipFiles.Any())
                return;

            foreach (string file in zipFiles)
                AddPlugin(file);

            PromptRestart();
        }

        private void AddPlugin(string file)
        {
            var f = new FileInfo(file);
            if (f.Extension == ".sln")
            {
                bool release = MessageBox.Show("Do you want to look for 'Release' directories instead of 'Debug'?" + Environment.NewLine + "Yes - Release" + Environment.NewLine + "No - Debug","Look for Release directories instead?",MessageBoxButtons.YesNo) == DialogResult.Yes;

                var zip = Path.Combine(f.Directory.FullName, Path.GetFileNameWithoutExtension(f.Name) +".zip");
                Packager packager = new Packager(f, zip, false,release);
                packager.PackageUpFile(checksUI1);
                
                f = new FileInfo(zip);
            }
             
            if(f.Exists)
            {
                var pluginProcessor = new PluginProcessor(checksUI1, RepositoryLocator.CatalogueRepository);
            
                if (pluginProcessor.ProcessFileReturningTrueIfIsUpgrade(f))
                {

                    MessageBox.Show("Replaced old version of Plugin '" + f.Name + "'");
                    RefreshObjects();
                }
            }
        }

        private void RefreshObjects()
        {
            foreach (var o in treeListView.Objects.OfType<DatabaseEntity>().ToArray())
            {
                if(!o.Exists())
                    treeListView.RemoveObject(o);
            }
        }

        private void PromptRestart()
        {
            if(MessageBox.Show("Application must restart for new Plugin(s) to be loaded, would you like to restart now","Restart Now",MessageBoxButtons.YesNo) == DialogResult.Yes)
                Application.Restart();
        }

        private string _version;

        #endregion

        private IList<Plugin> wrongPlugins;
        private IList<Plugin> compatiblePlugins;
        BackgroundWorker analyser;

        private void RefreshUIFromDatabase()
        {
            if (analyser != null)
            {
                MessageBox.Show("Cannot refresh at this time,plugin analysis is still running");
                return;
            }

            treeListView.ClearObjects();

            compatiblePlugins = RepositoryLocator.CatalogueRepository.GetCompatiblePlugins().ToList();
            wrongPlugins = RepositoryLocator.CatalogueRepository.GetAllObjects<Plugin>().Except(compatiblePlugins).ToList();
            analyser = new BackgroundWorker();
            analyser.DoWork += analyser_DoWork;
            analyser.RunWorkerCompleted += analyser_RunWorkerCompleted;
            analyser.RunWorkerAsync();
        }

        void analyser_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            treeListView.AddObjects(new[] { "Loaded", "Not loaded" });
            analyser = null;
        }

        void analyser_DoWork(object sender, DoWorkEventArgs e)
        {
            analysers.Clear();
            
            var mef = RepositoryLocator.CatalogueRepository.MEF;

            foreach (Plugin plugin in compatiblePlugins)
            {
                var pluginDir = plugin.GetPluginDirectoryName(mef.DownloadDirectory);
                var pa = new PluginAnalyser(plugin, new DirectoryInfo(pluginDir), mef.SafeDirectoryCatalog);

                pa.ProgressMade += pa_ProgressMade;

                pa.Analyse();
                analysers.Add(plugin, pa);
            }
        }

        void pa_ProgressMade(PluginAnalyser sender, PluginAnalyserProgressEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => pa_ProgressMade(sender, eventArgs)));
                return;
            }
            
            pbAnalysing.Maximum = eventArgs.ProgressMax;
            pbAnalysing.Value = eventArgs.Progress;
            lblProgressAnalysing.Text = eventArgs.CurrentAssemblyBeingProcessed != null
                ? "Analysing " + eventArgs.CurrentAssemblyBeingProcessed.Name
                : "Done";

        }

        private Dictionary<Plugin, PluginAnalyser> analysers = new Dictionary<Plugin, PluginAnalyser>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(VisualStudioDesignMode)
                return;

            RefreshUIFromDatabase();
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var deleteable = treeListView.SelectedObject as IDeleteable;

                if(deleteable is Plugin)
                    if(MessageBox.Show("Are you sure you want to delete '"+deleteable+"'?"+Environment.NewLine + Environment.NewLine +" NOTE:It is likely this assembly/plugin is currently ReadLocked since the application is running, deleting will remove it from the Database and remove it locally the next time you restart the application.","Confirm Deleting", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        deleteable.DeleteInDatabase();
                        if (wrongPlugins.Contains((Plugin)deleteable))
                            wrongPlugins.Remove((Plugin) deleteable);
                        else
                            compatiblePlugins.ToList().Remove((Plugin)deleteable);
                        RefreshUIFromDatabase();
                    }
            }
        }

        private void treeListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            var lma = treeListView.SelectedObject as LoadModuleAssembly;
            var cpa = treeListView.SelectedObject as PluginPart;

            if(lma != null)
            {
                if(!analysers.ContainsKey(lma.Plugin))
                    return;

                pluginDependencyVisualisation1.Select(analysers[lma.Plugin].Reports[lma]);
                
            }
            else
            if(cpa != null)
                pluginDependencyVisualisation1.Select(cpa);
            else
                pluginDependencyVisualisation1.ClearSelection();

        }

        private void treeListView_ItemActivate(object sender, EventArgs e)
        {
            if(treeListView.SelectedObject is Exception)
                ExceptionViewer.Show((Exception)treeListView.SelectedObject);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Plugins|*.zip|Solution Files|*.sln";
            fd.CheckFileExists = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                AddPlugin(fd.FileName);
                PromptRestart();
            }

        }

        private void btnSaveToRemote_Click(object sender, EventArgs e)
        {
            if (!compatiblePlugins.Any())
            {
                MessageBox.Show(this, "There are no compatible plugins in the system...", "Error");
                return;
            }

            var barsUI = new ProgressBarsUI("Pushing to remotes", true);
            var service = new RemotePushingService(RepositoryLocator, barsUI);
            var f = new SingleControlForm(barsUI);
            f.Show();

            service.SendToAllRemotes(compatiblePlugins.ToArray(), barsUI.Done);
        }

        private void btnExportToDisk_Click(object sender, EventArgs e)
        {
            var cmd = new ExecuteCommandExportObjectsToFileUI(_activator, compatiblePlugins.ToArray());
            cmd.Execute();
        }
    }
}
