// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Wizard;

/// <summary>
/// Provides streamlined/simplified access to the cohort creation functionality of RDMP.  The UI lets you pick from existing datasets and existing filters created by your Data Manager for
/// interacting with those datasets including specifying values for arguments e.g. TestCode = 'CRE'.
/// 
/// <para>Initially you are limited to 2 inclusion criteria and 2 exclsuion criteria (datasets).  Upon completing the wizard you will be taken to the execution screen of the Cohort Identification
/// Configuration created.  There you can test/refine your configuration as well as add more datasets and deeper nesting of set operations as required.</para>
/// 
/// </summary>
public partial class CreateNewCohortIdentificationConfigurationUI : RDMPForm
{
    private Size _smallSize = new(755, 140);
    private Size _bigSize = new(1368, 876);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CohortIdentificationConfiguration CohortIdentificationCriteriaCreatedIfAny { get; private set; }

    public CreateNewCohortIdentificationConfigurationUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();

        if (VisualStudioDesignMode)
            return;

        inclusionCriteria1.SetupFor(Activator);

        setOperationInclude.SetupFor(Activator, true);
        setOperationExclude.SetupFor(Activator, false);

        Size = _smallSize;
    }

    private void CheckBoxChanged(object sender, EventArgs e)
    {
        var cb = (CheckBox)sender;

        SimpleCohortSetUI target = null;

        if (sender == cbExclusion1)
            target = exclusionCriteria1;
        if (sender == cbExclusion2)
        {
            if (exclusionCriteria1.Catalogue == null && cb.Checked)
            {
                cb.Checked = false;
                return;
            }

            target = exclusionCriteria2;
        }

        if (sender == cbInclusion2)
        {
            if (inclusionCriteria1.Catalogue == null && cb.Checked)
            {
                cb.Checked = false;
                return;
            }

            target = inclusionCriteria2;
        }

        if (target == null)
            throw new ArgumentException("sender");

        if (cb.Checked)
        {
            target.SetupFor(Activator);
            target.Enabled = true;
        }
        else
        {
            target.Clear();
            target.Enabled = false;
        }

        setOperationInclude.Enabled = cbInclusion2.Checked;
        setOperationExclude.Enabled = cbExclusion1.Checked && cbExclusion2.Checked;
    }

    private void btnGo_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbName.Text))
        {
            MessageBox.Show("Enter a name for your Cohort Identification Criteria");
            return;
        }

        if (cbUseWizard.Checked &&
            !Activator.YesNo(
                "Are you sure you are happy with your configuration, this wizard will close after creating?",
                "Confirm"))
            return;

        var cic = CreateCohortIdentificationConfiguration();

        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cic));

        CohortIdentificationCriteriaCreatedIfAny = cic;
        DialogResult = DialogResult.OK;

        Close();
    }

    private CohortIdentificationConfiguration CreateCohortIdentificationConfiguration()
    {
        var cic = new CohortIdentificationConfiguration(Activator.RepositoryLocator.CatalogueRepository, tbName.Text);

        cic.CreateRootContainerIfNotExists();
        var root = cic.RootCohortAggregateContainer;
        root.Name = ExecuteCommandCreateNewCohortIdentificationConfiguration.RootContainerName;

        //If we're not using the wizard then just return an empty CIC
        if (!cbUseWizard.Checked)
        {
            root.Operation = SetOperation.UNION;
            root.SaveToDatabase();
            return cic;
        }

        //We're using the wizard, so this builds a framework
        root.Operation = SetOperation.EXCEPT;
        root.SaveToDatabase();

        //Create include and exclude containers
        var includeContainer = setOperationInclude.CreateCohortAggregateContainer(root);
        var excludeContainer = setOperationExclude.CreateCohortAggregateContainer(root);

        if (inclusionCriteria1.Catalogue != null || cbExclusion1.Checked || cbExclusion2.Checked)
        {
            inclusionCriteria1.CreateCohortSet(includeContainer);

            if (cbInclusion2.Checked)
                inclusionCriteria2.CreateCohortSet(includeContainer);
        }

        if (cbExclusion1.Checked || cbExclusion2.Checked)
        {
            if (cbExclusion1.Checked)
                exclusionCriteria1.CreateCohortSet(excludeContainer);

            if (cbExclusion2.Checked)
                exclusionCriteria2.CreateCohortSet(excludeContainer);
        }

        return cic;
    }

    private void CreateNewCohortIdentificationConfigurationUI_Load(object sender, EventArgs e)
    {
        tbName.Focus();
    }

    private void tbName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) btnGo_Click(this, EventArgs.Empty);
    }

    private void cbUseWizard_CheckedChanged(object sender, EventArgs e)
    {
        pnlWizard.Visible = cbUseWizard.Checked;
        pnlWizard.Enabled = cbUseWizard.Checked;

        if (cbUseWizard.Checked)
            Size = _bigSize;
        else
            Size = _smallSize;
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }
}