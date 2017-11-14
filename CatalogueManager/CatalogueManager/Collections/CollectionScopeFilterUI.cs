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
using CatalogueLibrary.Providers;
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

        public void ApplyToTree(ICoreChildProvider childProvider,TreeListView tree, IMapsDirectlyToDatabaseTable objectToEmphasise, DescendancyList descendancy)
        {
            if(_tree != null)
                throw new Exception("Scope filter is already applied to a tree");

            _tree = tree;
            lblFilter.Text = objectToEmphasise.ToString();
            
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
            
            whitelist.Add(objectToEmphasise);

            whitelist.AddRange(childProvider.GetAllChildrenRecursively(objectToEmphasise));

            _tree.UseFiltering = true;
            _tree.ModelFilter = new WhiteListOnlyFilter(whitelist);
        }

        public void UnApplyToTree()
        {
            if (_tree == null)
                throw new Exception("Cannot unapply filter because it is not currently applied to any tree");

            //remove ourselves
            _tree.Parent.Controls.Remove(this);
            
            //bump the tree down again
            _tree.Top -= 19;
            _tree.Height += 19;
            
            //add the original objects back in again (clear filter)
            _tree.UseFiltering = false;
            _tree.ModelFilter = null;
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
