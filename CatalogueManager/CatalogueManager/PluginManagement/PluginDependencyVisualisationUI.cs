// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using RDMPStartup.PluginManagement;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.PluginManagement
{
    /// <summary>
    /// Part of PluginManagementForm, this control lists all the exportable (plugin) classes in your selected plugin dll.  Each class can be expanded to see the methods, each method can be 
    /// expanded to see the MSIL operations that make them up.  This is useful for quickly determining whether a plugin is compatible with a new version of the RDMP software or for identifying
    /// missing dependencies.  
    /// 
    /// <para>This also ensures that you can see the exact MISL operations for a plugin for debugging errors even when you don't have the source code for the plugin. </para>
    /// </summary>
    public partial class PluginDependencyVisualisationUI : UserControl
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
        public PluginDependencyVisualisationUI()
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
