// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.DashboardTabs.Construction.Exceptions;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DashboardTabs;

/// <summary>
/// Allows you to create an arrangement of IDashboardableControls on a Form that is stored in the Catalogue database and viewable by all RDMP users.  Use the task bar at the top of the
/// screen to add new controls.  Then click the spanner to drag and resize them.  Each control may also have some flexibility in how it is configured which is accessible in edit mode.
/// </summary>
public partial class DashboardLayoutUI : DashboardLayoutUI_Design
{
    private DashboardLayout _layout;
    private DashboardControlFactory _controlFactory;
    private readonly DashboardEditModeFunctionality _editModeFunctionality;

    public Dictionary<DashboardControl, DashboardableControlHostPanel> ControlDictionary = new();

    public DashboardLayoutUI()
    {
        InitializeComponent();
        _editModeFunctionality = new DashboardEditModeFunctionality(this);

        AssociatedCollection = RDMPCollection.Catalogue;
    }

    private void btnEditMode_CheckedChanged(object sender, EventArgs e)
    {
        _editModeFunctionality.EditMode = btnEditMode.Checked;
    }

    public override void SetDatabaseObject(IActivateItems activator, DashboardLayout databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _controlFactory = new DashboardControlFactory(activator, new Point(5, 25));
        btnAddDashboardControl.Image = activator.CoreIconProvider.GetImage(RDMPConcept.AggregateGraph, OverlayKind.Add)
            .ImageToBitmap();
        _layout = databaseObject;
        ReLayout();

        activator.Theme.ApplyTo(toolStrip1);
    }

    private void ReLayout()
    {
        //remove old controls
        foreach (var kvp in ControlDictionary)
            Controls.Remove(kvp.Value);

        //restart audit of controls
        ControlDictionary.Clear();

        tbDashboardName.Text = _layout.Name;
        cbxAvailableControls.Items.Clear();
        cbxAvailableControls.Items.AddRange(_controlFactory.GetAvailableControlTypes());
        cbxAvailableControls.SelectedItem = cbxAvailableControls.Items.Cast<object>().FirstOrDefault();

        foreach (var c in _layout.Controls)
        {
            DashboardableControlHostPanel instance;
            try
            {
                instance = _controlFactory.Create(c);
            }
            catch (DashboardControlHydrationException e)
            {
                if (Activator.YesNo(
                        $"Error Hydrating Control '{c}', Do you want to delete the control? (No will attempt to clear the control state but leave it on the Dashboard).  Exception was:{Environment.NewLine}{e.Message}",
                        "Delete Broken Control?"))
                {
                    c.DeleteInDatabase();
                    continue;
                }

                c.PersistenceString = "";
                c.SaveToDatabase();
                MessageBox.Show("Control state cleared, we will now try to reload it");
                instance = _controlFactory.Create(c);
            }

            ControlDictionary.Add(c, instance);
            Controls.Add(instance);

            //let people know what the edit state is
            _editModeFunctionality.EditMode = btnEditMode.Checked;
        }
    }

    private void tbDashboardName_TextChanged(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbDashboardName.Text))
        {
            tbDashboardName.Text = "No Name";
            tbDashboardName.SelectAll();
        }
    }

    private void tbDashboardName_LostFocus(object sender, EventArgs e)
    {
        if (_layout.Name == tbDashboardName.Text)
            return;

        _layout.Name = tbDashboardName.Text;
        _layout.SaveToDatabase();

        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_layout));
    }

    private void btnAddDashboardControl_Click(object sender, EventArgs e)
    {
        if (cbxAvailableControls.SelectedItem is not Type type)
            return;

        var db = _controlFactory.Create(_layout, type, out var control);
        Controls.Add(control);
        ControlDictionary.Add(db, control);
        Controls.Add(control);
        control.BringToFront();

        //add the new control and tell it with the initial edit state is (also updates all the other controls)
        _editModeFunctionality.EditMode = btnEditMode.Checked;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DashboardLayoutUI_Design, UserControl>))]
public abstract class DashboardLayoutUI_Design : RDMPSingleDatabaseObjectControl<DashboardLayout>;