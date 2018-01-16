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
using HIC.Common.Validation.Constraints.Primary;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// User interface component for indicating to the user that there is a Pinned object in an RDMPCollectionUI.  Bumps the collection tree down 19 pixels (or docks top).
    /// And then assembles a whitelist filter that will show only the pinned object hierarchy and children.
    /// </summary>
    [TechnicalUI]
    public partial class CollectionPinFilterUI : UserControl
    {
        private TreeListView _tree;
        public event EventHandler UnApplied;

        public CollectionPinFilterUI()
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
        private object _toPin;
        
        public static bool IsPinnableType(object o)
        {
            return PinnableTypes.Contains(o.GetType());
        }

        public void ApplyToTree(ICoreChildProvider childProvider,TreeListView tree, IMapsDirectlyToDatabaseTable objectToEmphasise, DescendancyList descendancy)
        {
            if(_tree != null)
                throw new Exception("Scope filter is already applied to a tree");

            _toPin = null;

            if (IsPinnableType(objectToEmphasise))
                _toPin = objectToEmphasise;
            else if (descendancy != null)
                _toPin = descendancy.Parents.FirstOrDefault(IsPinnableType);

            if (_toPin == null)
                return;
            
            _tree = tree;

            lblFilter.Text = _toPin.ToString();
            
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

            whitelist.Add(_toPin);

            whitelist.AddRange(childProvider.GetAllChildrenRecursively(_toPin));

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

        public void OnRefreshObject(ICoreChildProvider childProvider,DatabaseEntity oRefreshedObject, DescendancyList knownDescendancy)
        {
            var whitelistFilter = _tree.ModelFilter as WhiteListOnlyFilter;
            
            //someone somehow erased the pin filter? or overwrote it with another filter
            if(whitelistFilter == null)
                return;

            //if this is an object being refreshed is to the pinned object
            if (oRefreshedObject.Equals(_toPin))
                RefreshWhiteList(whitelistFilter, childProvider, oRefreshedObject);

            //if the descendancy is known and it includes the pinned object
             if(knownDescendancy != null && knownDescendancy.Parents.Contains(_toPin))
                 RefreshWhiteList(whitelistFilter, childProvider, oRefreshedObject);

        }

        private void RefreshWhiteList(WhiteListOnlyFilter whitelistFilter, ICoreChildProvider childProvider, DatabaseEntity oRefreshedObject)
        {
            //create a new whitelist filter that is the old one + oRefreshedObject + all children recursively
            var oldList = whitelistFilter.Whitelist;
            var newList = new HashSet<object>(oldList);

            //hashset ignores duplicates
            newList.Add(oRefreshedObject);

            //and all children of oRefreshedObject incase it has multiple new children under it
            foreach (var child in childProvider.GetAllChildrenRecursively(oRefreshedObject))
                newList.Add(child);

            //if there are new objects
            if (oldList.Count < newList.Count)
                _tree.ModelFilter = new WhiteListOnlyFilter(newList);
        }
    }
}
