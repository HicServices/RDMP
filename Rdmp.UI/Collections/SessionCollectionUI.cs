// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Toolbox control for storing user defined collections of objects.  Similar to <see cref="FavouritesCollectionUI"/> but for limited lifetime scope.  Note that this class inherits from <see cref="RDMPUserControl"/> not <see cref="RDMPCollectionUI"/>
    /// </summary>
    public class SessionCollectionUI : RDMPUserControl, IObjectCollectionControl, IConsultableBeforeClosing
    {
        private BrightIdeasSoftware.TreeListView olvTree;
        private BrightIdeasSoftware.OLVColumn olvName;
        private bool _firstTime = true;

        public SessionCollection Collection {get; private set;}
        public RDMPCollectionCommonFunctionality CommonTreeFunctionality {get;} = new RDMPCollectionCommonFunctionality();

        public SessionCollectionUI()
        {
            InitializeComponent();

            olvName.AspectGetter = (o)=>o.ToString();
            CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration)};
        }

        public IPersistableObjectCollection GetCollection()
        {
            return Collection;
        }

        public string GetTabName()
        {
            return Collection?.SessionName ?? "Unamed Session";
        }

        public string GetTabToolTip()
        {
            return null;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            SetItemActivator(activator);

            Collection = (SessionCollection)collection;

            if(!CommonTreeFunctionality.IsSetup)
            {
                CommonTreeFunctionality.SetUp(RDMPCollection.None,olvTree,activator,olvName,olvName,new RDMPCollectionCommonFunctionalitySettings()
                {
                    // add custom options here
                    
                });
            }
                
            RefreshSessionObjects();

            CommonFunctionality.ClearToolStrip();
            CommonFunctionality.Add(new ToolStripButton("Add...",null,AddObjectToSession));
            CommonFunctionality.Add(new ToolStripButton("Remove...", null, RemoveObjectsFromSession));

            if(_firstTime)
            {
                CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("a6abe085-f5cc-4ce0-85ef-0d42e7dbfced"));
                _firstTime = false;
            }

        }

        private void RemoveObjectsFromSession(object sender, EventArgs e)
        {
            var toRemove = Activator.SelectMany("Remove From Session", typeof(IMapsDirectlyToDatabaseTable), Collection.DatabaseObjects.ToArray());

            if(toRemove != null && toRemove.Length > 0)
            {
                Remove(toRemove);
            }
        }

        /// <summary>
        /// Adds <paramref name="toAdd"/> to the list of objects tracked in the session
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(params IMapsDirectlyToDatabaseTable[] toAdd)
        {
            for(int i=0;i< toAdd.Length;i++)
            {
                //unwrap pipelines
                if(toAdd[i] is PipelineCompatibleWithUseCaseNode pcn)
                {
                    toAdd[i] = pcn.Pipeline;
                }
                else if(toAdd[i] is SpontaneousObject)
                {
                    throw new NotSupportedException("Object cannot be added to sessions");
                }
            }

            Collection.DatabaseObjects = toAdd.Union(Collection.DatabaseObjects).ToList();
            RefreshSessionObjects();
        }
        /// <summary>
        /// Removes <paramref name="toRemove"/> from the list of objects tracked in the session
        /// </summary>
        /// <param name="toRemove"></param>
        public void Remove(params IMapsDirectlyToDatabaseTable[] toRemove)
        {
            foreach(var r in toRemove)
            {
                Collection.DatabaseObjects.Remove(r);
                olvTree.RemoveObject(r);
            }
            
            RefreshSessionObjects();
        }
        private void AddObjectToSession(object sender, EventArgs e)
        {
            var toAdd = Activator.SelectMany(new DialogArgs
                {
                    WindowTitle = "Add to Session",
                    TaskDescription = "Pick which objects you want added to the session window."
                }, typeof(IMapsDirectlyToDatabaseTable),
                Activator.CoreChildProvider.GetAllSearchables().Keys.Except(Collection.DatabaseObjects).ToArray())?.ToList();

            if (toAdd == null || toAdd.Count() == 0)
            {
                // user cancelled picking objects
                return;
            }

            Add(toAdd.ToArray());
        }

        private void RefreshSessionObjects()
        {
            var actualObjects = FavouritesCollectionUI.FindRootObjects(Activator,Collection.DatabaseObjects.Contains)
                .Union(Collection.DatabaseObjects.OfType<Pipeline>()).ToList();
            
            //no change in root favouratism
            if (actualObjects.SequenceEqual(olvTree.Objects.OfType<IMapsDirectlyToDatabaseTable>()))
                return;

            //remove old objects
            foreach (var old in Collection.DatabaseObjects.Except(actualObjects))
                olvTree.RemoveObject(old);

            //add new objects
            foreach (var newObject in actualObjects.Except(olvTree.Objects.OfType<IMapsDirectlyToDatabaseTable>()))
                olvTree.AddObject(newObject);

            //update to the new list
            Collection.DatabaseObjects = actualObjects;
            olvTree.RebuildAll(true);
        }

        public override string ToString()
        {
            return Collection?.SessionName ?? "Unamed Session";
        }

        #region InitializeComponent
        private void InitializeComponent()
        {
            this.olvTree = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.olvTree)).BeginInit();
            this.SuspendLayout();
            // 
            // olvRecent
            // 
            this.olvTree.AllColumns.Add(this.olvName);
            this.olvTree.CellEditUseWholeCell = false;
            this.olvTree.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.olvTree.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.olvTree.FullRowSelect = true;
            this.olvTree.HideSelection = false;
            this.olvTree.Location = new System.Drawing.Point(0, 0);
            this.olvTree.Name = "olvRecent";
            this.olvTree.RowHeight = 19;
            this.olvTree.ShowGroups = false;
            this.olvTree.Size = new System.Drawing.Size(487, 518);
            this.olvTree.TabIndex = 4;
            this.olvTree.UseCompatibleStateImageBehavior = false;
            this.olvTree.View = System.Windows.Forms.View.Details;
            this.olvTree.VirtualMode = true;
            // 
            // olvName
            // 
            this.olvName.Groupable = false;
            this.olvName.Text = "Name";
            // 
            // SessionCollectionUI
            // 
            this.Controls.Add(this.olvTree);
            this.Name = "SessionCollectionUI";
            this.Size = new System.Drawing.Size(487, 518);
            ((System.ComponentModel.ISupportInitialize)(this.olvTree)).EndInit();
            this.ResumeLayout(false);

        }

        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = !Activator.YesNo($"Close Session {Collection.SessionName}? (this will end the session)","End Session");
            }
        }

        #endregion
    }

}
