using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueManager.Collections.Providers;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Reports;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// TECHNICAL: the base class for all collections of RDMP objects in a given toolbox.
    /// </summary>
    [TechnicalUI]
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPCollectionUI, UserControl>))]
    public abstract class RDMPCollectionUI : RDMPCollectionUI_Design,IConsultableBeforeClosing
    {
        public RDMPCollectionCommonFunctionality CommonFunctionality { get; private set; }
        
        protected RDMPCollectionUI()
        {
            CommonFunctionality = new RDMPCollectionCommonFunctionality();
        }


        public virtual void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            CommonFunctionality.TearDown();
        }

        public abstract void SetItemActivator(IActivateItems activator);

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<RDMPCollectionUI_Design, UserControl>))]
    public class RDMPCollectionUI_Design : RDMPUserControl
    {
    }
}
