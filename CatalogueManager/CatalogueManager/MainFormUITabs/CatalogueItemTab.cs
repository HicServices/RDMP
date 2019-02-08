// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Common.Validation;
using ReusableUIComponents;


namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// Each dataset (Catalogue) includes one or more virtual columns called CatalogueItems.  Each CatalogueItem is powered by an underlying columns in your data repository but there
    /// can be multiple CatalogueItems per column (for example if the DateOfBirth column is extractable either rounded to the nearest quarter or verbatim).  Thus CatalogueItems are both
    /// an extraction transform/rule set (See ExtractionInformationUI) and a descriptive entity which describes what the researcher will receive if they are given the column in an extract.
    /// This helpfully also lets you delete/restructure your data tables underneath without losing the descriptive data, validation rules, logging history etc of your datasets.
    /// 
    /// <para>This control lets you view/edit the descriptive metadata of a CatalogueItem in a dataset (Catalogue).</para>
    /// </summary>
    public partial class CatalogueItemTab : CatalogueItemTab_Design ,ISaveableUI
    {
        public CatalogueItemTab()
        {
            InitializeComponent();
            ObjectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
            AssociatedCollection = RDMPCollection.Catalogue;
            
            ci_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
        }

        private CatalogueItem _catalogueItem;
        
        bool objectSaverButton1_BeforeSave(DatabaseEntity databaseEntity)
        {
            //see if we need to display the dialog that lets the user sync up descriptions of multiuse columns e.g. CHI
            bool shouldDialogBeDisplayed;
            var propagate = new PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems(_activator,_catalogueItem, out shouldDialogBeDisplayed);
            propagate.RepositoryLocator = RepositoryLocator;

            //there are other CatalogueItems that share the same name as this one so give the user the option to propagate his changes to those too
            if (shouldDialogBeDisplayed)
            {
                DialogResult dialogResult = propagate.ShowDialog(this);

                if (dialogResult == DialogResult.Cancel)
                    return false;
            }

            return true;
        }

        public override void SetDatabaseObject(IActivateItems activator, CatalogueItem databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _catalogueItem = databaseObject;

            if (_catalogueItem.ExtractionInformation != null)
                AddToMenu(new ExecuteCommandActivate(activator, _catalogueItem.ExtractionInformation), "Go To Extraction Information");
            else
                AddToMenu(new ExecuteCommandMakeCatalogueItemExtractable(activator, _catalogueItem), "Make Extractable");

            if (_catalogueItem.ColumnInfo_ID != null)
                AddToMenu(new ExecuteCommandShow(activator, _catalogueItem.ColumnInfo, 0, true));
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, CatalogueItem databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(ci_tbID,"Text","ID",ci=>ci.ID);
            Bind(ci_tbName, "Text", "Name", ci => ci.Name);
            Bind(ci_tbStatisticalConsiderations,"Text", "Statistical_cons",ci=>ci.Statistical_cons);
            Bind(ci_tbResearchRelevance, "Text", "Research_relevance", ci => ci.Research_relevance);
            Bind(ci_tbDescription, "Text", "Description", ci => ci.Description);
            Bind(ci_tbTopics, "Text", "Topic", ci => ci.Topic);
            Bind(ci_ddPeriodicity, "SelectedItem", "Periodicity", ci => ci.Periodicity);
            Bind(ci_tbAggregationMethod, "Text", "Agg_method", ci => ci.Agg_method);
            Bind(ci_tbLimitations, "Text", "Limitations", ci => ci.Limitations);
            Bind(ci_tbComments,"Text", "Comments",ci=>ci.Comments);
        }


        private bool _expand = true;

        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !_expand;
            _expand = !_expand;
            btnExpandOrCollapse.Text = _expand ? "+" : "-";
        }


        public override string GetTabName()
        {
            return base.GetTabName() + " (" + _catalogueItem.Catalogue.Name + ")";
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueItemTab_Design, UserControl>))]
    public abstract class CatalogueItemTab_Design : RDMPSingleDatabaseObjectControl<CatalogueItem>
    {
    }
}
