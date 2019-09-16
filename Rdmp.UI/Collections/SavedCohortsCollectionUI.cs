// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// RDMP Collection which shows all the Cohorts that have been committed to RDMP accross all Projects / Cohort Sources.
    /// </summary>
    public partial class SavedCohortsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;

        public SavedCohortsCollectionUI()
        {
            InitializeComponent();

            olvProjectNumber.AspectGetter = AspectGetter_ProjectNumber;
            olvVersion.AspectGetter = AspectGetter_Version;
        }

        private object AspectGetter_Version(object rowObject)
        {
            var c = rowObject as ExtractableCohort;

            if (c != null)
                return c.ExternalVersion;

            return null;
        }

        private object AspectGetter_ProjectNumber(object rowObject)
        {
            var c = rowObject as ExtractableCohort;

            if (c != null)
                return c.ExternalProjectNumber;

            return null;
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            CommonTreeFunctionality.SetUp(RDMPCollection.SavedCohorts, tlvSavedCohorts,_activator,olvName,olvName);
            
            tlvSavedCohorts.AddObject(((DataExportChildProvider)_activator.CoreChildProvider).RootCohortsNode);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public static bool IsRootObject(object root)
        {
            return root is AllCohortsNode;
        }
    }
}
