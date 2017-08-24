using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs
{
    /// <summary>
    /// Extraction Filter Parameter Sets are 'known good values' of 1 or more parameters of an extraction filter.  For example you might have a filter 'Hospitalised with conditions'
    /// with a parameter @listOfConditionCodes, then you can have multiple ExtractionFilterParameterSets 'Dementia Conditions', 'High Blood Pressure', 'Coronary Heart Disease' which
    /// are currated lists of codes that are effectively just a 'good value' for the main filter.
    /// 
    /// This user interface lets you edit one of these.
    /// </summary>
    public partial class ExtractionFilterParameterSetUI : ExtractionFilterParameterSetUI_Design,IConsultableBeforeClosing, ISaveableUI
    {
        private ExtractionFilterParameterSet _extractionFilterParameterSet;
        
        public ExtractionFilterParameterSet ExtractionFilterParameterSet
        {
            get { return _extractionFilterParameterSet; }
            private set
            {
                _extractionFilterParameterSet = value;
                RefreshUIFromDatabase();
            }
        }

        private void RefreshUIFromDatabase()
        {
            tbID.Text = ExtractionFilterParameterSet.ID.ToString();
            tbName.Text = ExtractionFilterParameterSet.Name;
            tbDescription.Text = ExtractionFilterParameterSet.Description;

            ParameterCollectionUIOptionsFactory factory = new ParameterCollectionUIOptionsFactory();
            var options = factory.Create(ExtractionFilterParameterSet);
            parameterCollectionUI1.SetUp(options);
        }

        public ExtractionFilterParameterSetUI()
        {
            InitializeComponent();
        }

        private void ExtractionFilterParameterSetUI_Load(object sender, EventArgs e)
        {

        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            ExtractionFilterParameterSet.Name = tbName.Text;
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            ExtractionFilterParameterSet.Description = tbDescription.Text;
        }

        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            OfferChanceToSaveDialog.ShowIfRequired(ExtractionFilterParameterSet);
        }

        private void btnRefreshParameters_Click(object sender, EventArgs e)
        {
            ExtractionFilterParameterSet.CreateNewValueEntries();
            RefreshUIFromDatabase();
        }

        public override void SetDatabaseObject(IActivateItems activator, ExtractionFilterParameterSet databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            ExtractionFilterParameterSet = databaseObject;
            objectSaverButton1.SetupFor(databaseObject,_activator.RefreshBus);
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionFilterParameterSetUI_Design, UserControl>))]
    public abstract class ExtractionFilterParameterSetUI_Design : RDMPSingleDatabaseObjectControl<ExtractionFilterParameterSet>
    {
    }
}
