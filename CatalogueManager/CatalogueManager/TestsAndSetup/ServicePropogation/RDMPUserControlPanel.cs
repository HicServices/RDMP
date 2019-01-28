using System.Windows.Forms;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{ 
    /// <summary>
    /// An RDMPUserControl with a single docked panel (access via <see cref="Panel"/>)
    /// </summary>
    [TechnicalUI]
    class RDMPUserControlPanel : RDMPUserControl
    {
        public Panel Panel { get; private set; }

        public RDMPUserControlPanel()
        {
            Panel = new Panel();
            Panel.Dock = DockStyle.Fill;
            this.Controls.Add(Panel);
        }
    }
}