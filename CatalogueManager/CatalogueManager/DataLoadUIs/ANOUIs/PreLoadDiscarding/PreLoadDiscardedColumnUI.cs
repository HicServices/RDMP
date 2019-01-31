using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DatabaseManagement.Operations;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding
{
    /// <summary>
    /// Lets you configure a column in a dataset as discarded during the load process (either completely - Oblivion or stored in an identifiers only area - StoreInIdentifiersDump).
    ///  
    /// <para>BACKGROUND:
    /// PreLoadDiscardedColumn(s) are an alternative (to ANOTables) way of anonymising dataset columns.  A well implemented anonymisation protocol will include both ANOTable substitutions 
    /// and PreLoadDiscardedColumns.  A PreLoadDiscardedColumn is a column containing identifiable which is NOT REQUIRED by anyone to use the dataset.  For example if you have a demography
    /// dataset with a PatientIdentifier, Forename and Surname then you can safely configure Forename and Surname as discarded columns because you have the unique patient identifier (which
    /// should have an ANOTable transform on it btw) to distinguish between patients when doing linkage. </para>
    /// 
    /// <para>There are 3 types of PreLoadDiscardedColumn, each is treated differently at data load time:</para>
    /// 
    /// <para>DiscardedColumnDestination:
    /// Oblivion - This column DOES NOT exist in the live data table but is created in the RAW load bubble so that the data can be loaded from supplied files.  The data is then deleted prior
    /// to the migration to STAGING
    /// StoreInIdentifiersDump - This column DOES NOT exist in the live data table but is created in the RAW load bubble, instead of being migrated to STAGING the data is stored in an 'identifiers
    /// only' area (the Identifier Dump) along with the PrimaryKeys of the data, this allows you to use the identifiers in debugging or to change your mind about anonymisation later on and reintroduce
    /// the discarded data back into your live database. 
    /// Dilute - This column DOES exist in the live data table but is diluted during load e.g. date of birth 2001-05-03 might be diluted to the first of the month (I know, its insane right but hey
    /// governance wants it!).</para>
    /// 
    /// <para>The theory is that data analysts do not need to know patient level identifiable data to do their jobs and researchers certainly don't.  This is all entirely optional and if you 
    /// do not want to anonymise your data repository then don't worry about this window.</para>
    /// 
    /// <para>
    /// Before using the form, make sure you have configured at least one IdentifierDump server.  This can be done through ManageExternalServers.  Select the Identifier Dump Server. 
    /// Next create some PreLoadDiscardedColumn(s) that correspond to supplied fields you do not want to go through to your live database during data load.  Each column must have a
    /// name and SqlDataType that matches what you are trying to load.  </para>
    /// 
    /// <para>If you already have a data table and you want to drop some of the columns from it then you can paste in a list of column names and any that match known columns will automatically 
    /// get created as the appropriate datatype/name.  After doing that you will have to manually drop the columns yourself on your server though.</para>
    ///  
    /// <para> </para>
    /// </summary>
    public partial class PreLoadDiscardedColumnUI : PreLoadDiscardedColumnUI_Design, ISaveableUI
    {
        public PreLoadDiscardedColumnUI()
        {
            InitializeComponent();
            ddDestination.DataSource = Enum.GetValues(typeof (DiscardedColumnDestination));
            AssociatedCollection = RDMPCollection.Tables;
        }

        public override void SetDatabaseObject(IActivateItems activator, PreLoadDiscardedColumn databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            AddChecks(databaseObject);
            StartChecking();

            AddToMenu(new SetDumpServerMenuItem(_activator, databaseObject.TableInfo));
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, PreLoadDiscardedColumn databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID,"Text", "ID",p=>p.ID);
            Bind(tbRuntimeColumnName,"Text","RuntimeColumnName",p=>p.RuntimeColumnName);
            Bind(tbSqlDataType,"Text","SqlDataType", p=>p.SqlDataType);
            Bind(ddDestination,"SelectedItem", "Destination",p=>p.Destination);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<PreLoadDiscardedColumnUI_Design, UserControl>))]
    public abstract class PreLoadDiscardedColumnUI_Design : RDMPSingleDatabaseObjectControl<PreLoadDiscardedColumn>
    {
        
    }
}

