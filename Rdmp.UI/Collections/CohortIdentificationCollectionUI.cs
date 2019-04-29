// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Displays all the cohort identification configurations you have configured in RDMP. Cohort Identification Configurations (CIC) are created to identify specific patients e.g. 'all patients 
    /// with 3 or more prescriptions for a diabetes drug or who have been hospitalised for an amputation'.  Each CIC achieves it's goal by combining Cohort Sets with Set operations (UNION,
    /// INTERSECT, EXCEPT) for example Cohort Set 1 '3+ diabetes drug prescriptions' UNION 'hospital admissions for amputations'.  Cohort sets can be from the same or different data sets (as
    /// long as they have a common identifier).
    /// </summary>
    public partial class CohortIdentificationCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        //for expand all/ collapse all
        public CohortIdentificationCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            base.SetItemActivator(activator);

            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonTreeFunctionality.SetUp(
                RDMPCollection.Cohort, 
                tlvCohortIdentificationConfigurations,
                Activator,
                olvName,//column with the icon
                olvName//column that can be renamed
                
                );
            CommonTreeFunctionality.AxeChildren = new Type[]{typeof (CohortIdentificationConfiguration)};
            
            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;

            if (dataExportChildProvider == null)
            {
                CommonTreeFunctionality.MaintainRootObjects = new Type[] { typeof(CohortIdentificationConfiguration) };
                tlvCohortIdentificationConfigurations.AddObjects(Activator.CoreChildProvider.AllCohortIdentificationConfigurations);
            }
            else
            {
                CommonTreeFunctionality.MaintainRootObjects = new Type[] { typeof(AllProjectCohortIdentificationConfigurationsNode), typeof(AllFreeCohortIdentificationConfigurationsNode) };
                tlvCohortIdentificationConfigurations.AddObject(dataExportChildProvider.AllProjectCohortIdentificationConfigurationsNode);
                tlvCohortIdentificationConfigurations.AddObject(dataExportChildProvider.AllFreeCohortIdentificationConfigurationsNode);
            }

            CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = (a)=>new IAtomicCommand[]{new ExecuteCommandCreateNewCohortIdentificationConfiguration(a)};

            Activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            
        }
        
        public static bool IsRootObject(object root)
        {
            return root is CohortIdentificationConfiguration || root is AllProjectCohortIdentificationConfigurationsNode || root is AllFreeCohortIdentificationConfigurationsNode;
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }
    }
}
