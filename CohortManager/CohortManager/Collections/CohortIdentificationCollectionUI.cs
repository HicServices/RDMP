// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.CohortNodes;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CohortManager.CommandExecution.AtomicCommands;
using CohortManager.Menus;
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace CohortManager.Collections
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
        private IActivateItems _activator;
        
        public CohortIdentificationCollectionUI()
        {
            InitializeComponent();
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            
            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonFunctionality.SetUp(
                RDMPCollection.Cohort, 
                tlvCohortIdentificationConfigurations,
                _activator,
                olvName,//column with the icon
                olvName//column that can be renamed
                
                );
            CommonFunctionality.AxeChildren = new Type[]{typeof (CohortIdentificationConfiguration)};
            
            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;

            if (dataExportChildProvider == null)
            {
                CommonFunctionality.MaintainRootObjects = new Type[] { typeof(CohortIdentificationConfiguration) };
                tlvCohortIdentificationConfigurations.AddObjects(_activator.CoreChildProvider.AllCohortIdentificationConfigurations);
            }
            else
            {
                CommonFunctionality.MaintainRootObjects = new Type[] { typeof(AllProjectCohortIdentificationConfigurationsNode), typeof(AllFreeCohortIdentificationConfigurationsNode) };
                tlvCohortIdentificationConfigurations.AddObject(dataExportChildProvider.AllProjectCohortIdentificationConfigurationsNode);
                tlvCohortIdentificationConfigurations.AddObject(dataExportChildProvider.AllFreeCohortIdentificationConfigurationsNode);
            }

            CommonFunctionality.WhitespaceRightClickMenuCommandsGetter = (a)=>new IAtomicCommand[]{new ExecuteCommandCreateNewCohortIdentificationConfiguration(a)};

            _activator.RefreshBus.EstablishLifetimeSubscription(this);
            
            
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
