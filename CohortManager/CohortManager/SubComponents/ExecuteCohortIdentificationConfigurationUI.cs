using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CohortManager.SubComponents
{
    /// <summary>
    /// Hosts the CohortCompilerUI (See CohortCompilerUI for documentation on how this actually works) and allows you to set the QueryCachingServer for a Cohort Identification Configuration. 
    /// </summary>
    public partial class ExecuteCohortIdentificationConfigurationUI : ExecuteCohortIdentificationConfigurationUI_Design
    {
        private CohortIdentificationConfiguration _configuration;
        
        public ExecuteCohortIdentificationConfigurationUI()
        {
            InitializeComponent();
            queryCachingServerSelector.SelectedServerChanged += queryCachingServerSelector_SelectedServerChanged;
        }

        void queryCachingServerSelector_SelectedServerChanged()
        {
            if (queryCachingServerSelector.SelecteExternalDatabaseServer == null)
                _configuration.QueryCachingServer_ID = null;
            else
                _configuration.QueryCachingServer_ID = queryCachingServerSelector.SelecteExternalDatabaseServer.ID;

            _configuration.SaveToDatabase();
            _activator.RefreshBus.Publish(queryCachingServerSelector,new RefreshObjectEventArgs(_configuration));

        }

        public override void SetDatabaseObject(IActivateItems activator, CohortIdentificationConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _configuration = databaseObject;
            
            if (_configuration.QueryCachingServer_ID == null)
                queryCachingServerSelector.SelecteExternalDatabaseServer = null;
            else
                queryCachingServerSelector.SelecteExternalDatabaseServer = _configuration.QueryCachingServer;

            CohortCompilerUI1.SetDatabaseObject(activator,databaseObject);
        }

        public override string GetTabName()
        {
            return "Execute:" + base.GetTabName();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExecuteCohortIdentificationConfigurationUI_Design, UserControl>))]
    public abstract class ExecuteCohortIdentificationConfigurationUI_Design:RDMPSingleDatabaseObjectControl<CohortIdentificationConfiguration>
    {
    }
}
