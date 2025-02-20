using Rdmp.Core.CatalogueAnalysisTools;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
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
using static Rdmp.Core.CatalogueAnalysisTools.Data.PrimaryContraint;
using static Rdmp.Core.CatalogueAnalysisTools.Data.SecondaryConstraint;

namespace Rdmp.UI.CatalogueAnalysisUIs;

public partial class CatalogueAnalysisExecutionControlUI : CatalogueAnalysisExecutionControlUI_Design
{
    private Catalogue _catalogue;
    private DQERepository _dqeRepository;
    private List<PrimaryContraint> _primaryConstraints;

    //private List<SecondaryConstraint> _secondaryConstraints;
    private void SetupPrimaryConstrainsConfiguration()
    {
        foreach (var ci in _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo))
        {
            var existingEvaulation = _primaryConstraints.Where(pc => pc.ColumnInfo.ID == ci.ID).FirstOrDefault();

            var constraintsDropdown = new ComboBox();
            constraintsDropdown.Items.AddRange(Enum.GetNames(typeof(PrimaryContraint.Contraints)));
            constraintsDropdown.Click += (o, e) =>
            {
                HandleConstraintUpdate(o, e, ci);
            };

            var resultsDropdown = new ComboBox();
            resultsDropdown.Items.AddRange(Enum.GetNames(typeof(PrimaryContraint.ConstraintResults)));
            resultsDropdown.Click += (o, e) =>
            {
                HandleConstraintResultUpdate(o, e, ci);
            };

            if (existingEvaulation is not null)
            {
                constraintsDropdown.SelectedIndex = (int)existingEvaulation.Contraint;
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

    public CatalogueAnalysisExecutionControlUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;
        _dqeRepository = new DQERepository(activator.RepositoryLocator.CatalogueRepository);
        _primaryConstraints = _catalogue.CatalogueItems.SelectMany(ci => _dqeRepository.GetAllObjectsWhere<PrimaryContraint>("ColumnInfo_ID", ci.ColumnInfo_ID)).ToList();
        SetupPrimaryConstrainsConfiguration();
        HandleValidationChart();
        timePeriodicityChart1.ClearGraph();
        var columnInfos = _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo);
        cbTime.Items.AddRange(columnInfos.ToArray());
        cbPivot.Items.AddRange(columnInfos.ToArray());
        var evaluations = _dqeRepository.GetAllObjectsWhere<CatalogueValidation>("Catalogue_ID", _catalogue.ID);
        if (evaluations.Any())
        {
            var evaluation = evaluations?.Last();
            timePeriodicityChart1.SelectEvaluation(evaluation, "ALL");
            if (evaluation is not null)
            {
                cbTime.SelectedIndex = columnInfos.Select(ci => ci.ID).ToList().IndexOf(evaluation.TimeColumn_ID);
                cbPivot.SelectedIndex = columnInfos.Select(ci => ci.ID).ToList().IndexOf(evaluation.PivotColumn_ID);
            }
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
                existingEvaulation.Contraint = (PrimaryContraint.Contraints)constraint.SelectedIndex;
                existingEvaulation.Result = (PrimaryContraint.ConstraintResults)result.SelectedIndex;
                existingEvaulation.SaveToDatabase();
            }
            else
            {
                if (constraint.SelectedIndex >= 0 && result.SelectedIndex >= 0)
                {
                    var newEval = new PrimaryContraint(_dqeRepository,
                        _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).Where(ci => ci.GetRuntimeName() == label.Text).FirstOrDefault(),
                        (PrimaryContraint.Contraints)constraint.SelectedIndex,
                        (PrimaryContraint.ConstraintResults)result.SelectedIndex
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
            hasStart?parsedStartDate:null,
            hasEnd?parsedEndDate:null,
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

    private void AddSecondaryConstraint()
    {
        var columnsCb = new ComboBox();
        columnsCb.Items.AddRange(_catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToArray());

        var constraintCb = new ComboBox();
        constraintCb.Items.AddRange(Enum.GetNames(typeof(SecondaryConstraint.Constraints)).ToArray());
        constraintCb.SelectedIndexChanged += (object sender, EventArgs e) => ConstraintCb_SelectedIndexChanged(constraintCb, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
        var consequencesCb = new ComboBox();
        consequencesCb.Items.AddRange(Enum.GetNames(typeof(SecondaryConstraint.Consequences)).ToArray());


        SecondaryConstrainsTableLayoutPanel.RowCount++;
        SecondaryConstrainsTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 50F));
        SecondaryConstrainsTableLayoutPanel.Controls.Add(columnsCb, 0, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
        SecondaryConstrainsTableLayoutPanel.Controls.Add(constraintCb, 1, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
        SecondaryConstrainsTableLayoutPanel.Controls.Add(consequencesCb, 2, SecondaryConstrainsTableLayoutPanel.RowCount - 1);
    }

    private void ConstraintCb_SelectedIndexChanged(ComboBox cb, int rowIndex)
    {
        if (SecondaryConstrainsTableLayoutPanel.Controls.Count >= rowIndex * 4)
        {
            //todo this isn't right
            //SecondaryConstrainsTableLayoutPanel.Controls.RemoveAt(rowIndex * 4);
        }
        var gb = new GroupBox();
        gb.Width = 100;
        gb.Height = 1000;
        gb.AutoSize = true;
        gb.BackColor = System.Drawing.Color.BlueViolet;
        gb.Location = new Point(6, 19);


        var selectedType = (SecondaryConstraint.Constraints)cb.SelectedIndex;
        switch (selectedType)
        {
            case SecondaryConstraint.Constraints.REGULAREXPRESSION:
                var argsGb = new GroupBox();
                argsGb.Text = "Pattern";
                var tb = new TextBox();
                tb.Width = 200;
                tb.Height = 40;
                tb.Name = "patternArg";
                tb.Location = new Point(6, 19);
                argsGb.Controls.Add(tb);
                gb.Controls.Add(argsGb);
                break;
            case SecondaryConstraint.Constraints.BOUNDDATE:
            case SecondaryConstraint.Constraints.BOUNDDOUBLE:
                var gb1 = new GroupBox();
                gb1.Width = 100;
                gb1.Text = "Lower Bound Column 2";
                var cb1 = new ComboBox();
                cb1.Width = 200;
                cb1.Name = "lowerBoundColumn";
                cb1.Items.AddRange(_catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToArray());
                cb1.Location = new Point(6, 19);
                gb1.Controls.Add(cb1);
                gb.Controls.Add(gb1);

                //var gb2 = new GroupBox();
                //gb2.Location = new Point(6, 60);
                //gb2.Text = "Lower Bound";
                //var tb1 = new TextBox();
                //tb1.Width = 200;
                //tb1.Height = 40;
                //tb1.Name = "lowerBound";
                //tb1.Location = new Point(6, 19);
                //gb2.Controls.Add(tb1);
                //gb.Controls.Add(gb2);

                //var gb3 = new GroupBox();
                //gb3.Location = new Point(6, 100);
                //gb3.Text = "Upper Bound Column";
                //var cb2 = new ComboBox();
                //cb2.Width = 200;
                //cb2.Height = 40;
                //cb2.Name = "upperBoundColumn";
                //cb2.Items.AddRange(_catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).ToArray());
                //cb2.Location = new Point(6, 19);
                //gb3.Controls.Add(cb2);
                //gb.Controls.Add(gb2);

                //var gb4 = new GroupBox();
                //gb4.Location = new Point(6, 140);

                //gb2.Text = "Upper Bound";
                //var tb2 = new TextBox();
                //tb2.Width = 200;
                //tb2.Height = 40;
                //tb2.Name = "lowerBound";
                //tb2.Location = new Point(6, 19);
                //gb4.Controls.Add(tb2);
                //gb.Controls.Add(gb4);
                break;
            case SecondaryConstraint.Constraints.NOTNULL:
            default:
                break;
        }
        SecondaryConstrainsTableLayoutPanel.Controls.Add(gb, 3, rowIndex);
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
            //todo for some reason the save doesn't work...
            secondary.SaveToDatabase();
            var args = SecondaryConstrainsTableLayoutPanel.GetControlFromPosition(3, row + 1) as GroupBox;
            switch (constraint)
            {
                case Constraints.REGULAREXPRESSION:
                    var pattern = args.Controls.Find("patternArg", true).First();
                    var scArg = new SecondaryConstraintArgument(_dqeRepository, "Pattern", pattern.Text, secondary);
                    scArg.SaveToDatabase();
                    break;
                default:
                    break;
            }


            //SecondaryConstraintArgument
            //    var y = primaryConstrainsTableLayout.GetControlFromPosition(1, row + 1);

            //    var constraint = primaryConstrainsTableLayout.GetControlFromPosition(1, row + 1) as ComboBox;
            //    var result = primaryConstrainsTableLayout.GetControlFromPosition(2, row + 1) as ComboBox;
            //    var existingEvaulation = _primaryConstraints.Where(pc => pc.ColumnInfo.GetRuntimeName() == label.Text).FirstOrDefault();
            //    if (existingEvaulation != null)
            //    {
            //        existingEvaulation.Contraint = (PrimaryContraint.Contraints)constraint.SelectedIndex;
            //        existingEvaulation.Result = (PrimaryContraint.ConstraintResults)result.SelectedIndex;
            //        existingEvaulation.SaveToDatabase();
            //    }
            //    else
            //    {
            //        if (constraint.SelectedIndex >= 0 && result.SelectedIndex >= 0)
            //        {
            //            var newEval = new PrimaryContraint(_dqeRepository,
            //                _catalogue.CatalogueItems.Select(ci => ci.ColumnInfo).Where(ci => ci.GetRuntimeName() == label.Text).FirstOrDefault(),
            //                (PrimaryContraint.Contraints)constraint.SelectedIndex,
            //                (PrimaryContraint.ConstraintResults)result.SelectedIndex
            //            );
            //            newEval.SaveToDatabase();
            //        }
            //        //todo warn about not selecting both
        }
    }

    private void cbUsePrevious_CheckedChanged(object sender, EventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueAnalysisExecutionControlUI_Design, UserControl>))]
public abstract class CatalogueAnalysisExecutionControlUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}