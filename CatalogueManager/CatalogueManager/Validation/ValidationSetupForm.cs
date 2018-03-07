using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using HIC.Common.Validation.Constraints.Secondary;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.Validation
{
    /// <summary>
    /// Validation is an essential part of hosting research data.  If one month all the new records come in with values in gender of 'Male' and 'Female'  when previously they were 'M' or 
    /// 'F' then you want to know about it (because it will affect filters and end users of the data who now have to include 2 different values in their WHERE statements).  In such a 
    /// trivial situation the first step would be to confirm if it is a mistake with the data provider, if not then a decision should be made whether to standardise on the old/new 
    /// categories and adjust the data load accordingly.
    /// 
    /// But for this to happen at all you need to be able to recognise when such problems occur.  The RDMP handles this by allowing you to specify validation rules on each of the 
    /// extractable columns / transforms you make available to researchers.  On the left of this form you can see all the columns/transforms.  By selecting one you can view/edit its'
    /// collection of Secondary Constraints (see SecondaryConstraintUI) and choose a Primary Constraint (Validates the datatype, only use a primary constraint if you have an insane
    /// schema such as using varchar(max) to store 'dates' and have dirty data that includes values like 'last friday' mixed in with legit values).
    /// </summary>
    public partial class ValidationSetupForm : ValidationSetupForm_Design, ISaveableUI
    {
        private string _noPrimaryConstraintText = "No Primary Constraint Defined";
        
        public Validator Validator { get; private set; }
     
        private bool bSuppressChangeEvents = false;
        private Catalogue _catalogue;

        private string ClearSelection = "<<Clear Selection>>";
        
        private ItemValidator SelectedColumnItemValidator {get
        {
            var ei = olvColumns.SelectedObject as ExtractionInformation;
            //The user has not selected a column
            if (ei == null)
                return null;

            var c = ei.GetRuntimeName();

            //The validator can contains all the columns in the Catalogue (Dataset) but columns which don't have any validation on them yet might not be in it's ItemValidators collection
            if (!Validator.ItemValidators.Any(iv => iv.TargetProperty.Equals(c)))
                Validator.ItemValidators.Add(new ItemValidator(c));//It's a novel column, so create an empty ItemValidator for the column name so the user can configure new validation

            return Validator.GetItemValidator(c);
        }}

        public ValidationSetupForm()
        {
            InitializeComponent();
            
            SetupAvailableOperations();

            olvColumns.RowHeight = 19;
            ddConsequence.DataSource = Enum.GetValues(typeof (Consequence));

            int vertScrollWidth = SystemInformation.VerticalScrollBarWidth;
            tableLayoutPanel1.Padding = new Padding(0, 0, vertScrollWidth, 0);

            AssociatedCollection = RDMPCollection.Catalogue;
        }

        private bool isFirstTime = true;
        

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            //if someone renames a Catalogue we don't want to erase all the rule changes they have configured here
            if(!isFirstTime)
                return;
            
            isFirstTime = false;

            _catalogue = databaseObject;

            objectSaverButton1.SetupFor(databaseObject, _activator.RefreshBus);
            objectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;

            olvName.ImageGetter = (o) => activator.CoreIconProvider.GetImage(o);

            SetupComboBoxes(databaseObject);

            //get the validation XML
            if (string.IsNullOrWhiteSpace(databaseObject.ValidatorXML))
                Validator = new Validator();
            else
                Validator = Validator.LoadFromXml(databaseObject.ValidatorXML);
            
            var extractionInformations = databaseObject.GetAllExtractionInformation(ExtractionCategory.Any).ToArray();
            Array.Sort(extractionInformations);
            olvColumns.AddObjects(extractionInformations);

            ValidateConfiguration();
        }

        private void SetupComboBoxes(Catalogue catalogue)
        {
            cbxTimePeriodColumn.Items.Clear();
            cbxPivotColumn.Items.Clear();

            var cis = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            cbxTimePeriodColumn.Items.Add(ClearSelection);
            cbxTimePeriodColumn.Items.AddRange(cis);

            if (catalogue.TimeCoverage_ExtractionInformation_ID != null)
                cbxTimePeriodColumn.SelectedItem = catalogue.TimeCoverage_ExtractionInformation;

            cbxPivotColumn.Items.Add(ClearSelection);
            cbxPivotColumn.Items.AddRange(cis);

            if (catalogue.PivotCategory_ExtractionInformation_ID != null)
                cbxPivotColumn.SelectedItem = catalogue.PivotCategory_ExtractionInformation;
        }

        private void ValidateConfiguration()
        {
            //if there are any item validators that do not map to an existing column in the dataset then we must show the resolve missing references dialog.
            if (ResolveMissingTargetPropertiesForm.GetMissingReferences(Validator,olvColumns.Objects.Cast<ExtractionInformation>()).Any())
            {
               ResolveMissingTargetPropertiesForm dialog = new ResolveMissingTargetPropertiesForm(Validator,olvColumns.Objects.Cast<ExtractionInformation>().ToArray());

               if(dialog.ShowDialog() == DialogResult.OK)
                    Validator = dialog.AdjustedValidator;    
            }
        }

        private void SetupAvailableOperations()
        {
            List<string> constraintNames = new List<string>();
            constraintNames.AddRange(Validator.GetPrimaryConstraintNames());
            constraintNames.Sort();
            
            ddPrimaryConstraints.Items.AddRange(constraintNames.ToArray());
            ddPrimaryConstraints.Items.Add(_noPrimaryConstraintText);


            List<string> secondaryConstraintNames = new List<string>();
            secondaryConstraintNames.AddRange(Validator.GetSecondaryConstraintNames());
            secondaryConstraintNames.Sort();

            ddSecondaryConstraints.Items.AddRange(secondaryConstraintNames.ToArray());
        }

        private void lbColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (olvColumns.SelectedItem != null)
                PopulateFormForSelectedColumn();
        }


        private void PopulateFormForSelectedColumn()
        {
            if(IsDisposed)
               return;

            bSuppressChangeEvents = true;

            if (SelectedColumnItemValidator == null || SelectedColumnItemValidator.PrimaryConstraint == null)
            {
                ddPrimaryConstraints.Text = _noPrimaryConstraintText;
                ddConsequence.SelectedItem = Consequence.Missing;
            }
            else
            {
                ddPrimaryConstraints.Text = SelectedColumnItemValidator.PrimaryConstraint.GetType().Name;
                if (SelectedColumnItemValidator.PrimaryConstraint.Consequence.HasValue)
                    ddConsequence.SelectedItem = SelectedColumnItemValidator.PrimaryConstraint.Consequence.Value;
                else
                    ddConsequence.SelectedItem = Consequence.Missing;

            }

            //Make consequence selection only possible if there is a priary constraint selected
            ddConsequence.Enabled = ddPrimaryConstraints.Text != _noPrimaryConstraintText;

            //clear secondary constraints then add them again (it's the only way to be sure)
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 0;

            if (SelectedColumnItemValidator != null)
                foreach (var secondaryConstraint in SelectedColumnItemValidator.SecondaryConstraints)
                    AddSecondaryConstraintControl(secondaryConstraint);
            
            bSuppressChangeEvents = false;
        }

        private void ddPrimaryConstraints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(bSuppressChangeEvents)
                return;

            bSuppressChangeEvents = true;

            if (ddPrimaryConstraints.Text == _noPrimaryConstraintText)
            {
                SelectedColumnItemValidator.PrimaryConstraint = null;
                ddConsequence.SelectedItem = Consequence.Missing;
            }
            else
            try
            {
                SelectedColumnItemValidator.PrimaryConstraint =
                    Validator.CreateConstraint(ddPrimaryConstraints.Text,(Consequence)ddConsequence.SelectedValue) as PrimaryConstraint;
            }
            catch(Exception ex)
            {
                ExceptionViewer.Show("Failed to create PrimaryConstraint '" + ddPrimaryConstraints.Text + "'",ex);
            }

            //Make consequence selection only possible if there is a priary constraint selected
            ddConsequence.Enabled = ddPrimaryConstraints.Text != _noPrimaryConstraintText;


            bSuppressChangeEvents = false;
        }

        private void btnAddSecondaryConstraint_Click(object sender, EventArgs e)
        {
            if (ddSecondaryConstraints.SelectedItem != null)
            {
                var secondaryConstriant =
                    Validator.CreateConstraint(ddSecondaryConstraints.Text,Consequence.Missing) as SecondaryConstraint;
                
                SelectedColumnItemValidator.SecondaryConstraints.Add(secondaryConstriant);
                AddSecondaryConstraintControl(secondaryConstriant);
            }
        }

        private void SecondaryConstraintRequestDelete(object sender)
        {
            var toDelete = (sender as SecondaryConstraintUI).SecondaryConstriant;

            SelectedColumnItemValidator.SecondaryConstraints.Remove(toDelete);
            tableLayoutPanel1.Controls.Remove(sender as Control);
        }

        private void AddSecondaryConstraintControl(SecondaryConstraint secondaryConstriant)
        {
            tableLayoutPanel1.RowCount++;
            
            SecondaryConstraintUI toAdd = new SecondaryConstraintUI(RepositoryLocator.CatalogueRepository,secondaryConstriant,olvColumns.Objects.Cast<ExtractionInformation>().Select(c=>c.GetRuntimeName()).ToArray());

            toAdd.Width = splitContainer1.Panel2.Width;
            toAdd.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            toAdd.RequestDeletion += SecondaryConstraintRequestDelete;
            tableLayoutPanel1.Controls.Add(toAdd, tableLayoutPanel1.RowCount - 1,0);
            
            //this array always seems to be 1 element long..
            tableLayoutPanel1.RowStyles[0].SizeType = SizeType.AutoSize;    
        }


        private void ddConsequence_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(bSuppressChangeEvents)
                return;
            
            if (SelectedColumnItemValidator != null)
                if (SelectedColumnItemValidator.PrimaryConstraint != null)
                    SelectedColumnItemValidator.PrimaryConstraint.Consequence = (Consequence) ddConsequence.SelectedValue;
                else
                {
                    MessageBox.Show("you must select a primary constraint first");

                    bSuppressChangeEvents = true;
                    ddConsequence.SelectedItem = Consequence.Missing;
                    bSuppressChangeEvents = false;
                }
        }
        
        private void btnConfigureStandardRegex_Click(object sender, EventArgs e)
        {
            StandardRegexUI dialog = new StandardRegexUI();
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.Closed += (s, v) => PopulateFormForSelectedColumn();
            dialog.Show();
        }
        
        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvColumns.UseFiltering = true;
            olvColumns.ModelFilter = new TextMatchFilter(olvColumns,tbFilter.Text,StringComparison.CurrentCultureIgnoreCase);
        }

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if(HasChanges())
            {
                var dr = MessageBox.Show("Save Validation rule changes for Catalogue '" + _catalogue + "'?", "Save Changes",MessageBoxButtons.YesNoCancel);

                switch (dr)
                {
                    case DialogResult.Yes:
                        Save();
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void Save()
        {
            try
            {
                var final = Validator.SaveToXml();
                _catalogue.ValidatorXML = final;
                _catalogue.SaveToDatabase();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

            Publish(_catalogue);
        }

        private bool objectSaverButton1_BeforeSave(DatabaseEntity arg)
        {
            Save();
            return true;
        }

        private bool HasChanges()
        {
            try
            {
                var final = Validator.SaveToXml();
                return _catalogue.ValidatorXML != final;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void cbxTimePeriodColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTimePeriod(cbxTimePeriodColumn.SelectedItem as ExtractionInformation);
        }

        private void cbxPivotColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPivot(cbxPivotColumn.SelectedItem as ExtractionInformation);
        }
        private void SetTimePeriod(ExtractionInformation selected)
        {
            _catalogue.TimeCoverage_ExtractionInformation_ID = selected != null ? selected.ID : (int?)null;
            _catalogue.SaveToDatabase();
            Publish(_catalogue);
        }

        private void SetPivot(ExtractionInformation selected)
        {
            _catalogue.PivotCategory_ExtractionInformation_ID = selected != null ? selected.ID : (int?)null;
            _catalogue.SaveToDatabase();
            Publish(_catalogue);
        }

        private void lblPickTimePeriodColumn_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any), true, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var ei = dialog.Selected as ExtractionInformation;
                cbxTimePeriodColumn.SelectedItem = ei;
                SetTimePeriod(ei);
            }
        }

        private void lblPickPivotColumn_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any), true, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var ei = dialog.Selected as ExtractionInformation;
                cbxPivotColumn.SelectedItem = ei;
                SetPivot(ei);
            }
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ValidationSetupForm_Design, UserControl>))]
    public abstract class ValidationSetupForm_Design:RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}
