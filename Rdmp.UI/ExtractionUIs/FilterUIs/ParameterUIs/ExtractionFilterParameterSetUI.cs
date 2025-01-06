// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;

/// <summary>
/// Extraction Filter Parameter Sets are 'known good values' of 1 or more parameters of an extraction filter.  For example you might have a filter 'Hospitalised with conditions'
/// with a parameter @listOfConditionCodes, then you can have multiple ExtractionFilterParameterSets 'Dementia Conditions', 'High Blood Pressure', 'Coronary Heart Disease' which
/// are currated lists of codes that are effectively just a 'good value' for the main filter.
/// 
/// <para>This user interface lets you edit one of these.</para>
/// </summary>
public partial class ExtractionFilterParameterSetUI : ExtractionFilterParameterSetUI_Design, ISaveableUI
{
    private ExtractionFilterParameterSet _extractionFilterParameterSet;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ExtractionFilterParameterSet ExtractionFilterParameterSet
    {
        get => _extractionFilterParameterSet;
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

        var options = ParameterCollectionUIOptionsFactory.Create(ExtractionFilterParameterSet);
        parameterCollectionUI1.SetUp(options, Activator);
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

    private void btnRefreshParameters_Click(object sender, EventArgs e)
    {
        ExtractionFilterParameterSet.CreateNewValueEntries();
        RefreshUIFromDatabase();
    }

    public override void SetDatabaseObject(IActivateItems activator, ExtractionFilterParameterSet databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        ExtractionFilterParameterSet = databaseObject;
    }
}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<ExtractionFilterParameterSetUI_Design, UserControl>))]
public abstract class
    ExtractionFilterParameterSetUI_Design : RDMPSingleDatabaseObjectControl<ExtractionFilterParameterSet>
{
}