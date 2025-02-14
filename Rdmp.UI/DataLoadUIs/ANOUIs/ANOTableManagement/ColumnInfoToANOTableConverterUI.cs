// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.Core.DataLoad.Engine.Pipeline.Components.Anonymisation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs.SqlDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs.ANOUIs.ANOTableManagement;

/// <summary>
/// Converts the contents of an existing column of your live data into anonymous identifiers.  You should only use this after backing up your database first and being very certain
/// that you do not need the sensitive data being anonymised in any project extracts.
/// 
/// <para>BACKGROUND:
/// The process of anonymisation is referred to as ANO and involves moving existing identifiers into an ANOStore (separate database) and substituting in their place unique anonymous
/// identifiers (there is a 1 to 1 mapping between ANO identifiers and the original values).  Each type of data (e.g. GP Code, Practice Code etc) should have its own ANOTable with
/// a unique suffix such that you can more easily trace down identifiers if you ever have to deanonymise data.  </para>
/// 
/// <para>For example if you imagine that all GP codes must be anonymised, in your data they appear as a healthboard (T - Tayside, F - Fife) followed by 3 digits.  Then your ANOTable would
/// contain the (Deleted) original values e.g. 'T402' and the substituted (ANO) identifier '3622_G'.  ANO identifiers are always a sequence of random integers and letters (you can choose
/// how many letters and how many characters) followed by a suffix (in this case _G to indicate that it is a GP Code).  After Finalising the configuration, your live data table would go from
/// varchar(4) to varchar(6) - to accommodate the suffix and longer number of maximum digits and all codes would be replaced with ANO codes.  This lets your data users still match across
/// GPs (e.g. to identify prescribing patterns in GPs) without knowing which GP is which (which would be the case with a GP Code which can be looked up on any clinical system).</para>
/// 
/// <para>All columns that share an ANOTable (e.g. ANOGPCode) must have the same datatype (in above example this would be varchar(4)).</para>
/// 
/// <para>USING WINDOW:
/// To use this window you must be sure that you want to transform identifiable data into anonymous format.  It is advisable to never anonymise useful result data e.g. numberOfPrescriptions) and
/// stick to anonymising only categorical fields that compromise patient or carer anonymity (GP Codes, Patient identifiers etc).  Also if you never intend to process or even host certain columns
/// (e.g. Firstname / Surname) then you can drop the fields entirely as part of data loading through the PreLoadDiscardedColumn mechanism).</para>
/// 
/// <para>If the data in your column already conforms to a known type that you have anonymised before (e.g. 'GP Code' in another dataset) and the datatype matches exactly (e.g. varchar(4)) then you
/// can select an existing ANOTable and push the data straight through into ANO format.</para>
/// 
/// <para>If not then you will need to type in a name (beginning with ANO) that refers to the type (e.g. ANOPatientIdentifier) and give it a meaningful suffix (e.g. 'P' for patient) and select
/// Create ANOTable.  Adjust the Integer/Character count till the preview data looks pleasing and no errors are reported then Finalise the choice.</para>
/// 
/// <para></para>
/// </summary>
public partial class ColumnInfoToANOTableConverterUI : ColumnInfoToANOTableConverterUI_Design
{
    private ColumnInfo _columnInfo;
    private bool _yesToAll;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ColumnInfo ColumnInfo
    {
        get => _columnInfo;
        private set
        {
            _columnInfo = value;

            lblName.Text = value.ToString();
            lblDataType.Text = value.Data_type;
        }
    }

    private ANOTable ANOTable
    {
        get => _anoTable;
        set
        {
            _anoTable = value;

            if (value == null)
                return;

            tbSuffix.Text = value.Suffix;
            numericUpDown1.Value = value.NumberOfIntegersToUseInAnonymousRepresentation;
            numericUpDown2.Value = value.NumberOfCharactersToUseInAnonymousRepresentation;

            if (value.IsTablePushed())
            {
                gbConfigureANOTable.Enabled = false;
                btnFinalise.Enabled = true;
            }
        }
    }

    //constructor
    public ColumnInfoToANOTableConverterUI()
    {
        InitializeComponent();

        AssociatedCollection = RDMPCollection.Catalogue;

        dgPreview.ColumnAdded += (s, e) => e.Column.FillWeight = 1;
    }

    public override void SetDatabaseObject(IActivateItems activator, ColumnInfo databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        ColumnInfo = databaseObject;

        //make sure we can connect to the server
        if (!ColumnInfo.TableInfo.DiscoverExistence(DataAccessContext.DataLoad, out var reason))
        {
            activator.KillForm(ParentForm, reason);
            return;
        }


        try
        {
            try
            {
                lblRowcount.Text = ColumnInfo.TableInfo.Discover(DataAccessContext.DataLoad).GetRowCount().ToString();
            }
            catch (Exception e)
            {
                checksUI1.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Could not get rowcount of table {ColumnInfo.TableInfo.GetRuntimeName()} using data access context DataLoad",
                        CheckResult.Fail, e));
            }


            GeneratePreviews();

            RefreshServers();
        }
        catch (Exception e)
        {
            activator.KillForm(ParentForm, e);
        }
    }

    private void RefreshServers()
    {
        if (ColumnInfo == null)
            return;

        ddExternalDatabaseServer.Items.Clear();

        ddExternalDatabaseServer.Items.AddRange(Activator.RepositoryLocator.CatalogueRepository
            .GetAllDatabases<ANOStorePatcher>());

        var defaultServer = Activator.ServerDefaults.GetDefaultFor(PermissableDefaults.ANOStore);

        if (defaultServer != null)
            ddExternalDatabaseServer.SelectedItem = defaultServer;
        else
            lblNoDefaultANOStore.Visible = true;

        ddANOTables.Items.Clear();
        ddANOTables.Items.AddRange(Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ANOTable>().ToArray());
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
        ANOTable.NumberOfIntegersToUseInAnonymousRepresentation = Convert.ToInt32(numericUpDown1.Value);

        GeneratePreviews();
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
        ANOTable.NumberOfCharactersToUseInAnonymousRepresentation = Convert.ToInt32(numericUpDown2.Value);

        GeneratePreviews();
    }

    private DataTable preview;
    private ANOTable _anoTable;

    private void GeneratePreviews()
    {
        if (preview == null)
        {
            preview = new DataTable();
            preview.BeginLoadData();
            preview.Columns.Add(_columnInfo.GetRuntimeName(LoadStage.PostLoad));
            preview.Columns.Add(ANOTable.ANOPrefix + _columnInfo.GetRuntimeName(LoadStage.PostLoad));

            var server = DataAccessPortal.ExpectServer(ColumnInfo.TableInfo, DataAccessContext.DataLoad);

            using var con = server.GetConnection();
            con.Open();

            lblPreviewDataIsFictional.Visible = false;

            var qb = new QueryBuilder(null, null, new[] { ColumnInfo.TableInfo });
            qb.AddColumn(new ColumnInfoToIColumn(new MemoryRepository(), _columnInfo));
            qb.TopX = 10;

            var rowsRead = false;

            using (var cmd = server.GetCommand(qb.SQL, con))
            {
                cmd.CommandTimeout = Convert.ToInt32(ntimeout.Value);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    preview.Rows.Add(r[_columnInfo.GetRuntimeName(LoadStage.PostLoad)], DBNull.Value);
                    rowsRead = true;
                }
            }

            if (!rowsRead)
            {
                lblPreviewDataIsFictional.Visible = true;
                if (_columnInfo.GetRuntimeDataType(LoadStage.AdjustRaw).ToLower().Contains("char"))
                {
                    preview.Rows.Add("?", DBNull.Value);
                    preview.Rows.Add("?", DBNull.Value);
                    preview.Rows.Add("?", DBNull.Value);
                    preview.Rows.Add("?", DBNull.Value);
                }
                else if (_columnInfo.GetRuntimeDataType(LoadStage.AdjustRaw).ToLower().Contains("date"))
                {
                    preview.Rows.Add("1977-08-16", DBNull.Value);
                    preview.Rows.Add("1977-08-16", DBNull.Value);
                    preview.Rows.Add("1977-08-16", DBNull.Value);
                    preview.Rows.Add("1977-08-16", DBNull.Value);
                }
                else
                {
                    preview.Rows.Add("-1", DBNull.Value);
                    preview.Rows.Add("-1", DBNull.Value);
                    preview.Rows.Add("-1", DBNull.Value);
                    preview.Rows.Add("-1", DBNull.Value);
                }
            }

            con.Close();
        }

        if (ANOTable != null)
            try
            {
                if (preview.Rows.Count != 0)
                {
                    checksUI1.Clear();
                    var transformer =
                        new ANOTransformer(ANOTable, new FromCheckNotifierToDataLoadEventListener(checksUI1));
                    transformer.Transform(preview, preview.Columns[0], preview.Columns[1], true);
                }
            }
            catch (Exception e)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
            }

        preview.EndLoadData();
        dgPreview.DataSource = preview;
    }

    private void btnCreateNewANOTable_Click(object sender, EventArgs e)
    {
        try
        {
            var server = ddExternalDatabaseServer.SelectedItem as ExternalDatabaseServer;

            var a = new ANOTable(Activator.RepositoryLocator.CatalogueRepository, server, tbANOTableName.Text,
                tbSuffix.Text);

            //if we know the type is e.g. varchar(5)
            var length = ColumnInfo.Discover(DataAccessContext.InternalDataProcessing).DataType.GetLengthIfString();

            if (length > 0)
            {
                a.NumberOfIntegersToUseInAnonymousRepresentation = 0;
                a.NumberOfCharactersToUseInAnonymousRepresentation = length; //give it a sensible maximal that will work
                a.SaveToDatabase();
            }

            ANOTable = a; //and set the property to it to populate the rest of the form

            gbANOTable.Enabled = true;
            gbSelectExistingANOTable.Enabled = false;
            gbCreateNewANOTable.Enabled = false;
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs("Could not create ANOTable", CheckResult.Fail, exception));
        }
    }

    private void ddExternalDatabaseServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddExternalDatabaseServer.SelectedItem is not ExternalDatabaseServer server)
            return;

        ANOTransformer.ConfirmDependencies(DataAccessPortal.ExpectDatabase(server, DataAccessContext.DataLoad),
            checksUI1);
    }

    private void ddANOTables_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddANOTables.SelectedItem == null)
            return;

        //get ANOTable input datatype
        var anoTable = (ANOTable)ddANOTables.SelectedItem;

        //if table is already on the ANO server
        if (anoTable.IsTablePushed())
        {
            var anoDatatype = anoTable.GetRuntimeDataType(LoadStage.AdjustRaw);
            var colDatatype = _columnInfo.GetRuntimeDataType(LoadStage.PostLoad);

            if (!anoDatatype.Equals(colDatatype))
            {
                checksUI1.OnCheckPerformed(
                    new CheckEventArgs(
                        $"ANOTable  {anoTable} cannot be used because its input datatype is {anoDatatype} but the data in {_columnInfo} is of datatype {colDatatype}",
                        CheckResult.Fail));
                ddANOTables.SelectedItem = null;
                return;
            }
        }

        //either it is not pushed or it is pushed and the datatypes are compatible.
        ANOTable = anoTable;

        gbANOTable.Enabled = true;
        gbSelectExistingANOTable.Enabled = false;
        gbCreateNewANOTable.Enabled = false;
    }


    private void btnFinalise_Click(object sender, EventArgs e)
    {
        //if it is not pushed, push it now
        if (!ANOTable.IsTablePushed())
        {
            ANOTable.PushToANOServerAsNewTable(_columnInfo.Data_type, ThrowImmediatelyCheckNotifier.QuietPicky);
            ANOTable.SaveToDatabase();
        }


        var converter = new ColumnInfoToANOTableConverter(_columnInfo, ANOTable);

        try
        {
            bool worked;
            //there is no data in the column or the data is makey upey data
            if (dgPreview.Rows.Count == 0 || lblPreviewDataIsFictional.Visible)
                worked = converter.ConvertEmptyColumnInfo(UserAcceptSql, checksUI1);
            else
                worked = converter.ConvertFullColumnInfo(UserAcceptSql, checksUI1);

            if (worked)
                if (Activator.YesNo("successfully changed column to ANO, close form?", "Close form?"))
                    ParentForm?.Close();
        }
        catch (Exception exception)
        {
            checksUI1.OnCheckPerformed(new CheckEventArgs(
                "Failed to complete migration, your table is likely to be in a sorry state now - sorry",
                CheckResult.Fail, exception));
        }

        //it worked (or didn't!) so notify changes to the TableInfo
        Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(ColumnInfo.TableInfo));
    }

    private bool UserAcceptSql(string sql)
    {
        if (_yesToAll)
            return true;

        var window = new SQLPreviewWindow("Confirm happiness with SQL",
            "The following SQL is about to be executed:", sql);
        try
        {
            return window.ShowDialog() == DialogResult.OK;
        }
        finally
        {
            _yesToAll = window.YesToAll;
        }
    }

    private void tbANOTableName_TextChanged(object sender, EventArgs e)
    {
        //don't enable the button unless he has typed something
        if (string.IsNullOrWhiteSpace(tbANOTableName.Text))
        {
            lblMustStartWithANO.ForeColor = Color.Black;
            btnCreateNewANOTable.Enabled = false;
            return;
        }

        //Alert user if he is trying to create an ANOTable that does not conform to specifications
        if (!tbANOTableName.Text.StartsWith(ANOTable.ANOPrefix))
        {
            btnCreateNewANOTable.Enabled = false;
            lblMustStartWithANO.ForeColor = Color.Red;
        }
        else
        {
            //hide ANO complaint message
            btnCreateNewANOTable.Enabled = true;
            lblMustStartWithANO.ForeColor = Color.Black;
        }
    }

    private void tbSuffix_TextChanged(object sender, EventArgs e)
    {
        //don't enable the button unless he has typed something
        if (string.IsNullOrWhiteSpace(tbSuffix.Text))
        {
            lblDoNotIncludeUnderscore.ForeColor = Color.Black;
            btnCreateNewANOTable.Enabled = false;
            return;
        }


        if (tbSuffix.Text.StartsWith("_"))
        {
            btnCreateNewANOTable.Enabled = false;
            lblDoNotIncludeUnderscore.ForeColor = Color.Red;
        }
        else
        {
            btnCreateNewANOTable.Enabled = true;
            lblDoNotIncludeUnderscore.ForeColor = Color.Black;
        }
    }

    public override string GetTabName() => $"Convert {base.GetTabName()} to ANOColumnInfo";
}

[TypeDescriptionProvider(
    typeof(AbstractControlDescriptionProvider<ColumnInfoToANOTableConverterUI_Design, UserControl>))]
public abstract class ColumnInfoToANOTableConverterUI_Design : RDMPSingleDatabaseObjectControl<ColumnInfo>;