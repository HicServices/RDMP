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

namespace Rdmp.UI.CatalogueAnalysisUIs;

public partial class CatalogueAnalysisExecutionControlUI : CatalogueAnalysisExecutionControlUI_Design
{
    private Catalogue _catalogue;
    private DQERepository _dqeRepository;
    private List<PrimaryContraint> _primaryConstraints;
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

            primaryConstrainsTableLayout.RowCount++;
            primaryConstrainsTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            primaryConstrainsTableLayout.Controls.Add(new Label() { Text = ci.GetRuntimeName() }, 0, primaryConstrainsTableLayout.RowCount - 1);
            primaryConstrainsTableLayout.Controls.Add(constraintsDropdown, 1, primaryConstrainsTableLayout.RowCount - 1);
            primaryConstrainsTableLayout.Controls.Add(resultsDropdown, 2, primaryConstrainsTableLayout.RowCount - 1);
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
        var evaluation = _dqeRepository.GetAllObjectsWhere<CatalogueValidation>("Catalogue_ID", _catalogue.ID).Last();
        timePeriodicityChart1.SelectEvaluation(evaluation,"ALL");
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
        foreach (var row in Enumerable.Range(1, rowCount-2))
        {
            var label = primaryConstrainsTableLayout.GetControlFromPosition(0, row+1);

            var y = primaryConstrainsTableLayout.GetControlFromPosition(1, row+1);

            var constraint = primaryConstrainsTableLayout.GetControlFromPosition(1, row+1) as ComboBox;
            var result = primaryConstrainsTableLayout.GetControlFromPosition(2, row+1) as ComboBox;
            var existingEvaulation = _primaryConstraints.Where(pc => pc.ColumnInfo.GetRuntimeName() == label.Text).FirstOrDefault();
            if (existingEvaulation != null)
            {
                existingEvaulation.Contraint = (PrimaryContraint.Contraints)constraint.SelectedIndex;
                existingEvaulation.Result = (PrimaryContraint.ConstraintResults)result.SelectedIndex;
                existingEvaulation.SaveToDatabase();
            }
            else
            {
                if(constraint.SelectedIndex >= 0 && result.SelectedIndex >=0)
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
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueAnalysisExecutionControlUI_Design, UserControl>))]
public abstract class CatalogueAnalysisExecutionControlUI_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}