// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

public partial class LookupUI : LookupUI_Design
{
    private Lookup _lookup;

    public LookupUI()
    {
        InitializeComponent();

        olvCompositeJoinColumn.ImageGetter = ImageGetter;
        olvExtractionInformationName.ImageGetter = ImageGetter;
    }

    private Bitmap ImageGetter(object rowObject) => Activator.CoreIconProvider.GetImage(rowObject).ImageToBitmap();

    public override void SetDatabaseObject(IActivateItems activator, Lookup databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _lookup = databaseObject;

        pbColumnInfo1.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo).ImageToBitmap();
        pbColumnInfo2.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ColumnInfo).ImageToBitmap();
        pbTable.Image = activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo).ImageToBitmap();


        tbFk.Text = databaseObject.ForeignKey.Name;
        tbPk.Text = databaseObject.PrimaryKey.Name;
        tbTable.Text = databaseObject.Description.TableInfo.Name;

        UpdateTreeViews();
    }

    private void UpdateTreeViews()
    {
        olvCompositeJoins.ClearObjects();
        olvExtractionDescriptions.ClearObjects();

        var eis = Activator.CoreChildProvider.AllExtractionInformations
            .Value.Where(ei => ei.CatalogueItem.ColumnInfo_ID == _lookup.Description_ID).ToArray();
        olvExtractionDescriptions.AddObjects(eis);

        olvCompositeJoins.AddObjects(_lookup.GetSupplementalJoins().ToArray());
    }

    private void label2_Click(object sender, EventArgs e)
    {
    }

    private void olvExtractionDescriptions_ItemActivate(object sender, EventArgs e)
    {
        if (olvExtractionDescriptions.SelectedObject is ExtractionInformation ei)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(ei));
    }

    private void olv_KeyUp(object sender, KeyEventArgs e)
    {
        if (((ObjectListView)sender).SelectedObject is IDeleteable d)
        {
            Activator.DeleteWithConfirmation(d);
            UpdateTreeViews();
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupUI_Design, UserControl>))]
public abstract class LookupUI_Design : RDMPSingleDatabaseObjectControl<Lookup>
{
}