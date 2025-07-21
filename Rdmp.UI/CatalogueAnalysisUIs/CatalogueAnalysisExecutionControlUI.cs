using Rdmp.Core.CatalogueAnalysisTools;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Rdmp.UI.CatalogueAnalysisUIs.Charts;
using Rdmp.UI.CatalogueSummary;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Rdmp.Core.CatalogueAnalysisTools.Data.PrimaryConstraint;
using static Rdmp.Core.CatalogueAnalysisTools.Data.SecondaryConstraint;

namespace Rdmp.UI.CatalogueAnalysisUIs;

public partial class CatalogueAnalysisExecutionControlUI : CatalogueAnalysisExecutionControlUI_Design
{
    private Catalogue _catalogue;
    private DQERepository _dqeRepository;
    private List<PrimaryConstraint> _primaryConstraints = [];

    private List<SecondaryConstraint> _secondaryConstraints = [];
    private void SetupPrimaryConstrainsConfiguration()
    {
        foreach (var ci in _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo))
        {
            var existingEvaulation = _primaryConstraints.Where(pc => pc.ColumnInfo.ID == ci.ID).FirstOrDefault();

            var constraintsDropdown = new ComboBox();
            constraintsDropdown.Items.AddRange(Enum.GetNames(typeof(PrimaryConstraint.Constraints)));
            constraintsDropdown.Click += (o, e) =>
            {
                HandleConstraintUpdate(o, e, ci);
            };

            var resultsDropdown = new ComboBox();
            resultsDropdown.Items.AddRange(Enum.GetNames(typeof(PrimaryConstraint.ConstraintResults)));
            resultsDropdown.Click += (o, e) =>
            {
                HandleConstraintResultUpdate(o, e, ci);
            };

            if (existingEvaulation is not null)
            {
                constraintsDropdown.SelectedIndex = (int)existingEvaulation.Constraint;
                resultsDropdown.SelectedIndex = (int)existingEvaulation.Result;
            }

            var clearButton = new Button();
            clearButton.Click += (o, e) =>
            {
                constraintsDropdown.SelectedIndex = -1;
                resultsDropdown.SelectedIndex = -1;
            };
            clearButton.Text = "Clear";

            primaryConstrainsTableLayout.RowCount++;
            primaryConstrainsTableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize, 50F));
            primaryConstrainsTableLayout.Controls.Add(new Label() { Text = ci.GetRuntimeName() }, 0, primaryConstrainsTableLayout.RowCount - 1);
            primaryConstrainsTableLayout.Controls.Add(constraintsDropdown, 1, primaryConstrainsTableLayout.RowCount - 1);
            primaryConstrainsTableLayout.Controls.Add(resultsDropdown, 2, primaryConstrainsTableLayout.RowCount - 1);
            primaryConstrainsTableLayout.Controls.Add(clearButton, 3, primaryConstrainsTableLayout.RowCount - 1);
        }
    }

    public void SetupSecondaryConstrainsConfiguration()
    {
        foreach (var ci in _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo))
        {
            var foundConstraints = _dqeRepository.GetAllObjectsWhere<SecondaryConstraint>("ColumnInfo_ID", ci.ID);
            _secondaryConstraints.AddRange(foundConstraints);
        }
        foreach (var constraint in _secondaryConstraints)
        {
            AddSecondaryConstraint(constraint);
        }
    }

    public CatalogueAnalysisExecutionControlUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _dqeRepository = new DQERepository(activator.RepositoryLocator.CatalogueRepository);
        _primaryConstraints = _catalogue.CatalogueItems.SelectMany(ci => _dqeRepository.GetAllObjectsWhere<PrimaryConstraint>("ColumnInfo_ID", ci.ColumnInfo_ID)).ToList();
        SetupPrimaryConstrainsConfiguration();
        SetupSecondaryConstrainsConfiguration();
        HandleValidationChart();
        //timePeriodicityChart1.ClearGraph();
        catalogueValidationResultsui1.SetDatabaseObject(activator, databaseObject);
        var columnInfos = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo);
        cbTime.Items.AddRange(columnInfos.ToArray());
        cbPivot.Items.AddRange(columnInfos.ToArray());
        var evaluations = _dqeRepository.GetAllObjectsWhere<CatalogueValidation>("Catalogue_ID", _catalogue.ID);
        if (evaluations.Any())
        {
            var evaluation = evaluations?.Last();
            //timePeriodicityChart1.SelectEvaluation(evaluation, "ALL");
            if (evaluation is not null)
            {
                cbTime.SelectedIndex = columnInfos.Select(ci => ci.ID).ToList().IndexOf(evaluation.TimeColumn_ID);
                cbPivot.SelectedIndex = columnInfos.Select(ci => ci.ID).ToList().IndexOf(evaluation.PivotColumn_ID);
            }
        }


        var userDefinedCharts = _dqeRepository.GetAllObjectsWhere<UserDefinedChart>("Catalogue_ID", databaseObject.ID);
        foreach (var udc in userDefinedCharts)
        {
            var chartRunner = new UserDefinedChartRunner();
            chartRunner.Setup(activator, _dqeRepository, udc);
            tblCustomChart.Controls.Add(chartRunner);
        }

    }


    private void HandleValidationChart()
    {
        //timePeriodicityChart1
    }

    private void HandleConstraintUpdate(object sender, EventArgs e, ColumnInfo ci)
    {

    }
    private void HandleConstraintResultUpdate(object sender, EventArgs e, ColumnInfo ci)
    {

    }

    private void primaryConstrainsTableLayout_Paint(object sender, PaintEventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void btnSavePrimaryConstraints_Click(object sender, EventArgs e)
    {
        var rowCount = primaryConstrainsTableLayout.RowCount;
        foreach (var row in Enumerable.Range(1, rowCount - 2))
        {
            var label = primaryConstrainsTableLayout.GetControlFromPosition(0, row + 1);

            var y = primaryConstrainsTableLayout.GetControlFromPosition(1, row + 1);

            var constraint = primaryConstrainsTableLayout.GetControlFromPosition(1, row + 1) as ComboBox;
            var result = primaryConstrainsTableLayout.GetControlFromPosition(2, row + 1) as ComboBox;
            var existingEvaulation = _primaryConstraints.Where(pc => pc.ColumnInfo.GetRuntimeName() == label.Text).FirstOrDefault();
            if (existingEvaulation != null)
            {
                existingEvaulation.Constraint = (PrimaryConstraint.Constraints)constraint.SelectedIndex;
                existingEvaulation.Result = (PrimaryConstraint.ConstraintResults)result.SelectedIndex;
                existingEvaulation.SaveToDatabase();
            }
            else
            {
                if (constraint.SelectedIndex >= 0 && result.SelectedIndex >= 0)
                {
                    var newEval = new PrimaryConstraint(_dqeRepository,
                        _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).Where(ci => ci.GetRuntimeName() == label.Text).FirstOrDefault(),
                        (PrimaryConstraint.Constraints)constraint.SelectedIndex,
                        (PrimaryConstraint.ConstraintResults)result.SelectedIndex
                    );
                    newEval.SaveToDatabase();
                }
                //todo warn about not selecting both
            }

        }
    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (cbPivot.SelectedItem is null || cbTime.SelectedItem is null)
        {
            throw new Exception("No time or pivot column selected");
        }

        bool hasStart = DateTime.TryParse(tbStartDate.Text, out var parsedStartDate);
        bool hasEnd = DateTime.TryParse(tbEndDate.Text, out var parsedEndDate);


        var validation = new CatalogueValidationTool(Activator.RepositoryLocator.CatalogueRepository, _catalogue,
           cbTime.SelectedItem as ColumnInfo,
            cbPivot.SelectedItem as ColumnInfo,
            hasStart ? parsedStartDate : null,
            hasEnd ? parsedEndDate : null,
            cbUsePrevious.Checked
            );
        validation.GenerateValidationReports();
    }

    private void label5_Click(object sender, EventArgs e)
    {

    }

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void AddSecondaryConstraint(SecondaryConstraint constraint = null)
    {
        var columnsCb = new ComboBox();
        var columnInfos = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToArray();
        columnsCb.Items.AddRange(columnInfos);

        var constraintCb = new ComboBox();
        constraintCb.Items.AddRange(Enum.GetNames(typeof(SecondaryConstraint.Constraints)).ToArray());
        var x = SecondaryConstrainsTableLayoutPanel.RowCount;
        constraintCb.SelectedIndexChanged += (object sender, EventArgs e) => ConstraintCb_SelectedIndexChanged(constraintCb, x);
        var consequencesCb = new ComboBox();
        consequencesCb.Items.AddRange(Enum.GetNames(typeof(SecondaryConstraint.Consequences)).ToArray());



        SecondaryConstrainsTableLayoutPanel.RowCount++;
        SecondaryConstrainsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 50F));
        SecondaryConstrainsTableLayoutPanel.Controls.Add(columnsCb, 0, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
        SecondaryConstrainsTableLayoutPanel.Controls.Add(constraintCb, 1, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
        SecondaryConstrainsTableLayoutPanel.Controls.Add(consequencesCb, 2, SecondaryConstrainsTableLayoutPanel.RowCount - 1);

        if (constraint != null)
        {
            columnsCb.SelectedIndex = columnInfos.Select(ci => ci.ID).ToList().IndexOf(constraint.ColumnInfo.ID);
            constraintCb.SelectedIndex = (int)constraint.Constraint;
            consequencesCb.SelectedIndex = (int)constraint.Consequence;
            ConstraintCb_SelectedIndexChanged(constraintCb, SecondaryConstrainsTableLayoutPanel.RowCount - 1, constraint);
        }
    }

    private void ConstraintCb_SelectedIndexChanged(ComboBox cb, int rowIndex, SecondaryConstraint constraint = null)
    {
        var selectedType = (SecondaryConstraint.Constraints)cb.SelectedIndex;
        var argA = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(3, rowIndex);
        if (argA != null)
        {
            SecondaryConstrainsTableLayoutPanel.Controls.Remove(argA);
        }
        var argB = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(4, rowIndex);
        if (argB != null)
        {
            SecondaryConstrainsTableLayoutPanel.Controls.Remove(argB);
        }
        switch (selectedType)
        {
            case SecondaryConstraint.Constraints.REGULAREXPRESSION:
                var tb = new TextBox();
                tb.Width = 150;
                tb.Height = 40;
                tb.Name = "patternArg";
                if (constraint != null)
                {
                    var arg = constraint.GetArguments().FirstOrDefault();
                    if (arg != null)
                    {
                        tb.Text = arg.Value;
                    }
                }
                SecondaryConstrainsTableLayoutPanel.Controls.Add(tb, 3, rowIndex);
                break;
            case SecondaryConstraint.Constraints.BOUNDDATE:
            case SecondaryConstraint.Constraints.BOUNDDOUBLE:
                var lowertb = new TextBox();
                lowertb.Width = 150;
                lowertb.Height = 40;
                lowertb.Name = "lowerArg";
                if (constraint != null)
                {
                    var arg = constraint.GetArguments().Where(a => a.Key == "Lower").FirstOrDefault();
                    if (arg != null)
                    {
                        lowertb.Text = arg.Value;
                    }
                }
                SecondaryConstrainsTableLayoutPanel.Controls.Add(lowertb, 3, rowIndex);
                var upperArg = new TextBox();
                upperArg.Width = 150;
                upperArg.Height = 40;
                upperArg.Name = "upperArg";
                if (constraint != null)
                {
                    var arg = constraint.GetArguments().Where(a => a.Key == "Upper").FirstOrDefault();
                    if (arg != null)
                    {
                        upperArg.Text = arg.Value;
                    }
                }
                SecondaryConstrainsTableLayoutPanel.Controls.Add(upperArg, 4, rowIndex);
                break;
            case SecondaryConstraint.Constraints.NOTNULL:
            default:
                break;
        }
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
        AddSecondaryConstraint();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        var rowCount = SecondaryConstrainsTableLayoutPanel.RowCount;
        foreach (var row in Enumerable.Range(1, rowCount - 2))
        {
            var columnCB = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(0, row + 1) as ComboBox;
            var column = columnCB.SelectedItem as ColumnInfo;
            var constraintCB = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(1, row + 1) as ComboBox;
            SecondaryConstraint.Constraints constraint = (SecondaryConstraint.Constraints)Enum.Parse(typeof(SecondaryConstraint.Constraints), constraintCB.SelectedItem.ToString(), true);
            var consequenceCB = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(2, row + 1) as ComboBox;
            SecondaryConstraint.Consequences consequence = (SecondaryConstraint.Consequences)Enum.Parse(typeof(SecondaryConstraint.Consequences), consequenceCB.SelectedItem.ToString(), true);
            var secondary = new SecondaryConstraint(_dqeRepository, column, constraint, consequence);
            secondary.SaveToDatabase();
            switch (constraint)
            {
                case Core.CatalogueAnalysisTools.Data.SecondaryConstraint.Constraints.REGULAREXPRESSION:
                    var pattern = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(3, row + 1);
                    var scArg = new SecondaryConstraintArgument(_dqeRepository, "Pattern", pattern.Text, secondary);
                    scArg.SaveToDatabase();
                    break;
                case Core.CatalogueAnalysisTools.Data.SecondaryConstraint.Constraints.BOUNDDATE:
                case Core.CatalogueAnalysisTools.Data.SecondaryConstraint.Constraints.BOUNDDOUBLE:
                    var lower = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(3, row + 1);
                    if (!string.IsNullOrWhiteSpace(lower.Text))
                    {
                        var lowerArg = new SecondaryConstraintArgument(_dqeRepository, "Lower", lower.Text, secondary);
                        lowerArg.SaveToDatabase();
                    }
                    var upper = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(4, row + 1);
                    if (!string.IsNullOrWhiteSpace(upper.Text))
                    {
                        var upperArg = new SecondaryConstraintArgument(_dqeRepository, "Upper", upper.Text, secondary);
                        upperArg.SaveToDatabase();
                    }
                    break;
                default:
                    break;
            }
        }
        foreach (var sc in _secondaryConstraints)
        {
            sc.DeleteInDatabase();
        }
    }

    private void cbUsePrevious_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void button3_Click(object sender, EventArgs e)
    {
        var dialog = new UserDefinedChartCreationForm();
        dialog.Setup(Activator, _dqeRepository, _catalogue);
        dialog.Show();
    }

    private void lblRecordCount_Click(object sender, EventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueAnalysisExecutionControlUI_Design, UserControl>))]
public abstract class CatalogueAnalysisExecutionControlUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}