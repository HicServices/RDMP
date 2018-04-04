using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RDMPStartup.Events;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    /// <summary>
    /// There are 3 'tiers' of platform database in the RDMP:
    ///  
    /// <para>Tier 1 - Catalogue Manager database and Data Export Manager database (optionally).  These are stored in user settings and configured through ChoosePlatformDatabases</para>
    /// 
    /// <para>Tier 2 - Databases created and referenced in the Catalogue Manager database such as Logging databases, ANO databases, Data Quality Engine Reporting databases etc (See ManageExternalServers)</para>
    /// 
    /// <para>Tier 3 - Plugin databases, these are wild and uncontrollable by RDMP.  All functionality to interact with these is stores in the plugin that created them.  For example you
    /// might decide that you wanted to do your own unique anonymisation method and create a plugin which uses its own database schema to store/generate anonymous identifiers.</para>
    /// 
    /// <para>This control indicates that the RDMP startup process has found a plugin which has tier 3 databases associated with it.  You will see any/all tier 3 databases found by the 
    /// plugin in StartupUIMainForm under tier 3 (See ManagedDatabaseUI).</para>
    /// </summary>
    public partial class PluginPatcherUI : UserControl
    {
        public PluginPatcherUI()
        {
            InitializeComponent();

            DoTransparencyProperly.ThisHoversOver(ragSmiley1,lblTypeName);
        }

        public void HandlePatcherFound(PluginPatcherFoundEventArgs eventArgs)
        {
            lblTypeName.Text = eventArgs.Type.Name;

            switch (eventArgs.Status)
            {
                case PluginPatcherStatus.CouldNotConstruct:
                    ragSmiley1.Fatal(eventArgs.Exception);
                    break;
                case PluginPatcherStatus.Healthy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
