using Rdmp.UI.TestsAndSetup.ServicePropogation;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.UI.Refreshing;
using Rdmp.Core.Dataset;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Drawing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.SubComponents;
public partial class RegexRedactionConfigurationUI : RegexRedactionConfigurationUI_Design, IRefreshBusSubscriber
{
    public RegexRedactionConfigurationUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, RegexRedactionConfiguration databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        Bind(tbName, "Text", "Name", static c => c.Name);
        Bind(tbRegexPattern, "Text", "RegexPattern", static c => c.RegexPattern);
        Bind(tbRedactionString, "Text", "RedactionString", static c => c.RedactionString);
        Bind(tbDescription, "Text", "Description", static c => c.Description);
        var s = GetObjectSaverButton();
        s.SetupFor(this, databaseObject, activator);
        GetObjectSaverButton()?.Enable(false);
    }


    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void label4_Click(object sender, EventArgs e)
    {

    }

    private void label5_Click(object sender, EventArgs e)
    {

    }
}
[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<RegexRedactionConfigurationUI_Design, UserControl>))]
public abstract class
    RegexRedactionConfigurationUI_Design : RDMPSingleDatabaseObjectControl<RegexRedactionConfiguration>
{
}