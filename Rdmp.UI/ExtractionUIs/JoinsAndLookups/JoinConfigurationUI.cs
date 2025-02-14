// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

/// <summary>
/// Many researchers like flat tables they can load into SPSS or STATA or Excel and thus would prefer not to deal with multiple tables if possible.  Storing datasets as flat
/// tables however is often suboptimal in terms of performance and storage space.  Therefore it is possible to configure a dataset (Catalogue) which includes columns from
/// multiple tables.  For example if you have a Header and Results table in which Header tells you when a test was done and by whom including sample volume etc and each test
/// gives multiple results (white blood cell count, red blood cell count etc) then you will obviously want to store it as two separate tables.
/// 
/// <para>Configuring join logic in RDMP enables the QueryBuilder to write an SQL SELECT query including a LEFT JOIN / RIGHT JOIN / INNER JOIN over two or more tables.  If you don't
/// understand what an SQL Join is then you should research this before using this window.  Unlike a Lookup (See LookupConfiguration) it doesn't really matter which table contains
/// the ForeignKey and which contains the PrimaryKey since changing the Join direction effectively inverts this logic anyway (i.e. Header RIGHT JOIN Results is the same as Results
/// LEFT JOIN Header).  Configuring join logic in the RDMP database does not affect your data repository in any way (i.e. it doesn't mean we will suddenly start putting referential
/// integrity constraints into your database!).</para>
///  
/// <para>You might wonder why you have to configure JoinInfo information into RDMP when it is possibly already implemented in your data model (e.g. with foreign key constraints).  The
/// explicit record in the RDMP database allows you to hold corrupt/unlinkable data (which would violate a foreign key constraint) and still know that the tables must be joined.
/// Additionally it lets you configure joins between tables in different databases and to specify an explicit direction (LEFT / RIGHT / INNER) which is always the same when it comes
/// time to extract your data for researchers.</para>
/// 
/// <para>If you need to join on more than 1 column then just create a JoinInfo for each pair of columns (making sure the direction - LEFT/RIGHT/INNER matches).  For example if the join is
/// Header.LabNumber = Results.LabNumber AND Header.Hospital = Results.Hospital (because of crossover in LabNumber between hospitals) then you would configure a JoinInfo for
/// Header.LabNumber = Results.LabNumber and another for Header.Hospital = Results.Hospital.</para>
/// </summary>
public partial class JoinConfigurationUI : JoinConfiguration_Design
{
    private TableInfo _leftTableInfo;
    private TableInfo _rightTableInfo;

    public JoinConfigurationUI()
    {
        InitializeComponent();
        fk1.KeyType = JoinKeyType.ForeignKey;
        fk2.KeyType = JoinKeyType.ForeignKey;
        fk3.KeyType = JoinKeyType.ForeignKey;

        //cannot drop anything until you pick a foreign key table
        fk1.IsValidGetter = c => false;
        fk2.IsValidGetter = c => false;
        fk3.IsValidGetter = c => false;

        olvLeftColumns.RowHeight = 19;
        olvRightColumns.RowHeight = 19;
        AssociatedCollection = RDMPCollection.Tables;
    }

    public override void SetDatabaseObject(IActivateItems activator, TableInfo databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        olvLeftColumnNames.ImageGetter = o => activator.CoreIconProvider.GetImage(o).ImageToBitmap();
        olvRightColumnNames.ImageGetter = o => activator.CoreIconProvider.GetImage(o).ImageToBitmap();

        _leftTableInfo = databaseObject;
        tbLeftTableInfo.Text = _leftTableInfo.ToString();

        btnChooseRightTableInfo.Image = activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Add)
            .ImageToBitmap();
        UpdateValidityAssessment();

        olvLeftColumns.ClearObjects();
        olvLeftColumns.AddObjects(_leftTableInfo.ColumnInfos);

        if (pk1.IsValidGetter == null)
        {
            pk1.IsValidGetter = c => c.TableInfo_ID == databaseObject.ID;
            pk2.IsValidGetter = c => c.TableInfo_ID == databaseObject.ID;
            pk3.IsValidGetter = c => c.TableInfo_ID == databaseObject.ID;
        }
    }

    private void btnChooseRightTableInfo_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription = $"Which other table should be joined to '{_leftTableInfo.Name}'?"
        }, _leftTableInfo.Repository.GetAllObjects<TableInfo>().Where(t => t.ID != _leftTableInfo.ID).ToArray(),
                out var selected))
            SetRightTableInfo(selected);
    }

    private void SetRightTableInfo(TableInfo t)
    {
        //it's the same as the last one!
        if (_rightTableInfo != null && _rightTableInfo.ID == t.ID)
            return;

        _rightTableInfo = t;
        tbRightTableInfo.Text = t.ToString();

        fk1.Clear();
        fk2.Clear();
        fk3.Clear();

        fk1.IsValidGetter = c => c.TableInfo_ID == t.ID;
        fk2.IsValidGetter = c => c.TableInfo_ID == t.ID;
        fk3.IsValidGetter = c => c.TableInfo_ID == t.ID;

        olvRightColumns.ClearObjects();
        olvRightColumns.AddObjects(_rightTableInfo.ColumnInfos);
    }

    private void k_SelectedColumnChanged()
    {
        UpdateValidityAssessment();
    }

    private void UpdateValidityAssessment(bool actuallyDoIt = false)
    {
        ragSmiley1.Reset();
        try
        {
            var fks = new ColumnInfo[] { fk1.SelectedColumn, fk2.SelectedColumn, fk3.SelectedColumn }
                .Where(f => f != null).ToArray();
            var pks = new ColumnInfo[] { pk1.SelectedColumn, pk2.SelectedColumn, pk3.SelectedColumn }
                .Where(p => p != null).ToArray();

            if (fk1.SelectedColumn == null || pk1.SelectedColumn == null)
                throw new Exception(
                    "You must specify at least one pair of keys to join on, do this by dragging columns out of the collection into the key boxes");

            if (
                pk2.SelectedColumn == null != (fk2.SelectedColumn == null)
                ||
                pk3.SelectedColumn == null != (fk3.SelectedColumn == null))
                throw new Exception(
                    "You must have the same number of primary and foreign keys (they must come in pairs)");

            if (pks.Any(p => p.TableInfo_ID != _leftTableInfo.ID))
                throw new Exception("All Primary Keys must come from the Left hand TableInfo");

            if (fks.Any(f => f.TableInfo_ID != _rightTableInfo.ID))
                throw new Exception("All Foreign Keys must come from the Right hand TableInfo");


            ExtractionJoinType joinType;
            if (rbAllLeftHandTableRecords.Checked)
                joinType = ExtractionJoinType
                    .Right; //confusing I know, basically JoinInfo database record has fk,pk and direction field assuming fk joins via that direction to pk which is the opposite to the layout of this form
            else if (rbAllRightHandTableRecords.Checked)
                joinType = ExtractionJoinType.Left;
            else if (rbJoinInner.Checked)
                joinType = ExtractionJoinType.Inner;
            else
                throw new Exception("You must select an Extraction Join direction");

            var cataRepo = _leftTableInfo.CatalogueRepository;

            for (var i = 0; i < pks.Length; i++)
                if (cataRepo.GetAllObjects<JoinInfo>()
                    .Any(j => j.PrimaryKey_ID == pks[i].ID && j.ForeignKey_ID == fks[i].ID))
                    throw new Exception($"Join already exists between {fks[i]} and {pks[i].ID}");


            if (actuallyDoIt)
            {
                for (var i = 0; i < pks.Length; i++)
                    new JoinInfo(cataRepo, fks[i], pks[i], joinType, tbCollation.Text);

                MessageBox.Show("Successfully Created Joins");
                Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_leftTableInfo));

                foreach (var ui in new[] { pk1, pk2, pk3, fk1, fk2, fk3 })
                    ui.Clear();
            }
            else
            {
                btnCreateJoinInfo.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            btnCreateJoinInfo.Enabled = false;
            ragSmiley1.Fatal(ex);
        }
    }

    private void btnCreateJoinInfo_Click(object sender, EventArgs e)
    {
        UpdateValidityAssessment(true);
    }

    private void tbFilterLeft_TextChanged(object sender, EventArgs e)
    {
        olvLeftColumns.UseFiltering = true;
        olvLeftColumns.ModelFilter =
            new TextMatchFilter(olvLeftColumns, tbFilterLeft.Text, StringComparison.CurrentCultureIgnoreCase);
    }

    private void tbFilterRight_TextChanged(object sender, EventArgs e)
    {
        olvRightColumns.UseFiltering = true;
        olvRightColumns.ModelFilter = new TextMatchFilter(olvRightColumns, tbFilterRight.Text,
            StringComparison.CurrentCultureIgnoreCase);
    }

    private void rb_CheckedChanged(object sender, EventArgs e)
    {
        UpdateValidityAssessment();
    }

    public void SetOtherTableInfo(TableInfo otherTableInfo)
    {
        SetRightTableInfo(otherTableInfo);
    }

    private void olvRightColumns_DragEnter(object sender, DragEventArgs e)
    {
        var tableInfo = GetTableInfoOrNullFromDrag(e);

        if (tableInfo != null)
            e.Effect = DragDropEffects.Copy;
    }

    private void olvRightColumns_DragDrop(object sender, DragEventArgs e)
    {
        var tableInfo = GetTableInfoOrNullFromDrag(e);

        if (tableInfo != null)
            SetRightTableInfo(tableInfo);
    }

    private static TableInfo GetTableInfoOrNullFromDrag(DragEventArgs e)
    {
        return e.Data is not OLVDataObject data || data.ModelObjects.Count != 1
            ? null
            : data.ModelObjects[0] switch
            {
                TableInfo ti => ti,
                TableInfoCombineable ticmd => ticmd.TableInfo,
                _ => null
            };
    }

    private void tbCollation_Leave(object sender, EventArgs e)
    {
        if (tbCollation.Text != null &&
            tbCollation.Text.StartsWith("collate", StringComparison.CurrentCultureIgnoreCase))
            tbCollation.Text = tbCollation.Text["collate".Length..].Trim();
    }

    private void olv_ItemActivate(object sender, EventArgs e)
    {
        var olv = (ObjectListView)sender;

        if (olv.SelectedObject is IMapsDirectlyToDatabaseTable o)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o));
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<JoinConfiguration_Design, UserControl>))]
public abstract class JoinConfiguration_Design : RDMPSingleDatabaseObjectControl<TableInfo>;