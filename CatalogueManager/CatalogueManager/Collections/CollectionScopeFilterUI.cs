using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Remoting;
using CatalogueLibrary.Providers;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;

namespace CatalogueManager.Collections
{
    public partial class CollectionScopeFilterUI : UserControl
    {
        private TreeListView _tree;
        public event EventHandler UnApplied;

        public CollectionScopeFilterUI()
        {
            InitializeComponent();
        }

        public static Type[] PinnableTypes = new Type[]
        {
            typeof (Catalogue),
            typeof (LoadMetadata),
            typeof (Project),
            typeof(CohortIdentificationConfiguration),
            
            //TableInfoCollectionUI pinnables
            typeof(AutomationServiceSlot),
            typeof(ExtractableDataSetPackage),
            typeof(RemoteRDMP),
            typeof(ExternalDatabaseServer),
            typeof(DataAccessCredentials),
            typeof(ANOTable),
            typeof(TableInfo),

            //saved cohorts collection
            typeof(ExtractableCohort)
        };

        private IModelFilter _beforeModelFilter;
        private bool _beforeUseFiltering;

        public static bool IsPinnableType(object o)
        {
            return PinnableTypes.Contains(o.GetType());
        }

        public void ApplyToTree(ICoreChildProvider childProvider,TreeListView tree, IMapsDirectlyToDatabaseTable objectToEmphasise, DescendancyList descendancy)
        {
            if(_tree != null)
                throw new Exception("Scope filter is already applied to a tree");

            object toPin = null;

            if (IsPinnableType(objectToEmphasise))
                toPin = objectToEmphasise;
            else if (descendancy != null)
                toPin = descendancy.Parents.FirstOrDefault(IsPinnableType);

            if(toPin == null)
                return;
            
            _tree = tree;

            lblFilter.Text = toPin.ToString();
            
            //add the filter to the tree
            Dock = DockStyle.Top;

            if (_tree.Dock != DockStyle.Fill)
            {
               //tree is not docked, make some room for us
                _tree.Height -= 19;
                _tree.Top += 19;
            }

            _tree.Parent.Controls.Add(this);

            //get rid of all objects that are not the object to emphasise tree either as parents or as children (recursive)
            List<object> whitelist = new List<object>();

            //add parents to whitelist
            if(descendancy != null && descendancy.Parents.Any())
                whitelist.AddRange(descendancy.Parents);

            whitelist.Add(toPin);

            whitelist.AddRange(childProvider.GetAllChildrenRecursively(toPin));

            _beforeModelFilter = _tree.ModelFilter;
            _beforeUseFiltering = _tree.UseFiltering;

            _tree.UseFiltering = true;
            _tree.ModelFilter = new WhiteListOnlyFilter(whitelist);
        }

        public void UnApplyToTree()
        {
            if (_tree == null)
                return;

            //remove ourselves
            _tree.Parent.Controls.Remove(this);
            
            //bump the tree down again
            _tree.Top -= 19;
            _tree.Height += 19;
            
            //add the original objects back in again (clear filter)
            _tree.UseFiltering = _beforeUseFiltering;
            _tree.ModelFilter = _beforeModelFilter;
            _tree = null;

            if(UnApplied != null)
                UnApplied(this,new EventArgs());
        }

        private void pbRemoveFilter_Click(object sender, EventArgs e)
        {
            UnApplyToTree();
        }

    }
}
