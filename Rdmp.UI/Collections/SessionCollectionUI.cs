using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs.NavigateTo;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.Collections
{
    public class SessionCollectionUI : RDMPUserControl, IObjectCollectionControl
    {
        private BrightIdeasSoftware.TreeListView olvTree;
        private BrightIdeasSoftware.OLVColumn olvName;

        public SessionCollection Collection {get; private set;}
        public RDMPCollectionCommonFunctionality CommonTreeFunctionality {get;} = new RDMPCollectionCommonFunctionality();

        public SessionCollectionUI()
        {
            InitializeComponent();

            olvName.AspectGetter = (o)=>o.ToString();
        }

        public IPersistableObjectCollection GetCollection()
        {
            return Collection;
        }

        public string GetTabName()
        {
            return Collection?.SessionName ?? "Unamed Session";
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
                CommonTreeFunctionality.AxeChildren = new Type[] { typeof(CohortIdentificationConfiguration) };
            }
                
            RefreshSessionObjects();

            CommonFunctionality.ClearToolStrip();
            CommonFunctionality.Add(new ToolStripButton("Add",null,AddObjectToSession));

        }

        private void AddObjectToSession(object sender, EventArgs e)
        {
            var ui = new NavigateToObjectUI(Activator);
            ui.CompletionAction = (s)=>
            {
                Collection.DatabaseObjects.Add(s);
                RefreshSessionObjects();
            };
            ui.Show();
        }

        private void RefreshSessionObjects()
        {
            var actualObjects = FavouritesCollectionUI.FindRootObjects(Activator,Collection.DatabaseObjects.Contains);
            
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
            this.olvName.FillsFreeSpace = true;
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

        #endregion
    }

}
