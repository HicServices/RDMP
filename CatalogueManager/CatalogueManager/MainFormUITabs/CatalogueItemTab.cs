using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Common.Validation;
using ReusableUIComponents;


namespace CatalogueManager.MainFormUITabs
{
    /// <summary>
    /// Each dataset (Catalogue) includes one or more virtual columns called CatalogueItems.  Each CatalogueItem is powered by an underlying columns in your data repository but there
    /// can be multiple CatalogueItems per column (for example if the DateOfBirth column is extractable either rounded to the nearest quarter or verbatim).  Thus CatalogueItems are both
    /// an extraction transform/rule set (See ExtractionInformationUI) and a descriptive entity which describes what the researcher will receive if they are given the column in an extract.
    /// This helpfully also lets you delete/restructure your data tables underneath without losing the descriptive data, validation rules, logging history etc of your datasets.
    /// 
    /// <para>This control lets you view/edit the descriptive metadata of a CatalogueItem in a dataset (Catalogue).</para>
    /// </summary>
    public partial class CatalogueItemTab : CatalogueItemTab_Design ,ISaveableUI
    {
        private bool _clearingFormComponents;
        
        public CatalogueItemTab()
        {
            InitializeComponent();
            objectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
            AssociatedCollection = RDMPCollection.Catalogue;
        }

        private CatalogueItem _catalogueItem;

        public CatalogueItem CatalogueItem
        {
            get { return _catalogueItem; }
            private set
            {
                _catalogueItem = value;
                
                RefreshUIFromDatabase();
            }
        }

        private void RefreshUIFromDatabase()
        {
            _clearingFormComponents = true;

            ci_tbID.Text = _catalogueItem.ID.ToString();
            ci_tbName.Text = _catalogueItem.Name;

            _oldCatalogueItemName = _catalogueItem.Name;
            _newCatalogueItemName = _catalogueItem.Name;

            ci_tbStatisticalConsiderations.Text = _catalogueItem.Statistical_cons;
            ci_tbResearchRelevance.Text = _catalogueItem.Research_relevance;
            ci_tbDescription.Text = _catalogueItem.Description;
            ci_tbTopics.Text = _catalogueItem.Topic;

            ci_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));
            ci_ddPeriodicity.SelectedItem = _catalogueItem.Periodicity;

            ci_tbAggregationMethod.Text = _catalogueItem.Agg_method;
            ci_tbLimitations.Text = _catalogueItem.Limitations;
            ci_tbComments.Text = _catalogueItem.Comments;
           
            _clearingFormComponents = false;
                
        }


        private string _oldCatalogueItemName = "";

        private string _newCatalogueItemName = "";


        private void ci_tbName_TextChanged(object sender, EventArgs e)
        {
            _newCatalogueItemName = ci_tbName.Text;
            SetStringPropertyOnCatalogueItem((TextBox)sender,"Name");
        }

        private void ci_tbStatisticalConsiderations_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Statistical_cons");
        }

        private void ci_tbResearchRelevance_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Research_relevance");
        }

        private void ci_tbDescription_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Description");
        }

        private void ci_tbTopics_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Topic");
        }

        private void ci_ddPeriodicity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_clearingFormComponents)
                return;

            if (_catalogueItem != null)
                _catalogueItem.Periodicity = (Catalogue.CataloguePeriodicity) ci_ddPeriodicity.SelectedItem;
        }

        private void ci_tbAggregationMethod_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Agg_method");
        }

        private void ci_tbLimitations_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Limitations");
        }

        private void ci_tbComments_TextChanged(object sender, EventArgs e)
        {
            SetStringPropertyOnCatalogueItem((TextBox)sender, "Comments");
        }

        bool objectSaverButton1_BeforeSave(DatabaseEntity databaseEntity)
        {
            //see if we need to display the dialog that lets the user sync up descriptions of multiuse columns e.g. CHI
            bool shouldDialogBeDisplayed;
            var propagate = new PropagateSaveChangesToCatalogueItemToSimilarNamedCatalogueItems(_activator,_catalogueItem, out shouldDialogBeDisplayed);
            propagate.RepositoryLocator = RepositoryLocator;

            //there are other CatalogueItems that share the same name as this one so give the user the option to propagate his changes to those too
            if (shouldDialogBeDisplayed)
            {
                DialogResult dialogResult = propagate.ShowDialog(this);

                if (dialogResult == DialogResult.Cancel)
                    return false;
            }

            PropagateRenameIfRequired();

            return true;
        }

        private bool PropagateRenameIfRequired()
        {
            if(_oldCatalogueItemName.Equals(_newCatalogueItemName))
                return false;

            if(_catalogueItem.Name != _newCatalogueItemName)
                throw new Exception("Unsure why _newCatalogueItemName is not an exact match for ci.Name");

            var cata = _catalogueItem.Catalogue;

            if (!string.IsNullOrWhiteSpace(cata.ValidatorXML) && cata.ValidatorXML.Contains(_oldCatalogueItemName))
                if(MessageBox.Show("You just renamed the CatlogueItem " + _oldCatalogueItemName + " to " + _newCatalogueItemName + " would you like to perform a rename on the validation XML for the Catalogue?","Fix Validation references?",MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                    try
                    {
                        Validator validator = Validator.LoadFromXml(cata.ValidatorXML);

                        validator.RenameColumn(_oldCatalogueItemName, _newCatalogueItemName);
                    
                        string newXML = validator.SaveToXml();

                        cata.ValidatorXML = newXML;
                        cata.SaveToDatabase();
                        _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cata));
                        return true;
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Problem occurred when attempting to rename column " + _oldCatalogueItemName +
                                        " to " + _newCatalogueItemName + " : " + ex);
                    }

            return false;
        }

        public override void SetDatabaseObject(IActivateItems activator, CatalogueItem databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            CatalogueItem = databaseObject;
            
            if (CatalogueItem.ExtractionInformation != null)
                AddToMenu(new ExecuteCommandActivate(activator, CatalogueItem.ExtractionInformation), "Go To Extraction Information");
            else
                AddToMenu(new ExecuteCommandMakeCatalogueItemExtractable(activator, CatalogueItem), "Make Extractable");

            if(CatalogueItem.ColumnInfo_ID != null)
                AddToMenu(new ExecuteCommandShow(activator, CatalogueItem.ColumnInfo, 0, true));
        }
        
        #region Helper Methods for setting string and Uri properties

        private void SetStringPropertyOnCatalogueItem(TextBox tb, string property)
        {
            SetStringProperty(tb, property, _catalogueItem);
        }

        private void SetStringProperty(TextBox tb, string property, object toSetOn)
        {
            PropertyInfo target = toSetOn.GetType().GetProperty(property);
            FieldInfo targetMaxLength = toSetOn.GetType().GetField(property + "_MaxLength");
            
            if (target == null || targetMaxLength == null)
                throw new Exception("Could not find property " + property + " or it did not have a specified _MaxLength");

            if (tb.TextLength > (int)targetMaxLength.GetValue(toSetOn))
                tb.ForeColor = Color.Red;
            else
            {
                target.SetValue(toSetOn, tb.Text, null);
                tb.ForeColor = Color.Black;
            }
        }

        #endregion

        private bool _expand = true;

        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !_expand;
            _expand = !_expand;
            btnExpandOrCollapse.Text = _expand ? "+" : "-";
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        public override string GetTabName()
        {
            return base.GetTabName() + " (" + _catalogueItem.Catalogue.Name + ")";
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueItemTab_Design, UserControl>))]
    public abstract class CatalogueItemTab_Design : RDMPSingleDatabaseObjectControl<CatalogueItem>
    {
    }
}
