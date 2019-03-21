using System.Windows.Forms;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    public interface IRDMPControl
    {
        RDMPControlCommonFunctionality CommonFunctionality { get; }
        
        Form ParentForm { get; }
        bool InvokeRequired { get; }

        IActivateItems Activator { get; }
        void SetItemActivator(IActivateItems activator);

        IRDMPControl GetTopmostRDMPUserControl();
    }
}