using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.UI.Refreshing;
using static Azure.Core.HttpHeader;
using Rdmp.Core.Dataset;
using Rdmp.Core.Curation.Data;
using NPOI.OpenXmlFormats.Dml.Diagram;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core;
using Rdmp.UI.Collections;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.SubComponents;
public partial class DatsetConfigurationUI : DatsetConfigurationUI_Design, IRefreshBusSubscriber
{
    DatasetConfigurationUICommon Common;

    public DatsetConfigurationUI()
    {
        InitializeComponent();
        Common = new DatasetConfigurationUICommon();
    }

    public override void SetDatabaseObject(IActivateItems activator, Dataset databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        Common.Dataset = databaseObject;
        //tbName.Text = databaseObject.Name;
        //if(databaseObject.DigitalObjectIdentifier is not null)
        //    tbDOI.Text = databaseObject.DigitalObjectIdentifier.ToString();
        //if(databaseObject.Source is not null)
        //    tbSource.Text = databaseObject.Source;

        Bind(tbName, "Text", "Name", c => c.Name);
        Bind(tbDOI, "Text", "DigitalObjectIdentifier", c => c.DigitalObjectIdentifier);
        Bind(tbName, "Text", "Source", c => c.Source);
        var s = GetObjectSaverButton();
        s.SetupFor(this, databaseObject, activator);
        GetObjectSaverButton()?.Enable(false);
        
    }


    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        //Common.Activator = Activator;
    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
}
[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<DatsetConfigurationUI_Design, UserControl>))]
public abstract class
    DatsetConfigurationUI_Design : RDMPSingleDatabaseObjectControl<Dataset>
{
}