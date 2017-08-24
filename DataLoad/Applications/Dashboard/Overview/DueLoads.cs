using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace Dashboard.Overview
{
    /// <summary>
    /// Lists all data loads that are configured for automation (See LoadPeriodicallyUI) but which have not yet been run yet and are due.  This could be because the automation service is
    /// not running or has crashed or because it is busy loading another dataset and will get to these when it has finished.
    /// </summary>
    public partial class DueLoads : RDMPUserControl
    {
        public DueLoads()
        {
            InitializeComponent();
        }


        public void Populate()
        {
            try
            {
                //clear any old items
                listView1.Items.Clear();

                var lockedCatalogues = RepositoryLocator.CatalogueRepository.GetAllAutomationLockedCatalogues();

                LoadPeriodically[] all = RepositoryLocator.CatalogueRepository.GetAllObjects<LoadPeriodically>();
                LoadPeriodically[] due = all.Where(lp => lp.IsLoadDue(lockedCatalogues)).ToArray();

                gbDueLoads.Text = "Due Loads (" + due.Length + "/" + all.Length + "):";

                foreach (LoadPeriodically lp in due)
                {
                    var lmd = lp.LoadMetadata;

                    ListViewItem listViewItem = listView1.Items.Add(lmd.Name);

                    DateTime dueDate = lp.WhenIsNextLoadDue();
                    listViewItem.SubItems.Add(dueDate.ToString());

                }

                foreach (ColumnHeader c in listView1.Columns)
                    c.Width = -2;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(this.GetType().Name + " failed to load data", e);
            }
        }
    }
}
