// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Icons;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.Raceway
{
    /// <summary>
    /// Allows you to quickly view the timespan of each of your datasets, which sections of your datasets are failing validation (e.g. 'Prescribing' 2001-2002 records are all failing
    /// validation but 2003 onwards are fine) and identify any gaps in your record coverage.
    /// 
    /// <para>Each dataset appears as a green/red bar along a shared axis (See RacewayRenderAreaUI).  You can switch from viewing all months for which you have data, only
    /// the last decade, year or last 6 months.</para>
    /// 
    /// <para>By default the row height of each bar in a dataset indicates the proportion of records in that month relative to the average number of records per month, this allows you to see for
    /// example the gradual increase in volume of records per month in a dataset and identify any periods where it doubles (may indicate duplication) or a hole appears.  If you tick 'Ignore
    /// Row Counts' then full bars will appear only, this lets you identify which datasets are responsible for sparse errors (e.g. if 'Biochemistry' has 1,00,000,000 and some records have
    /// dates sprinkled between 1900-01-01 and 2000-01-01 then these will appear on the axis but won't be visible due to how sparse the number of error records are).</para>
    /// </summary>
    public partial class DatasetRaceway : RDMPUserControl, IDashboardableControl
    {
        private ToolStripButton btnAddCatalogue = new ToolStripButton("Add Catalogue"){Name= "btnAddCatalogue" };
        private ToolStripButton btnRemoveAll = new ToolStripButton("Clear",FamFamFamIcons.delete_multi.ImageToBitmap()) { Name = "btnRemoveAll" };
        private ToolStripButton btnAddExtractableDatasetPackage = new ToolStripButton("Add Package") { Name = "btnAddExtractableDatasetPackage" };
        private ToolStripLabel toolStripLabel1 = new ToolStripLabel("Show Period") { Name = "toolStripLabel1" };
        private ToolStripComboBox ddShowPeriod = new ToolStripComboBox() { Name = "ddShowPeriod", Size = new Size(121, 25) };
        private ToolStripButton cbIgnoreRowCounts = new ToolStripButton() { Name = "cbIgnoreRowCounts" };

        public DatasetRaceway()
        {
            InitializeComponent();

            this.btnAddCatalogue.Click += btnAddCatalogue_Click;
            this.btnRemoveAll.Click += btnRemoveAll_Click;
            this.ddShowPeriod.DropDownClosed += ddShowPeriod_DropDownClosed;

            this.cbIgnoreRowCounts.CheckOnClick = true;
            this.cbIgnoreRowCounts.CheckedChanged += this.cbIgnoreRowCounts_CheckedChanged;
            this.btnAddExtractableDatasetPackage.Click += btnAddExtractableDatasetPackage_Click;
            
            ddShowPeriod.ComboBox.DataSource = Enum.GetValues(typeof (RacewayShowPeriod));
            
            btnRemoveAll.Image = FamFamFamIcons.delete_multi.ImageToBitmap();
            _ignoreRowCounts = CatalogueIcons.RowCounts_Ignore.ToBitmap();
            _respectRowCounts = CatalogueIcons.RowCounts_Respect.ToBitmap();
        }

        private DashboardControl _dashboardControlDatabaseRecord;
        private DatasetRacewayObjectCollection _collection;
        private IActivateItems _activator;
        private bool _isEditmodeOn;

        private bool isFirstTime = true;
        private Bitmap _ignoreRowCounts;
        private Bitmap _respectRowCounts;

        public void GenerateChart()
        {
            CommonFunctionality.ResetChecks();
            
            var allCatalogues = _collection.GetCatalogues();

            Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> cataloguesToAdd = new Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>>();

            DQERepository dqeRepository;

            try
            {
                dqeRepository = new DQERepository(_activator.RepositoryLocator.CatalogueRepository);
            }
            catch (NotSupportedException e)
            {
                CommonFunctionality.Fatal("Failed to get DQE Repository",e);
                return;
            }
            
            foreach (var cata in allCatalogues.OrderBy(c => c.Name))
            {
                var eval = dqeRepository.GetMostRecentEvaluationFor(cata);
                
                Dictionary<DateTime, ArchivalPeriodicityCount> dictionary = null;

                if (eval != null)
                    dictionary = PeriodicityState.GetPeriodicityCountsForEvaluation(eval,true);

                cataloguesToAdd.Add(cata, dictionary);
            }
            
            //every month seen in every dataset ever
            var buckets = GetBuckets(cataloguesToAdd);

            racewayRenderArea.AddTracks(_activator,cataloguesToAdd, buckets, _collection.IgnoreRows);
            racewayRenderArea.Refresh();

            this.Invalidate();
        }

        private DateTime[] GetBuckets(Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> cataloguesDictionary)
        {
            var buckets = new List<DateTime>();

            foreach (var dictionary in cataloguesDictionary.Values)
                if (dictionary != null)
                    foreach (DateTime dt in dictionary.Keys)
                    {
                        //if it is before the start or after today
                        if (dt < GetUserPickedStartDate() || dt.Date > DateTime.Now.Date)
                            continue;

                        if (!buckets.Contains(dt))
                            buckets.Add(dt);
                    }

            return buckets.OrderBy(d => d).ToArray(); 
        }

        private DateTime GetUserPickedStartDate()
        {
            switch (_collection.ShowPeriod)
            {
                case RacewayShowPeriod.AllTime:
                    return DateTime.MinValue;
                case RacewayShowPeriod.LastDecade:
                    return DateTime.Now.AddYears(-10);
                case RacewayShowPeriod.LastYear:
                    return DateTime.Now.AddYears(-1);
                case RacewayShowPeriod.LastSixMonths:
                    return DateTime.Now.AddMonths(-6);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public enum RacewayShowPeriod
        {
            AllTime,
            LastDecade,
            LastYear,
            LastSixMonths
        }
        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _activator = activator;
            _collection = (DatasetRacewayObjectCollection) collection;

            SetItemActivator(activator);

            btnAddCatalogue.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Import).ToBitmap();
            btnAddExtractableDatasetPackage.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Import).ToBitmap();
            
            ddShowPeriod.ComboBox.SelectedItem = _collection.ShowPeriod;
            cbIgnoreRowCounts.Checked = _collection.IgnoreRows;
            UpdateIgnoreRowCountCheckBoxIconAndText();

            if(isFirstTime)
            {
                isFirstTime = false;
                racewayRenderArea.RequestDeletion += (c) =>
                {
                    _collection.RemoveCatalogue(c);
                    SaveCollectionChanges();
                };
                racewayRenderArea.NotifyEditModeChange(_isEditmodeOn);
            }

            CommonFunctionality.Add(btnAddCatalogue);
            CommonFunctionality.Add(btnAddExtractableDatasetPackage);
            CommonFunctionality.Add(btnRemoveAll);
            CommonFunctionality.Add(toolStripLabel1);
            CommonFunctionality.Add(ddShowPeriod);
            CommonFunctionality.Add(cbIgnoreRowCounts);

            GenerateChart();

        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return Text;
        }

        public string GetTabToolTip()
        {
            return null;
        }

        public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
        {
            _dashboardControlDatabaseRecord = databaseRecord;

            return new DatasetRacewayObjectCollection();
        }

        public void NotifyEditModeChange(bool isEditModeOn)
        {
            _isEditmodeOn = isEditModeOn;

            racewayRenderArea.NotifyEditModeChange(isEditModeOn);

            CommonFunctionality.ToolStrip.Visible = isEditModeOn;
        }
        
        private void btnAddCatalogue_Click(object sender, EventArgs e)
        {
            if(_activator.SelectObjects(new DialogArgs { 
                TaskDescription = "Choose which new Catalogues should be represented in the diagram.",
            },
                _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>()
                .Except(_collection.GetCatalogues())
                .ToArray(),
                out var selected))
            {
                foreach (var catalogue in selected)
                    AddCatalogue(catalogue);

                SaveCollectionChanges();
                GenerateChart();
            }
        }

        private void SaveCollectionChanges()
        {
            _dashboardControlDatabaseRecord.SaveCollectionState(_collection);
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            _collection.ClearDatabaseObjects();

            SaveCollectionChanges();
            GenerateChart();
        }

        private void btnAddExtractableDatasetPackage_Click(object sender, EventArgs e)
        {
            var dataExportChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

            if(dataExportChildProvider == null)
                return;

            if(Activator.SelectObject(new DialogArgs
            {
                TaskDescription = "Choose a Package.  All Catalogues in the Package will be added to the diagram.",

            }, dataExportChildProvider.AllPackages,out var packageToAdd))
            {
                var contents = _activator.RepositoryLocator.DataExportRepository.GetAllDataSets(packageToAdd, dataExportChildProvider.ExtractableDataSets);

                foreach (var cata in contents.Select(ds => ds.Catalogue))
                {
                    if(!_collection.GetCatalogues().Contains(cata))
                        AddCatalogue((Catalogue) cata);
                }
                
                SaveCollectionChanges();
                GenerateChart();
            }

        }

        private void AddCatalogue(Catalogue cata)
        {
            _collection.AddCatalogue(cata);
        }

        private void ddShowPeriod_DropDownClosed(object sender, EventArgs e)
        {
            if (ddShowPeriod.SelectedItem == null)
                return;

            var newPeriod = (RacewayShowPeriod)ddShowPeriod.SelectedItem;

            //no change
            if (newPeriod == _collection.ShowPeriod)
                return;

            _collection.ShowPeriod = newPeriod;
            GenerateChart();
            SaveCollectionChanges();
        }

        private void cbIgnoreRowCounts_CheckedChanged(object sender, EventArgs e)
        {
            
            UpdateIgnoreRowCountCheckBoxIconAndText();
            _collection.IgnoreRows = cbIgnoreRowCounts.Checked;
            GenerateChart();
            SaveCollectionChanges();
        }

        private void UpdateIgnoreRowCountCheckBoxIconAndText()
        {
            cbIgnoreRowCounts.Image = cbIgnoreRowCounts.Checked ? _ignoreRowCounts : _respectRowCounts;
        }
    }
}
