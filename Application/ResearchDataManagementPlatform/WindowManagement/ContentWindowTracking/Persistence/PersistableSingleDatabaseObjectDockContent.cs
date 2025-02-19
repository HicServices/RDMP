// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;


namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

/// <summary>
/// A Document Tab that hosts an RDMPSingleDatabaseObjectControl T, the control knows how to save itself to the persistence settings file for the user ensuring that when they next open the
/// software the Tab can be reloaded and displayed.  Persistance involves storing this Tab type, the Control type being hosted by the Tab (a RDMPSingleDatabaseObjectControl) and the object
/// ID , object Type and Repository (DataExport or Catalogue) of the T object currently held in the RDMPSingleDatabaseObjectControl.
/// </summary>
[System.ComponentModel.DesignerCategory("")]
[TechnicalUI]
public class PersistableSingleDatabaseObjectDockContent : RDMPSingleControlTab
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IMapsDirectlyToDatabaseTable DatabaseObject { get; private set; }

    public const string Prefix = "RDMPSingleDatabaseObjectControl";

    public PersistableSingleDatabaseObjectDockContent(IRDMPSingleDatabaseObjectControl control,
        IMapsDirectlyToDatabaseTable databaseObject, RefreshBus refreshBus) : base(refreshBus)
    {
        Control = (Control)control;

        DatabaseObject = databaseObject;
        TabText = "Loading...";

        control.UnSavedChanges += OnUnSavedChanges;
        Closing += (s, e) => control.UnSavedChanges -= OnUnSavedChanges;
    }

    private void OnUnSavedChanges(object sender, bool unsavedChanges)
    {
        if (TabText == null)
            return;

        TabText = unsavedChanges ? $"{TabText.TrimEnd('*')}*" : TabText.TrimEnd('*');
    }

    protected override string GetPersistString()
    {
        const char s = PersistStringHelper.Separator;
        return Prefix + s + Control.GetType().FullName + s + DatabaseObject.Repository.GetType().FullName + s +
               DatabaseObject.GetType().FullName + s + DatabaseObject.ID;
    }

    public override void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        var newTabName = ((IRDMPSingleDatabaseObjectControl)Control).GetTabName();

        if (ParentForm is CustomFloatWindow floatWindow)
            floatWindow.Text = newTabName;

        TabText = newTabName;
    }

    public override void HandleUserRequestingTabRefresh(IActivateItems activator)
    {
        var cmd = new ExecuteCommandRefreshObject(activator, DatabaseObject as DatabaseEntity);

        if (!cmd.IsImpossible)
            cmd.Execute();
    }

    public override void HandleUserRequestingEmphasis(IActivateItems activator)
    {
        activator.RequestItemEmphasis(this, new EmphasiseRequest(DatabaseObject));
    }
}