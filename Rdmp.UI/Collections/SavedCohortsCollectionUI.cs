// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CohortUI.ImportCustomData;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// RDMP Collection which shows all the Cohorts that have been committed to RDMP accross all Projects / Cohort Sources.
    /// </summary>
    public partial class SavedCohortsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        public SavedCohortsCollectionUI()
        {
            InitializeComponent();

            olvProjectNumber.AspectGetter = AspectGetter_ProjectNumber;
            olvVersion.AspectGetter = AspectGetter_Version;
            olvVersion.IsEditable = false;
            olvProjectNumber.IsEditable = false;
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
            base.SetItemActivator(activator);

            CommonTreeFunctionality.SetUp(RDMPCollection.SavedCohorts, tlvSavedCohorts,Activator,olvName,olvName);
            
            tlvSavedCohorts.AddObject(((DataExportChildProvider)Activator.CoreChildProvider).RootCohortsNode);

            SetupToolStrip();

            Activator.RefreshBus.EstablishLifetimeSubscription(this);

        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            SetupToolStrip();
        }

        private void SetupToolStrip()
        {
            CommonFunctionality.ClearToolStrip();
            CommonFunctionality.Add(new ExecuteCommandCreateNewCohortFromFile(Activator,null),GlobalStrings.FromFile,null,"New...");
            CommonFunctionality.Add(new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(Activator,null),"From Query",null,"New...");
        }

        public static bool IsRootObject(object root)
        {
            return root is AllCohortsNode;
        }
    }
}
