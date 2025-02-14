// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.Validation;

/// <summary>
/// Validation is an essential part of hosting research data.  If one month all the new records come in with values in gender of 'Male' and 'Female'  when previously they were 'M' or
/// 'F' then you want to know about it (because it will affect filters and end users of the data who now have to include 2 different values in their WHERE statements).  In such a
/// trivial situation the first step would be to confirm if it is a mistake with the data provider, if not then a decision should be made whether to standardise on the old/new
/// categories and adjust the data load accordingly.
/// 
/// <para>But for this to happen at all you need to be able to recognise when such problems occur.  The RDMP handles this by allowing you to specify validation rules on each of the
/// extractable columns / transforms you make available to researchers.  On the left of this form you can see all the columns/transforms.  By selecting one you can view/edit its'
/// collection of Secondary Constraints (see SecondaryConstraintUI) and choose a Primary Constraint (Validates the datatype, only use a primary constraint if you have an insane
/// schema such as using varchar(max) to store 'dates' and have dirty data that includes values like 'last friday' mixed in with legit values).</para>
/// </summary>
public partial class ValidationSetupUI : ValidationSetupForm_Design, ISaveableUI
{
    private string _noPrimaryConstraintText = "No Primary Constraint Defined";

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Validator Validator { get; private set; }

    private bool bSuppressChangeEvents;
    private Catalogue _catalogue;

    private string ClearSelection = "<<Clear Selection>>";

    private ItemValidator SelectedColumnItemValidator
    {
        get
        {
            //The user has not selected a column
            if (olvColumns.SelectedObject is not ExtractionInformation ei)
                return null;

            var c = ei.GetRuntimeName();

            //The validator can contains all the columns in the Catalogue (Dataset) but columns which don't have any validation on them yet might not be in its ItemValidators collection
            if (!Validator.ItemValidators.Any(iv => iv.TargetProperty.Equals(c)))
                Validator.ItemValidators.Add(
                    new ItemValidator(
                        c)); //It's a novel column, so create an empty ItemValidator for the column name so the user can configure new validation

            return Validator.GetItemValidator(c);
        }
    }

    public ValidationSetupUI()
    {
        InitializeComponent();

        SetupAvailableOperations();

        olvColumns.RowHeight = 19;
        ddConsequence.DataSource = Enum.GetValues(typeof(Consequence));

        var vertScrollWidth = SystemInformation.VerticalScrollBarWidth;
        tableLayoutPanel1.Padding = new Padding(0, 0, vertScrollWidth, 0);

        AssociatedCollection = RDMPCollection.Catalogue;

        ObjectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;

        olvName.ImageGetter = o => Activator.CoreIconProvider.GetImage(o).ImageToBitmap();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        CommonFunctionality.AddToMenu(new ExecuteCommandRunDQEOnCatalogue(activator).SetTarget(databaseObject));
        CommonFunctionality.AddToMenu(new ExecuteCommandViewDQEResultsForCatalogue(activator)
        { OverrideCommandName = "View Results..." }.SetTarget(databaseObject));

        _catalogue = databaseObject;

        SetupComboBoxes(databaseObject);

        //get the validation XML
        Validator = string.IsNullOrWhiteSpace(databaseObject.ValidatorXML)
            ? new Validator()
            : Validator.LoadFromXml(databaseObject.ValidatorXML);

        var extractionInformations = databaseObject.GetAllExtractionInformation(ExtractionCategory.Any).ToArray();
        Array.Sort(extractionInformations);

        //Update the objects in case the publish is because the user added new columns etc
        var oldSelection = olvColumns.SelectedObject;
        olvColumns.ClearObjects();
        olvColumns.AddObjects(extractionInformations);
        olvColumns.SelectedObject = oldSelection;

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
        if (ResolveMissingTargetPropertiesUI
            .GetMissingReferences(Validator, olvColumns.Objects.Cast<ExtractionInformation>()).Any())
        {
            var dialog = new ResolveMissingTargetPropertiesUI(Validator,
                olvColumns.Objects.Cast<ExtractionInformation>().ToArray());

            if (dialog.ShowDialog() == DialogResult.OK)
                Validator = dialog.AdjustedValidator;
        }
    }

    private void SetupAvailableOperations()
    {
        var constraintNames = new List<string>();
        constraintNames.AddRange(Validator.GetPrimaryConstraintNames());
        constraintNames.Sort();

        ddPrimaryConstraints.Items.AddRange(constraintNames.ToArray());
        ddPrimaryConstraints.Items.Add(_noPrimaryConstraintText);


        var secondaryConstraintNames = new List<string>();
        secondaryConstraintNames.AddRange(Validator.GetSecondaryConstraintNames());
        secondaryConstraintNames.Sort();

        ddSecondaryConstraints.Items.AddRange(secondaryConstraintNames.ToArray());
    }

    private void lbColumns_SelectedIndexChanged(object sender, EventArgs e)
    {
        PopulateFormForSelectedColumn();
    }


    private void PopulateFormForSelectedColumn()
    {
        if (IsDisposed)
            return;

        bSuppressChangeEvents = true;

        if (SelectedColumnItemValidator?.PrimaryConstraint == null)
        {
            ddPrimaryConstraints.Text = _noPrimaryConstraintText;
            ddConsequence.SelectedItem = Consequence.Missing;
        }
        else
        {
            ddPrimaryConstraints.Text = SelectedColumnItemValidator.PrimaryConstraint.GetType().Name;
            ddConsequence.SelectedItem =
                SelectedColumnItemValidator.PrimaryConstraint.Consequence ?? Consequence.Missing;
        }

        //Make consequence selection only possible if there is a primary constraint selected
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
        if (bSuppressChangeEvents)
            return;

        if (SelectedColumnItemValidator == null)
            return;

        bSuppressChangeEvents = true;

        if (ddPrimaryConstraints.Text == _noPrimaryConstraintText)
        {
            SelectedColumnItemValidator.PrimaryConstraint = null;
            ddConsequence.SelectedItem = Consequence.Missing;
        }
        else
        {
            try
            {
                SelectedColumnItemValidator.PrimaryConstraint =
                    Validator.CreateConstraint(ddPrimaryConstraints.Text, (Consequence)ddConsequence.SelectedValue) as
                        PrimaryConstraint;
            }
            catch (Exception ex)
            {
                ExceptionViewer.Show($"Failed to create PrimaryConstraint '{ddPrimaryConstraints.Text}'", ex);
            }
        }

        //Make consequence selection only possible if there is a priary constraint selected
        ddConsequence.Enabled = ddPrimaryConstraints.Text != _noPrimaryConstraintText;


        bSuppressChangeEvents = false;
        ObjectSaverButton1.Enable(true);
    }

    private void btnAddSecondaryConstraint_Click(object sender, EventArgs e)
    {
        if (SelectedColumnItemValidator == null)
        {
            MessageBox.Show("You must select a column on the left first");
            return;
        }

        if (ddSecondaryConstraints.SelectedItem != null)
        {
            var secondaryConstriant =
                Validator.CreateConstraint(ddSecondaryConstraints.Text, Consequence.Missing) as SecondaryConstraint;

            SelectedColumnItemValidator.SecondaryConstraints.Add(secondaryConstriant);
            AddSecondaryConstraintControl(secondaryConstriant);
            ObjectSaverButton1.Enable(true);
        }
    }

    private void SecondaryConstraintRequestDelete(object sender)
    {
        var toDelete = (sender as SecondaryConstraintUI).SecondaryConstriant;

        SelectedColumnItemValidator.SecondaryConstraints.Remove(toDelete);
        tableLayoutPanel1.Controls.Remove(sender as Control);
        ObjectSaverButton1.Enable(true);
    }

    private void AddSecondaryConstraintControl(SecondaryConstraint secondaryConstriant)
    {
        tableLayoutPanel1.RowCount++;

        var toAdd = new SecondaryConstraintUI(Activator.RepositoryLocator.CatalogueRepository, secondaryConstriant,
            olvColumns.Objects.Cast<ExtractionInformation>().Select(c => c.GetRuntimeName()).ToArray())
        {
            Width = splitContainer1.Panel2.Width,
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
        };
        toAdd.RequestDeletion += SecondaryConstraintRequestDelete;
        tableLayoutPanel1.Controls.Add(toAdd, tableLayoutPanel1.RowCount - 1, 0);

        //this array always seems to be 1 element long..
        tableLayoutPanel1.RowStyles[0].SizeType = SizeType.AutoSize;
    }


    private void ddConsequence_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (bSuppressChangeEvents)
            return;

        if (SelectedColumnItemValidator != null)
            if (SelectedColumnItemValidator.PrimaryConstraint != null)
            {
                SelectedColumnItemValidator.PrimaryConstraint.Consequence = (Consequence)ddConsequence.SelectedValue;
                ObjectSaverButton1.Enable(true);
            }
            else
            {
                MessageBox.Show("you must select a primary constraint first");

                bSuppressChangeEvents = true;
                ddConsequence.SelectedItem = Consequence.Missing;
                bSuppressChangeEvents = false;
            }
    }

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        olvColumns.UseFiltering = true;
        olvColumns.ModelFilter =
            new TextMatchFilter(olvColumns, tbFilter.Text, StringComparison.CurrentCultureIgnoreCase);
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        if (HasChanges())
        {
            var dr = MessageBox.Show($"Save Validation rule changes for Catalogue '{_catalogue}'?", "Save Changes",
                MessageBoxButtons.YesNoCancel);

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
        _catalogue.TimeCoverage_ExtractionInformation_ID = selected?.ID;
    }

    private void SetPivot(ExtractionInformation selected)
    {
        _catalogue.PivotCategory_ExtractionInformation_ID = selected?.ID;
    }

    private void lblPickTimePeriodColumn_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "Which date column in the Catalogue should provide the time element of the data when generating graphs, DQE etc?",
            AllowSelectingNull = true
        }, _catalogue.GetAllExtractionInformation(ExtractionCategory.Any), out var selected))
        {
            cbxTimePeriodColumn.SelectedItem = selected;
            SetTimePeriod(selected);
        }
    }

    private void lblPickPivotColumn_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "Which column in the Catalogue provides the most useful subdivision of the data when viewing in DQE? The column should have a relatively small number of unique values e.g. healthboard.",
            AllowSelectingNull = true
        }, _catalogue.GetAllExtractionInformation(ExtractionCategory.Any), out var selected))
        {
            cbxPivotColumn.SelectedItem = selected;
            SetPivot(selected);
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ValidationSetupForm_Design, UserControl>))]
public abstract class ValidationSetupForm_Design : RDMPSingleDatabaseObjectControl<Catalogue>;