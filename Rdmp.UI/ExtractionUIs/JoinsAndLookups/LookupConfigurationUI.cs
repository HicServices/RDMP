// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.MainFormUITabs.SubComponents;
using Rdmp.UI.Menus;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using DragDropEffects = System.Windows.Forms.DragDropEffects;
using Point = System.Drawing.Point;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.ExtractionUIs.JoinsAndLookups;

/// <summary>
/// A Lookup in RDMP is a relationship between three columns.  The 'Foreign Key' column must come from a normal dataset table e.g. 'Prescribing.DrugCode', the 'Primary Key' must come
/// from a different table (usually prefixed z_ to indicate it is a lookup table) e.g. 'z_DrugsLookup.DrugCode' and then a 'Description' column from the same table e.g.
/// 'z_DrugsLookup.DrugName'.  This is maintained in the RDMP Catalogue database and does not result in any changes / constraints on your actual data repository.
/// 
/// <para>While it might seem redundant to have to configure this logic in the RDMP as well as (if you choose to) constraints in your data repository, this approach allows for
/// flexibility when it comes to incomplete/corrupt lookup tables (common in the research data management domain) as well as letting us bundle lookups with data extracts etc.</para>
/// 
/// <para>This window is a low level alternative to LookupConfiguration (the recommended way of creating these Lookup relationships), this form lets you explicitly create a Lookup
/// relationship using the supplied columns.  First of all you should make sure that the column you right clicked to activate the Form is the Description column.  Then select the
/// 'Primary Key' and 'Foreign Key' as described above.  </para>
/// 
/// <para>If you have a particularly insane database design you can configure composite joins (where there are multiple columns that make up a composite 'Foreign Key' / 'Primary Key'.  For
/// example if there was crossover in 'DrugCode' between two countries then the Lookup relationship would need 'Primary Key' Prescribing.DrugCode + Prescribing.Country and the
/// 'Foreign Key' would need to be z_DrugsLookup.DrugCode + z_DrugsLookup.Country.</para>
///
/// <para>Allows you to rapidly import and configure lookup table relationships into the RDMP.  This has two benefits, firstly lookup tables will be automatically included in project extracts
/// of the dataset you are editing.  Secondly lookup columns will be available for inclusion directly into the extraction on a per row basis (for researchers who can't deal with having
/// to lookup the meaning of codes in separate files).</para>
/// 
/// <para>Start by identifying a lookup table and click Import Lookup.  Then drag the primary key of the lookup into the PrimaryKey box.  Then drag the description column of the lookup onto the
/// Foreign key field in the dataset you are modifying.  If you have multiple foreign keys (e.g. two columns SendingLocation and DischargeLocation both of which are location codes) then
/// join them both up (this will give you two lookup description fields SendingLocation_Desc and DischargeLocation_Desc).  </para>
/// 
/// <para>All Lookups and Lookup column description configurations are artifacts in the RDMP database and no actual changes will take place on your data repository (i.e. no constraints will be added
/// to the underlying data database). </para>
/// </summary>
public partial class LookupConfigurationUI : LookupConfiguration_Design
{
    private Catalogue _catalogue;
    private ToolTip toolTip = new();

    //constructor
    public LookupConfigurationUI()
    {
        InitializeComponent();
        olvLookupColumns.RowHeight = 19;
        olvExtractionInformations.RowHeight = 19;
        olvSelectedDescriptionColumns.RowHeight = 19;

        olvLookupColumns.IsSimpleDragSource = true;
        olvExtractionInformations.IsSimpleDragSource = true;

        pk1.KeyType = JoinKeyType.PrimaryKey;
        pk1.SelectedColumnChanged += pk1_SelectedColumnChanged;

        pk2.KeyType = JoinKeyType.PrimaryKey;
        pk2.SelectedColumnChanged += UpdateValidityAssesment;

        pk3.KeyType = JoinKeyType.PrimaryKey;
        pk3.SelectedColumnChanged += UpdateValidityAssesment;

        fk1.KeyType = JoinKeyType.ForeignKey;
        fk1.SelectedColumnChanged += fk1_SelectedColumnChanged;

        fk2.KeyType = JoinKeyType.ForeignKey;
        fk2.SelectedColumnChanged += UpdateValidityAssesment;

        fk3.KeyType = JoinKeyType.ForeignKey;
        fk3.SelectedColumnChanged += UpdateValidityAssesment;

        AssociatedCollection = RDMPCollection.Tables;
    }

    private void UpdateValidityAssesment()
    {
        UpdateValidityAssesment(false);
    }

    private void fk1_SelectedColumnChanged()
    {
        SetStage(
            pk1.SelectedColumn == null ? LookupCreationStage.DragAForeignKey : LookupCreationStage.DragADescription);
        UpdateValidityAssesment();
    }

    private void pk1_SelectedColumnChanged()
    {
        SetStage(pk1.SelectedColumn == null
            ? LookupCreationStage.DragAPrimaryKey
            : LookupCreationStage.DragAForeignKey);
        UpdateValidityAssesment();
    }

    public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _catalogue = databaseObject;

        cbxLookup.SetItemActivator(activator);
        olvLookupNameColumn.ImageGetter = o => activator.CoreIconProvider.GetImage(o).ImageToBitmap();
        olvExtractionInformationsNameColumn.ImageGetter = o => activator.CoreIconProvider.GetImage(o).ImageToBitmap();
        olvDescriptionsColumn.ImageGetter = o => activator.CoreIconProvider.GetImage(o).ImageToBitmap();

        //add the currently configured extraction informations in the order they appear in the dataset
        var allExtractionInformationFromCatalogue =
            new List<ExtractionInformation>(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
        allExtractionInformationFromCatalogue.Sort();

        olvExtractionInformations.ClearObjects();
        olvExtractionInformations.AddObjects(allExtractionInformationFromCatalogue.ToArray());

        btnImportNewTableInfo.Image = activator.CoreIconProvider.GetImage(RDMPConcept.TableInfo, OverlayKind.Import)
            .ImageToBitmap();
        toolTip.SetToolTip(btnImportNewTableInfo, "Import new...");

        btnPrimaryKeyCompositeHelp.Image = FamFamFamIcons.help.ImageToBitmap();

        pictureBox1.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue).ImageToBitmap();
        tbCatalogue.Text = databaseObject.ToString();

        cbxLookup.SetUp(activator.CoreChildProvider.AllTableInfos.Value);

        UpdateValidityAssesment();
    }

    public void SetLookupTableInfo(TableInfo t, bool setComboBox = true)
    {
        if (t is { IsTableValuedFunction: true })
        {
            WideMessageBox.Show("Lookup table not valid",
                $"Table '{t}' is a TableValuedFunction, you cannot use it as a lookup table");
            return;
        }

        if (setComboBox)
            cbxLookup.SelectedItem = t;

        olvLookupColumns.ClearObjects();

        if (t != null)
        {
            olvLookupColumns.AddObjects(t.ColumnInfos);

            SetStage(LookupCreationStage.DragAPrimaryKey);

            pk1.IsValidGetter = c => c.TableInfo_ID == t.ID;
            pk2.IsValidGetter = c => c.TableInfo_ID == t.ID;
            pk3.IsValidGetter = c => c.TableInfo_ID == t.ID;

            fk1.IsValidGetter = c => c.TableInfo_ID != t.ID;
            fk2.IsValidGetter = c => c.TableInfo_ID != t.ID;
            fk3.IsValidGetter = c => c.TableInfo_ID != t.ID;
        }
        else
        {
            SetStage(LookupCreationStage.ChooseLookupTable);
        }
    }

    private void btnImportNewTableInfo_Click(object sender, EventArgs e)
    {
        var importDialog = new ImportSQLTableUI(Activator, false);

        if (importDialog.ShowDialog() == DialogResult.OK)
            if (importDialog.TableInfoCreatedIfAny != null)
                cbxLookup.SelectedItem = importDialog.TableInfoCreatedIfAny;
    }

    private void SetStage(LookupCreationStage newStage)
    {
        _currentStage = newStage;
        Invalidate(true);
    }

    private enum LookupCreationStage
    {
        ChooseLookupTable,
        DragAPrimaryKey,
        DragADescription,
        DragAForeignKey
    }

    private LookupCreationStage _currentStage = LookupCreationStage.ChooseLookupTable;

    private void LookupConfiguration_Paint(object sender, PaintEventArgs e)
    {
        var drawTaskListAt = new Point(580, 10);


        var lines = new[]
        {
            "Defining a lookup relationship:",
            "  1. Choose Lookup Table",
            "  2. Choose the Code column (e.g. T/F)",
            "  3. Choose the dataset column containing a matching code (T/F)",
            "  4. Choose the Description column (e.g. Tayside,Fife)"
        };


        var lineHeight = e.Graphics.MeasureString(lines[0], Font).Height;

        for (var i = 0; i < lines.Length; i++)
            e.Graphics.DrawString(lines[i], Font, Brushes.Black,
                new PointF(drawTaskListAt.X, drawTaskListAt.Y + lineHeight * i));

        var bulletLineIndex = _currentStage switch
        {
            LookupCreationStage.ChooseLookupTable => 1,
            LookupCreationStage.DragAPrimaryKey => 2,
            LookupCreationStage.DragAForeignKey => 3,
            LookupCreationStage.DragADescription => 4,
            _ => throw new InvalidOperationException()
        };

        DrawArrows(e.Graphics);

        var triangleBasePoints = new[]
        {
            //basically (where 1 is the line height)
            //0,0
            //.5.5
            //0,1
            //offset by the drawing start location + the appropriate line number

            new PointF(drawTaskListAt.X, drawTaskListAt.Y + bulletLineIndex * lineHeight),
            new PointF(drawTaskListAt.X + lineHeight / 2,
                drawTaskListAt.Y + bulletLineIndex * lineHeight + lineHeight / 2),
            new PointF(drawTaskListAt.X, drawTaskListAt.Y + lineHeight + bulletLineIndex * lineHeight)
        };

        e.Graphics.FillPolygon(Brushes.Black, triangleBasePoints);
    }

    private void DrawArrows(Graphics graphics)
    {
        var arrowPen = new Pen(Color.DarkGray, 2);

        var capPath = new GraphicsPath();

        // Create the outline for our custom end cap.
        capPath.AddLine(new Point(0, 0), new Point(2, -2));
        capPath.AddLine(new Point(2, -2), new Point(0, 0));
        capPath.AddLine(new Point(0, 0), new Point(-2, -2));
        capPath.AddLine(new Point(-2, -2), new Point(0, 0));

        arrowPen.CustomEndCap = new CustomLineCap(null, capPath);


        switch (_currentStage)
        {
            case LookupCreationStage.ChooseLookupTable:
                break;
            case LookupCreationStage.DragAPrimaryKey:

                DrawCurveWithLabel(
                    new PointF(groupBox1.Right + 10, groupBox1.Top + groupBox1.Height / 2f),
                    new PointF(pk1.Left - 10, pk1.Top - 2),
                    "2. Drag Primary Key Column", graphics, arrowPen);
                break;
            case LookupCreationStage.DragAForeignKey:

                DrawCurveWithLabel(
                    new PointF(olvExtractionInformations.Right + 10,
                        olvExtractionInformations.Bottom - olvExtractionInformations.Height / 10f),
                    new PointF(olvSelectedDescriptionColumns.Right + 100, olvSelectedDescriptionColumns.Bottom + 200),
                    new PointF(fk1.Right + 500, fk1.Top + 100),
                    new PointF(fk1.Right + 15, fk1.Bottom - 10),
                    "3. Drag Matching Foreign Key Column", graphics, arrowPen);
                break;
            case LookupCreationStage.DragADescription:
                DrawCurveWithLabel(
                    new PointF(groupBox1.Right + 10, groupBox1.Top + groupBox1.Height / 2f),
                    new PointF(olvSelectedDescriptionColumns.Left - 10, olvSelectedDescriptionColumns.Top - 2),
                    "4. Drag a Description Column", graphics, arrowPen);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawCurveWithLabel(PointF start, PointF end, string label, Graphics g, Pen p)
    {
        var w = end.X - start.X;
        var h = end.Y - start.Y;

        DrawCurveWithLabel(start, start with { X = start.X + w }, start with { Y = start.Y + h }, end, label, g, p);
    }

    private bool debugPoints = false;

    private void DrawCurveWithLabel(PointF start, PointF mid1, PointF mid2, PointF end, string label, Graphics g, Pen p)
    {
        g.DrawBezier(p,
            start,
            mid1,
            mid2,
            end);

        if (debugPoints)
        {
            g.FillEllipse(Brushes.Red, start.X - 2, start.Y - 2, 5, 5);
            g.FillEllipse(Brushes.Red, mid1.X - 2, mid1.Y - 2, 5, 5);
            g.FillEllipse(Brushes.Red, mid2.X - 2, mid2.Y - 2, 5, 5);
            g.FillEllipse(Brushes.Red, end.X - 2, end.Y - 2, 5, 5);
        }

        g.DrawString(label, Font, Brushes.Black, new PointF(start.X, start.Y));
    }


    private void btnPrimaryKeyCompositeHelp_Click(object sender, EventArgs e)
    {
        WideMessageBox.Show("Lookup help",
            @"Usually you only need one primary key/foreign key relationship e.g. M=Male, F=Female in which z_GenderLookup..Sex is the primary key and Demography..PatientGender is the foreign key.  However sometimes you need additional lookup joins.

For example:
if the Drug Code 'TIB' is reused in Tayside and Fife healthboard with different meanings then the primary key/foreign key would of the Lookup table would have to be both the 'Code' (TIB) and the 'Prescribing Healthboard' (T or F).

Only define secondary columns if you really need them! if any of the key fields do not match between the Lookup table and the Dataset table then no lookup description will be recorded",
            null, true, null, WideMessageBoxTheme.Help);
    }

    private void olvLookupColumns_CellRightClick(object sender, CellRightClickEventArgs e)
    {
        if (e.Model is not ColumnInfo c)
            return;

        e.MenuStrip = new ColumnInfoMenu(new RDMPContextMenuStripArgs(Activator), c);
    }

    private void olvSelectedDescriptionColumns_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            olvSelectedDescriptionColumns.RemoveObject(olvSelectedDescriptionColumns.SelectedObject);
            UpdateValidityAssesment();
        }
    }

    private void olvSelectedDescriptionColumns_ModelDropped(object sender, ModelDropEventArgs e)
    {
        olvSelectedDescriptionColumns.AddObject(e.SourceModels[0]);

        UpdateValidityAssesment();
    }

    private void olvSelectedDescriptionColumns_ModelCanDrop(object sender, ModelDropEventArgs e)
    {
        if (e.SourceModels.Count == 1)
            if (e.SourceModels[0] is ColumnInfo)
            {
                var c = e.SourceModels[0] as ColumnInfo;

                //it's already in it
                if (olvSelectedDescriptionColumns.IndexOf(c) != -1)
                {
                    e.InfoMessage = "ColumnInfo is already selected as a Description";
                    return;
                }

                e.Effect = DragDropEffects.Copy;
            }
    }

    private void btnCreateLookup_Click(object sender, EventArgs e)
    {
        UpdateValidityAssesment(true);
    }

    private void UpdateValidityAssesment(bool actuallyDoIt)
    {
        btnCreateLookup.Enabled = false;
        ragSmiley1.Reset();

        try
        {
            if (pk1.SelectedColumn == null)
                throw new Exception("No Primary key column selected");

            if (fk1.SelectedColumn == null)
                throw new Exception("No Foreign key column selected");

            var allExtractionInformations = olvExtractionInformations.Objects.Cast<ExtractionInformation>().ToArray();
            var foreignKeyExtractionInformation =
                allExtractionInformations.SingleOrDefault(e =>
                    e.ColumnInfo != null && e.ColumnInfo.Equals(fk1.SelectedColumn)) ??
                throw new Exception("Foreign key column(s) must come from the Catalogue ExtractionInformation columns");
            if (pk2.SelectedColumn == null != (fk2.SelectedColumn == null))
                throw new Exception("If you want to have secondary joins you must have them in pairs");

            if (pk3.SelectedColumn == null != (fk3.SelectedColumn == null))
                throw new Exception("If you want to have secondary joins you must have them in pairs");

            var p1 = pk1.SelectedColumn;
            var f1 = fk1.SelectedColumn;

            var p2 = pk2.SelectedColumn;
            var f2 = fk2.SelectedColumn;

            var p3 = pk3.SelectedColumn;
            var f3 = fk3.SelectedColumn;

            var uniqueIDs = new[] { p1, p2, p3, f1, f2, f3 }.Where(o => o != null).Select(c => c.ID).ToArray();

            if (uniqueIDs.Distinct().Count() != uniqueIDs.Length)
                throw new Exception("Columns can only appear once in any given key box");

            if (new[] { p1, p2, p3 }.Where(o => o != null).Select(c => c.TableInfo_ID).Distinct().Count() != 1)
                throw new Exception("All primary key columns must come from the same Lookup table");

            var descs = olvSelectedDescriptionColumns.Objects.Cast<ColumnInfo>().ToArray();

            if (!descs.Any())
                throw new Exception("You must have at least one Description column from the Lookup table");

            if (descs.Any(d => d.TableInfo_ID != p1.TableInfo_ID))
                throw new Exception("All Description columns must come from the Lookup table");

            if (actuallyDoIt)
            {
                var alsoCreateExtractionInformation =
                    Activator.YesNo(
                        $"Also create a virtual extractable column(s) in '{_catalogue}' called '<Column>_Desc'",
                        "Create Extractable Column?");

                var keyPairs = new List<Tuple<ColumnInfo, ColumnInfo>> { Tuple.Create(f1,p1) };

                if (p2 != null)
                    keyPairs.Add(Tuple.Create(f2, p2));

                if (p3 != null)
                    keyPairs.Add(Tuple.Create(f3, p3));

                var cmd = new ExecuteCommandCreateLookup(Activator.RepositoryLocator.CatalogueRepository,
                    foreignKeyExtractionInformation, descs,
                    keyPairs, tbCollation.Text, alsoCreateExtractionInformation);

                cmd.Execute();

                Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_catalogue));
                SetDatabaseObject(Activator, _catalogue);

                MessageBox.Show("Lookup created successfully, fields will now be cleared");
                pk1.Clear();
                pk2.Clear();
                pk3.Clear();

                fk1.Clear();
                fk2.Clear();
                fk3.Clear();

                olvSelectedDescriptionColumns.ClearObjects();
                SetStage(LookupCreationStage.DragAPrimaryKey);
            }

            btnCreateLookup.Enabled = true;
        }
        catch (Exception e)
        {
            if (actuallyDoIt)
                ExceptionViewer.Show(e);

            ragSmiley1.Fatal(e);
        }
    }

    public override string GetTabName() => $"Add Lookup ({base.GetTabName()})";

    private void tbFilter_TextChanged(object sender, EventArgs e)
    {
        olvExtractionInformations.UseFiltering = true;
        olvExtractionInformations.ModelFilter = new TextMatchFilter(olvExtractionInformations, tbFilter.Text);
    }

    private void olv_ItemActivate(object sender, EventArgs e)
    {
        var olv = (ObjectListView)sender;

        if (olv.SelectedObject is IMapsDirectlyToDatabaseTable o)
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(o));
    }

    private void cbxLookup_SelectedItemChanged(object sender, EventArgs e)
    {
        SetLookupTableInfo((TableInfo)cbxLookup.SelectedItem, false);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LookupConfiguration_Design, UserControl>))]
public abstract class LookupConfiguration_Design : RDMPSingleDatabaseObjectControl<Catalogue>
{
}