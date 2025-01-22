// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.UI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;


namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

/// <summary>
/// Allows you to persist user interfaces which are built on more than one RDMP database object (if you only require one object you should use RDMPSingleDatabaseObjectControl instead
/// </summary>
[DesignerCategory("")]
[TechnicalUI]
public class PersistableObjectCollectionDockContent : RDMPSingleControlTab
{
    private readonly IObjectCollectionControl _control;

    public const string Prefix = "RDMPObjectCollection";

    public IPersistableObjectCollection Collection => _control.GetCollection();

    public PersistableObjectCollectionDockContent(IActivateItems activator, IObjectCollectionControl control,
        IPersistableObjectCollection collection) : base(activator.RefreshBus, collection.GetType().ToString())
    {
        _control = control;
        Control = (Control)control;

        //tell the control what its collection is
        control.SetCollection(activator, collection);

        //ask the control what it wants its name to be
        TabText = _control.GetTabName();
    }

    protected override string GetPersistString()
    {
        var collection = _control.GetCollection();
        const char s = PersistStringHelper.Separator;

        //Looks something like this  RDMPObjectCollection:MyCoolControlUI:MyControlUIsBundleOfObjects:[CatalogueRepository:AggregateConfiguration:105,CatalogueRepository:AggregateConfiguration:102,CatalogueRepository:AggregateConfiguration:101]###EXTRA_TEXT###I've got a lovely bunch of coconuts
        var sb = new StringBuilder();

        //Output <Prefix>:<The Control Type>:<The Type name of the Collection - must be new()>:
        sb.Append(Prefix + s + _control.GetType().FullName + s + collection.GetType().Name + s);

        sb.Append(PersistStringHelper.GetObjectCollectionPersistString(collection.DatabaseObjects.ToArray()));

        //now add the bit that starts the user specific text
        sb.Append(PersistStringHelper.ExtraText);

        //let him save whatever text he wants
        sb.Append(collection.SaveExtraText());

        return sb.ToString();
    }


    public override void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        var newTabName = _control.GetTabName();

        if (ParentForm is CustomFloatWindow floatWindow)
            floatWindow.Text = newTabName;

        TabText = newTabName;

        //pass the info on to the control
        _control.RefreshBus_RefreshObject(sender, e);
    }

    public override void HandleUserRequestingTabRefresh(IActivateItems activator)
    {
        var collection = _control.GetCollection();

        foreach (var o in collection.DatabaseObjects)
            if (o is IRevertable revertable)
                revertable.RevertToDatabaseState();

        _control.SetCollection(activator, collection);
    }


    public override void HandleUserRequestingEmphasis(IActivateItems activator)
    {
        var collection = _control.GetCollection();

        if (collection != null)
            if (collection.DatabaseObjects.Count >= 1)
            {
                var o = activator.SelectOne("Show", collection.DatabaseObjects.ToArray(), null, true);

                if (o != null)
                    activator.RequestItemEmphasis(this, new EmphasiseRequest(o));
            }
    }
}