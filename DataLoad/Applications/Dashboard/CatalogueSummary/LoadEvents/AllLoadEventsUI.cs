using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using HIC.Logging.PastEvents;
using ReusableUIComponents;

namespace Dashboard.CatalogueSummary.LoadEvents
{
    /// <summary>
    /// Shows two LoadEventsTreeView controls, one for loads audited in the Live logging server and one for loads audited in the Test logging server (if you have one).  See 
    /// LoadEventsTreeView for a description of the full logging functionality.
    /// </summary>
    public partial class AllLoadEventsUI : AllLoadEventsUI_Design
    {
        
        //constructor
        public AllLoadEventsUI()
        {
            InitializeComponent();

            liveLoads.IsTestServerInterrogation = false;
            testLoads.IsTestServerInterrogation = true;

            AssociatedCollection = RDMPCollection.DataLoad;
        }
        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            testLoads.ApplyFilter(tbFilter.Text);
            liveLoads.ApplyFilter(tbFilter.Text);
        }

        public override void SetDatabaseObject(IActivateItems activator, LoadMetadata databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);

            if (databaseObject.AreLiveAndTestLoggingDifferent())
            {
                if (!tabControl1.TabPages.Contains(tabPage2))
                    tabControl1.TabPages.Add(tabPage2);

                testLoads.SetDatabaseObject(activator, databaseObject);
            }
            else
                tabControl1.TabPages.Remove(tabPage2);

            liveLoads.SetDatabaseObject(activator,databaseObject);
            
        }

        public override string GetTabName()
        {
            return "Log:" + base.GetTabName();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<AllLoadEventsUI_Design, UserControl>))]
    public abstract class AllLoadEventsUI_Design:RDMPSingleDatabaseObjectControl<LoadMetadata>
    {
    }
}
