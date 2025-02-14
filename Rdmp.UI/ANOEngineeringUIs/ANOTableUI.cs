// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using FAnsi.Discovery;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.ANOEngineeringUIs;

/// <summary>
/// Displays the live status of an ANOTable (is it pushed or not) and how many rows it has.  Also allows dropping/changing the anonymisation schema (number of
/// integers / characters to use in anonymous format) when the ANOTable is not pushed to the ANOStore.Database.
/// </summary>
public partial class ANOTableUI : ANOTableUI_Design
{
    private ANOTable _anoTable;
    private readonly ErrorProvider _serverErrorProvider = new();

    public ANOTableUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.Catalogue;
    }

    public override void SetDatabaseObject(IActivateItems activator, ANOTable databaseObject)
    {
        _anoTable = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);

        llServer.Text = _anoTable.Server.Name;

        CommonFunctionality.AddChecks(databaseObject);
        CommonFunctionality.StartChecking();

        SetEnabledness();

        CommonFunctionality.AddHelp(tbSuffix, "ANOTable.Suffix");
        CommonFunctionality.AddHelp(llServer, "ANOTable.Server_ID");
        CommonFunctionality.AddHelpString(tbInputDataType, "DataType",
            "Datatype for private identifiers being mapped e.g. varchar(100)");
        CommonFunctionality.AddHelp(nIntegers, "ANOTable.NumberOfIntegersToUseInAnonymousRepresentation");
        CommonFunctionality.AddHelp(nCharacters, "ANOTable.NumberOfCharactersToUseInAnonymousRepresentation");

        if (!_anoTable.Server.WasCreatedBy(new ANOStorePatcher()))
            _serverErrorProvider.SetError(llServer, "Server is not an ANO server");
        else
            _serverErrorProvider.Clear();
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ANOTable databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", a => a.ID);
        Bind(nIntegers, "Value", "NumberOfIntegersToUseInAnonymousRepresentation",
            a => a.NumberOfIntegersToUseInAnonymousRepresentation);
        Bind(nCharacters, "Value", "NumberOfCharactersToUseInAnonymousRepresentation",
            a => a.NumberOfCharactersToUseInAnonymousRepresentation);
        Bind(tbName, "Text", "TableName", a => a.TableName);
        Bind(tbSuffix, "Text", "Suffix", a => a.Suffix);
    }

    private void SetEnabledness()
    {
        DiscoveredTable pushedTable;
        try
        {
            pushedTable = _anoTable.GetPushedTable();
        }
        catch (Exception e)
        {
            CommonFunctionality.Fatal("Could not reach ANO Server", e);
            return;
        }

        var isPushed = pushedTable != null;

        nIntegers.Enabled = !isPushed;
        nCharacters.Enabled = !isPushed;
        btnFinalise.Enabled = !isPushed;
        tbInputDataType.Enabled = !isPushed;

        btnDropANOTable.Enabled = isPushed;
        gbPushedTable.Visible = isPushed;

        if (isPushed)
        {
            tbInputDataType.Text = _anoTable.GetRuntimeDataType(LoadStage.AdjustRaw);

            lblANOTableName.Text = pushedTable.GetRuntimeName();
            var cols = pushedTable.DiscoverColumns();

            lblPrivate.Text = $"{cols[0].GetRuntimeName()} {cols[0].DataType.SQLType}";
            lblPublic.Text = $"{cols[1].GetRuntimeName()} {cols[1].DataType.SQLType}";

            lblRowCount.Text = $"{pushedTable.GetRowCount()} rows";
        }
    }

    private void btnFinalise_Click(object sender, EventArgs e)
    {
        ragSmiley1.Reset();
        _anoTable.PushToANOServerAsNewTable(tbInputDataType.Text, ragSmiley1);
        SetEnabledness();
    }

    private void btnDropANOTable_Click(object sender, EventArgs e)
    {
        ragSmiley1.Reset();
        try
        {
            _anoTable.DeleteANOTableInANOStore();
        }
        catch (Exception exception)
        {
            ragSmiley1.OnCheckPerformed(new CheckEventArgs("Drop failed", CheckResult.Fail, exception));
        }

        SetEnabledness();
    }

    private void nIntegers_ValueChanged(object sender, EventArgs e)
    {
        _anoTable.NumberOfIntegersToUseInAnonymousRepresentation = (int)nIntegers.Value;
    }

    private void nCharacters_ValueChanged(object sender, EventArgs e)
    {
        _anoTable.NumberOfCharactersToUseInAnonymousRepresentation = (int)nCharacters.Value;
    }

    private void llServer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var cmd = new ExecuteCommandShow(Activator, _anoTable.Server, 0);
        cmd.Execute();
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ANOTableUI_Design, UserControl>))]
public abstract class ANOTableUI_Design : RDMPSingleDatabaseObjectControl<ANOTable>;