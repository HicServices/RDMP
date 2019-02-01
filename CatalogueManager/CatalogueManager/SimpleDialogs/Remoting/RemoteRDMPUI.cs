using System.ComponentModel;
using System.Windows.Forms;
using CatalogueLibrary.Data.Remoting;
using CatalogueManager.Collections;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Remoting
{
    /// <summary>
    /// Lets you change the settings for a RemoteRDMP which is a set of web credentials / url to reach another RDMP instance across the network/internet via a web service.
    /// </summary>
    public partial class RemoteRDMPUI : RemoteRDMPUI_Design, ISaveableUI
    {
        public RemoteRDMPUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Tables;
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, RemoteRDMP databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID,"Text","ID",r=>r.ID);
            Bind(tbName, "Text", "Name", r => r.Name);
            Bind(tbBaseUrl, "Text", "URL", r => r.URL);
            Bind(tbUsername, "Text", "Username", r => r.Username);
            Bind(tbPassword, "Text", "Password", r => r.Password);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RemoteRDMPUI_Design, UserControl>))]
    public abstract class RemoteRDMPUI_Design : RDMPSingleDatabaseObjectControl<RemoteRDMP>
    {
    }
}
