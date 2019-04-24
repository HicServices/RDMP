// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using BrightIdeasSoftware;
using CatalogueManager.Collections.Providers.Filtering;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using Rdmp.Core.CatalogueLibrary.Data.Governance;
using Rdmp.Core.CatalogueLibrary.Nodes;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Settings;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Main window for Data Management, this Control lets you view all your datasets, curate descriptive metadata, configure extractable columns, generate reports etc
    /// 
    /// <para>The tree shows you all the datasets you have configured for use with the RDMP.  Double clicking on a dataset (called a Catalogue) will show you the descriptive data you 
    /// have recorded. Right clicking a Catalogue will give you access to operations relevant to Catalogues (e.g. viewing dataset extraction logic if any).  Right clicking a
    /// CatalogueItem will give you access to operations relevant to CatalogueItems (e.g. adding an Issue).  And so on.</para>
    /// 
    /// <para>Each Catalogue has 1 or more CatalogueItems (visible through the CatalogueItems tab), these are the columns in the dataset that are maintained by RDMP. If you have very 
    /// wide data tables with hundreds of columns you might only configure a subset of those columns (the ones most useful  to researchers) for extraction.</para>
    /// 
    /// <para>You can also drag Catalogues between folders or into other Controls (e.g. dragging a Catalogue into a CohortIdentificationCollectionUI container to add the dataset to the identification
    /// criteria).</para>
    /// 
    /// <para>Pressing the Del key will prompt you to delete the selected item.</para>
    /// 
    /// <para>By default Deprecated, Internal and ColdStorage Catalogues do not appear, you can turn visibility of these on by selecting the relevant tick boxes.</para>
    /// 
    /// <para>Finally you can launch 'Checking' for every dataset, this will attempt to verify the extraction SQL you
    /// have configured for each dataset and to ensure that it runs and that at least 1 row of data is returned.  Checking all the datasets can take a while so runs asynchronously.</para>
    /// </summary>
    public partial class CatalogueCollectionUI : RDMPCollectionUI
    {
        private IActivateItems _activator;
        
        private Catalogue[] _allCatalogues;

        //constructor

        private bool bLoading = true;
        public CatalogueCollectionUI()
        {
            InitializeComponent();
            
            cbShowInternal.Checked = UserSettings.ShowInternalCatalogues;
            cbShowDeprecated.Checked = UserSettings.ShowDeprecatedCatalogues ;
            cbShowColdStorage.Checked = UserSettings.ShowColdStorageCatalogues;
            cbProjectSpecific.Checked = UserSettings.ShowProjectSpecificCatalogues;
            cbShowNonExtractable.Checked = UserSettings.ShowNonExtractableCatalogues;

            olvFilters.AspectGetter += FilterAspectGetter;
            olvFilters.IsVisible = UserSettings.ShowColumnFilters;
            olvFilters.VisibilityChanged += (s, e) => UserSettings.ShowColumnFilters = ((OLVColumn)s).IsVisible;
            
            
            bLoading = false;
        }

        //The color to highlight each Catalogue based on its extractability status
        
        
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool isFirstTime = true;

        public void RefreshUIFromDatabase(object oRefreshFrom)
        {   
            //if there are new catalogues we don't already have in our tree
            if (_allCatalogues != null)
            {
                var newCatalogues = CommonTreeFunctionality.CoreChildProvider.AllCatalogues.Except(_allCatalogues);
                if (newCatalogues.Any())
                {
                    oRefreshFrom = CatalogueFolder.Root;//refresh from the root instead
                    tlvCatalogues.RefreshObject(oRefreshFrom);
                }
            }

            _allCatalogues = CommonTreeFunctionality.CoreChildProvider.AllCatalogues;
            
            if(isFirstTime || Equals(oRefreshFrom, CatalogueFolder.Root))
            {
                tlvCatalogues.RefreshObject(CatalogueFolder.Root);
                ExpandAllFolders(CatalogueFolder.Root);
                isFirstTime = false;
            }

        }

        private void ExpandAllFolders(CatalogueFolder model)
        {
            //expand it
            tlvCatalogues.Expand(model);
            
            foreach (var folder in tlvCatalogues.GetChildren(model).OfType<CatalogueFolder>())
                ExpandAllFolders(folder);
        }

        public void ApplyFilters()
        {
            if(bLoading)
                return;

            UserSettings.ShowInternalCatalogues = cbShowInternal.Checked;
            UserSettings.ShowDeprecatedCatalogues = cbShowDeprecated.Checked;
            UserSettings.ShowColdStorageCatalogues = cbShowColdStorage.Checked;
            UserSettings.ShowProjectSpecificCatalogues = cbProjectSpecific.Checked;
            UserSettings.ShowNonExtractableCatalogues = cbShowNonExtractable.Checked;
            
            tlvCatalogues.UseFiltering = true;
            tlvCatalogues.ModelFilter = new CatalogueCollectionFilter(_activator.CoreChildProvider);
        }

        public enum HighlightCatalogueType
        {
            None,
            Extractable,
            ExtractionBroken,
            TOP1Worked
        }
        
        private object FilterAspectGetter(object rowObject)
        {
            var cataItem = rowObject as CatalogueItem;
            if (cataItem != null)
                return _activator.CoreChildProvider.GetAllChildrenRecursively(cataItem).OfType<IFilter>().Count();

            return null;
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;

            _activator.Emphasise += _activator_Emphasise;

            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonTreeFunctionality.SetUp(
                RDMPCollection.Catalogue,
                tlvCatalogues,
                _activator,
                olvColumn1, //the icon column
                //we have our own custom filter logic so no need to pass tbFilter
                olvColumn1 //also the renameable column
                );

            CommonTreeFunctionality.MaintainRootObjects = new[]
            {
                typeof (AllGovernanceNode)
            };

            //Things that are always visible regardless
            CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = (a)=>new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewCatalogueByImportingFile(a),
                new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(a),
                new ExecuteCommandCreateNewEmptyCatalogue(a)
            };

            _activator.RefreshBus.EstablishLifetimeSubscription(this);

            tlvCatalogues.AddObject(activator.CoreChildProvider.AllGovernanceNode);
            tlvCatalogues.AddObject(CatalogueFolder.Root);
            ApplyFilters();

            RefreshUIFromDatabase(CatalogueFolder.Root);
        }

        void _activator_Emphasise(object sender, ItemActivation.Emphasis.EmphasiseEventArgs args)
        {
            //user wants this object emphasised
            var c = args.Request.ObjectToEmphasise as Catalogue;
            
            if (c == null)
            {
                var descendancy = _activator.CoreChildProvider.GetDescendancyListIfAnyFor(args.Request.ObjectToEmphasise);

                if (descendancy != null)
                    c = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();
            }
            
            if (c != null)
            {
                if ((c.IsColdStorageDataset || c.IsDeprecated || c.IsInternalDataset))
                {
                    //trouble is our flags might be hiding it so make sure it is visible
                    cbShowColdStorage.Checked = cbShowColdStorage.Checked || c.IsColdStorageDataset;
                    cbShowDeprecated.Checked = cbShowDeprecated.Checked || c.IsDeprecated;
                    cbShowInternal.Checked = cbShowInternal.Checked || c.IsInternalDataset;
                }

                var isExtractable = c.GetExtractabilityStatus(null);

                cbShowNonExtractable.Checked = cbShowNonExtractable.Checked || isExtractable == null || isExtractable.IsExtractable == false;
                
                ApplyFilters();
            }
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var o = e.Object;
            var cata = o as Catalogue;

            if(o is GovernancePeriod || o is GovernanceDocument)
                tlvCatalogues.RefreshObject(_activator.CoreChildProvider.AllGovernanceNode);

            if (cata != null)
            {
                var oldFolder = tlvCatalogues.GetParent(cata) as CatalogueFolder;

                //if theres a change to the folder of the catalogue or it is a new Catalogue (no parent folder) we have to rebuild the entire tree
                if (oldFolder == null || !oldFolder.Path.Equals(cata.Folder.Path))
                    RefreshUIFromDatabase(CatalogueFolder.Root);
                else
                    RefreshUIFromDatabase(o);
                return;
            }

            if( o is CatalogueItem || o is AggregateConfiguration ||
            o is ColumnInfo || o is TableInfo || o is ExtractionFilter || o is ExtractionFilterParameter ||
            o is ExtractionFilterParameterSet || o is ExtractionInformation ||
            o is AggregateFilterContainer || o is AggregateFilter || o is AggregateFilterParameter)
            {
                //then refresh us
                RefreshUIFromDatabase(o);
            }
        }
        
        private void rbFlag_CheckedChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        public static bool IsRootObject(object root)
        {
            return root.Equals(CatalogueFolder.Root) || root is AllGovernanceNode;
        }

        public void SelectCatalogue(Catalogue catalogue)
        {
            tlvCatalogues.SelectObject(catalogue, true);
        }
    }
}
