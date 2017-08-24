using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using RDMPStartup.PluginManagement;
using ReusableUIComponents;
using ReusableUIComponents.Annotations;

namespace CatalogueManager.PluginManagement
{
    /// <summary>
    /// Part of PluginManagementForm, this control lists all the exportable (plugin) classes in your selected plugin dll.  Each class can be expanded to see the methods, each method can be 
    /// expanded to see the MSIL operations that make them up.  This is useful for quickly determining whether a plugin is compatible with a new version of the RDMP software or for identifying
    /// missing dependencies.  
    /// 
    /// This also ensures that you can see the exact MISL operations for a plugin for debugging errors even when you don't have the source code for the plugin. 
    /// </summary>
    public partial class PluginDependencyVisualisation : UserControl
    {
        public void Select(PluginAnalyserReport report)
        {
            treeView1.ClearObjects();
            if (report != null)
                treeView1.AddObjects(report.Parts);
        }

        public void Select(PluginPart part)
        {
            treeView1.ClearObjects();
            treeView1.AddObject(part);
        }

        //constructor
        public PluginDependencyVisualisation()
        {
            InitializeComponent();

            treeView1.CanExpandGetter += CanExpandGetter;
            treeView1.ChildrenGetter += ChildrenGetter;
            treeView1.FormatRow += TreeView1OnFormatRow;
        }

        private void TreeView1OnFormatRow(object sender, FormatRowEventArgs formatRowEventArgs)
        {
            var part = formatRowEventArgs.Model as PluginPart;
            var dependency = formatRowEventArgs.Model as PluginDependency;
            var exception = formatRowEventArgs.Model as Exception;

            if(formatRowEventArgs.Model is Exception)
                formatRowEventArgs.Item.ForeColor = Color.Red;
            
            if(part != null)
                if (part.Dependencies.Any(d => d.Exception != null))
                    formatRowEventArgs.Item.ForeColor = Color.Red;

            if (dependency != null && dependency.Exception != null)
                formatRowEventArgs.Item.ForeColor = Color.Red;


            if (dependency != null && dependency.Exception != null)
                formatRowEventArgs.Item.ForeColor = Color.Red;

            if (exception != null)
            {
                formatRowEventArgs.Item.Font = new Font(formatRowEventArgs.Item.Font, FontStyle.Underline);
                formatRowEventArgs.Item.ForeColor = Color.Blue;
            }

        }

        private bool CanExpandGetter(object model)
        {
            var dependency = model as PluginDependency;
            var parts = model as PluginPart;

            if (parts != null)
                return parts.Dependencies.Any();

            if (dependency != null)
                return dependency.Instructions.Any() || dependency.Exception != null;

            return false;
        }

        private IEnumerable ChildrenGetter(object model)
        {
            var dependency = model as PluginDependency;
            var parts = model as PluginPart;

            if (parts != null)
                return parts.Dependencies;

            if (dependency != null)
            {
                if (dependency.Exception != null)
                    return new[]{dependency.Exception};

                return dependency.Instructions;
            }

            return null;
        }

        private void RefreshUI()
        {
            
        }

        public void ClearSelection()
        {
            treeView1.ClearObjects();
        }

        private void treeView1_ItemActivate(object sender, EventArgs e)
        {

            if (treeView1.SelectedObject is Exception)
                ExceptionViewer.Show((Exception)treeView1.SelectedObject);
        
        }
    }
}
