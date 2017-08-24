using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;


namespace Dashboard.Automation
{
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<SingleDataLoadLogView_Design, UserControl>))]
    public abstract class SingleDataLoadLogView_Design : RDMPSingleDatabaseObjectControl<ExternalDatabaseServer>
    {
        
    }
}
