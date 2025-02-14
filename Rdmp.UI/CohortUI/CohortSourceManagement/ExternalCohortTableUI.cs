// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.CohortUI.CohortSourceManagement;

/// <summary>
/// Allows you to edit an external cohort reference.  This is the location of a cohort database and includes the names of the Cohort table and the names of
/// private/release identifiers in the database
/// </summary>
public partial class ExternalCohortTableUI : ExternalCohortTableUI_Design, ISaveableUI
{
    private ExternalCohortTable _externalCohortTable;

    public ExternalCohortTableUI()
    {
        InitializeComponent();

        AssociatedCollection = RDMPCollection.SavedCohorts;

        serverDatabaseTableSelector1.HideTableComponents();
        serverDatabaseTableSelector1.SelectionChanged += SaveDatabaseSettings;
    }

    private void SaveDatabaseSettings()
    {
        var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();

        if (db == null)
            return;

        _externalCohortTable.Server = db.Server.Name;
        _externalCohortTable.Database = db.GetRuntimeName();
        _externalCohortTable.Username = db.Server.ExplicitUsernameIfAny;
        _externalCohortTable.Password = db.Server.ExplicitPasswordIfAny;
        _externalCohortTable.DatabaseType = db.Server.DatabaseType;
    }

    public override void SetDatabaseObject(IActivateItems activator, ExternalCohortTable databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _externalCohortTable = databaseObject;

        serverDatabaseTableSelector1.DatabaseType = _externalCohortTable.DatabaseType;

        string password;
        try
        {
            password = _externalCohortTable.GetDecryptedPassword();
        }
        catch (Exception)
        {
            password = null;
        }

        serverDatabaseTableSelector1.SetExplicitDatabase(_externalCohortTable.Server, _externalCohortTable.Database,
            _externalCohortTable.Username, password);

        CommonFunctionality.AddHelp(tbTableName, "IExternalCohortTable.TableName");
        CommonFunctionality.AddHelp(tbPrivateIdentifierField, "IExternalCohortTable.PrivateIdentifierField");
        CommonFunctionality.AddHelp(tbReleaseIdentifierField, "IExternalCohortTable.ReleaseIdentifierField");
        CommonFunctionality.AddHelp(tbDefinitionTableForeignKeyField,
            "IExternalCohortTable.DefinitionTableForeignKeyField");

        CommonFunctionality.AddHelp(tbDefinitionTableName, "IExternalCohortTable.DefinitionTableName");

        CommonFunctionality.AddChecks(_externalCohortTable);
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ExternalCohortTable databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", e => e.ID);

        Bind(tbID, "Text", "ID", e => e.ID);
        Bind(tbName, "Text", "Name", e => e.Name);
        Bind(tbTableName, "Text", "TableName", e => e.TableName);
        Bind(tbPrivateIdentifierField, "Text", "PrivateIdentifierField", e => e.PrivateIdentifierField);
        Bind(tbReleaseIdentifierField, "Text", "ReleaseIdentifierField", e => e.ReleaseIdentifierField);
        Bind(tbDefinitionTableForeignKeyField, "Text", "DefinitionTableForeignKeyField",
            e => e.DefinitionTableForeignKeyField);

        Bind(tbDefinitionTableName, "Text", "DefinitionTableName", e => e.DefinitionTableName);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExternalCohortTableUI_Design, UserControl>))]
public abstract class ExternalCohortTableUI_Design : RDMPSingleDatabaseObjectControl<ExternalCohortTable>;