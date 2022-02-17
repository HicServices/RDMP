// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.UI.Collections.Providers.Filtering;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;


namespace Rdmp.UI.Collections
{
    /// <summary>
    /// User interface component for indicating to the user that there is a Pinned object in an RDMPCollectionUI.  Bumps the collection tree down 19 pixels (or docks top).
    /// And then assembles a always show list filter that will show only the pinned object hierarchy and children.
    /// </summary>
    [TechnicalUI]
    public partial class CollectionPinFilterUI : UserControl
    {
        private TreeListView _tree;
        public event EventHandler UnApplied;
        IActivateItems _activator;

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

            typeof(GovernancePeriod),
            
            //TableInfoCollectionUI pinnables
            typeof(ExtractableDataSetPackage),
            typeof(RemoteRDMP),
            typeof(ExternalDatabaseServer),
            typeof(DataAccessCredentials),
            typeof(ANOTable),
            typeof(TableInfo),

            //saved cohorts collection
            typeof(ExtractableCohort)
        };

        /// <summary>
        /// Records the IModelFilter that was on the Tree before we arrived on the scene.  This lets us unapply from the tree but set the IModelFilter back to what
        /// it was before we were applied (e.g. hide deprecated catalogues)
        /// </summary>
        private IModelFilter _beforeModelFilter;

        /// <summary>
        /// Same as _beforeModelFilter but the bool for whether it was applied before we came along
        /// </summary>
        private bool _beforeUseFiltering;
        private object _toPin;
        
        public static bool IsPinnableType(object o)
        {
            //Project specific Catalogues are not pinnable because they are children of Projects
            var cata = o as Catalogue;

            //Catalogues are pinnable so long as they are not ProjectSpecific
            if (cata != null)
                return !cata.IsProjectSpecific(null);

            return PinnableTypes.Contains(o.GetType());
        }

        public void ApplyToTree(IActivateItems activator,TreeListView tree, IMapsDirectlyToDatabaseTable objectToEmphasise, DescendancyList descendancy)
        {
            _activator = activator;

            if (_tree != null)
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

            _beforeModelFilter = _tree.ModelFilter;
            _beforeUseFiltering = _tree.UseFiltering;

            RefreshAlwaysShowList(_activator.CoreChildProvider,descendancy);
        }

        private void RefreshAlwaysShowList(ICoreChildProvider childProvider,DescendancyList descendancy)
        {
            //get rid of all objects that are not the object to emphasise tree either as parents or as children (recursive)
            List<object> alwaysShowList = new List<object>();

            //add parents to alwaysShowList
            if (descendancy != null && descendancy.Parents.Any())
                alwaysShowList.AddRange(descendancy.Parents);

            alwaysShowList.Add(_toPin);

            alwaysShowList.AddRange(childProvider.GetAllChildrenRecursively(_toPin));
            
            _tree.UseFiltering = true;
            _tree.ModelFilter = new AlwaysShowListOnlyFilter(alwaysShowList);
        }

        public void UnApplyToTree()
        {
            if (_tree == null)
                return;

            //remove ourselves
            _tree.Parent.Controls.Remove(this);
            
            //clear the tree selection since the selected index will now be invalid having shown/hidden half the tree
            _tree.SelectedObject = null;
            _tree.SelectedObjects = null;

            //bump the tree down again
            _tree.Top -= 19;
            _tree.Height += 19;
            
            //add the original objects back in again (clear filter)
            _tree.UseFiltering = _beforeUseFiltering;
            _tree.ModelFilter = _beforeModelFilter;
            _tree = null;

            var h = UnApplied;
            if(h != null)
                h(this,new EventArgs());
        }

        private void pbRemoveFilter_Click(object sender, EventArgs e)
        {
            if(_activator != null && _toPin is DatabaseEntity de)
            {
                var cmd = new ExecuteCommandUnpin(_activator,de);
                cmd.Execute();
            }
            else
                UnApplyToTree();

        }

        public void OnRefreshObject(ICoreChildProvider childProvider, RefreshObjectEventArgs e)
        {
            if(_tree == null)
                return;

            var alwaysShowList = _tree.ModelFilter as AlwaysShowListOnlyFilter;

            if(_beforeModelFilter is CatalogueCollectionFilter f1)
                f1.ChildProvider = childProvider;

            if (_tree.ModelFilter is CatalogueCollectionFilter f2) 
                f2.ChildProvider = childProvider;

            //someone somehow erased the pin filter? or overwrote it with another filter
            if(alwaysShowList == null)
                return;

            if(_toPin != null)
                RefreshAlwaysShowList(childProvider,childProvider.GetDescendancyListIfAnyFor(_toPin));
        }
    }
}
