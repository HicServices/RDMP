// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.ExtractionUIs;

/// <summary>
/// The RDMP is intended to curate research datasets, including which recording which columns in a given dataset are extractable and the Governance level for those columns (e.g. can
/// anyone get the column or are special approvals required).  This window lets you decide whether a CatalogueItem is extractable, optionally provide a transform (e.g. UPPER() etc),
/// curate filter logic, flag it as the datasets extraction identifier (e.g. PatientId).
/// 
/// <para>Start by deciding whether a given Column is extractable by ticking Yes or No.  Then choose an extraction category, a column will only appear in DataExportManager as extractable if
/// it is Core, Supplemental or SpecialApprovalRequired (Internal and Deprecated columns cannot be extracted).  </para>
/// 
/// <para>You should have a single field across all your datasets which identifies your cohorts (patients) e.g. PatientId.  If this column contains PatientIds then tick 'Is Extraction
/// Identifier', very occasionally you might have multiple columns containing PatientIds e.g. Birth records might have a column for MotherId and a column for BabyId both of which contain
/// PatientIds (if this is the case then just tick both as 'Is Extraction Identifier'.  </para>
/// 
/// <para>You can edit the Extraction Code which is a single line of SELECT SQL.  If you change this to include a function or something else make sure to include an alias
/// (e.g. 'UPPER(MyTable.MyColumn) as MyColumn')</para>
/// 
/// <para>You can also view the Filters that are associated with this column.  These are centrally curated and validated (Make sure to validate your filters!!!) pieces of WHERE logic which
/// can be used in Data Extraction and Cohort Identification with the dataset.  For example the Prescribing.DrugCode column could have 2 filters 'Prescription Painkillers' and
/// 'Diabetes Drugs'.  Filters should be adequately documented with name and description such that a data analyst can use them without necessarily understanding the SQL implementation.
/// For more information on configuring Filters see ExtractionFilterUI.</para>
/// 
/// <para>If you tick the HashOnDataRelease column then the transform/column will be wrapped by the Hashing Algorithm (if any - See ConfigureHashingAlgorithm) when it comes to data extraction.
/// Use this only if you have a hashing system implemented.  Hashing is separate from identifier allocation such as ANO (See ANOTable) in that its done at extraction
/// time in SQL only and the exact implementation is up to you.</para>
///
/// <para></para>
/// </summary>
public partial class ExtractionInformationUI : ExtractionInformationUI_Design, ISaveableUI
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ExtractionInformation ExtractionInformation { get; private set; }

    //Editor that user can type into
    private Scintilla QueryEditor;

    //handles the case when the user renames the SQL e.g. by putting an alias on the column
    private bool _namesMatchedWhenDialogWasLaunched;

    private bool isFirstTimeSetupCalled = true;
    private IQuerySyntaxHelper _querySyntaxHelper = MicrosoftQuerySyntaxHelper.Instance;

    private readonly RAGSmileyToolStrip ragSmiley1;
    private bool _isLoading;

    public ExtractionInformationUI() //For use with SetDatabaseObject
    {
        InitializeComponent();

        if (VisualStudioDesignMode) //stop right here if in designer mode
            return;

        //note that we don't add the Any category
        ddExtractionCategory.DataSource = new object[]
        {
            ExtractionCategory.Core, ExtractionCategory.Supplemental, ExtractionCategory.SpecialApprovalRequired,
            ExtractionCategory.Internal, ExtractionCategory.Deprecated, ExtractionCategory.ProjectSpecific
        };

        ObjectSaverButton1.BeforeSave += BeforeSave;

        AssociatedCollection = RDMPCollection.Catalogue;

        ragSmiley1 = new RAGSmileyToolStrip();

        UseCommitSystem = true;
    }

    private bool BeforeSave(DatabaseEntity arg)
    {
        //alias prefix is ' as ' so make sure user doesn't start a new line with "bobbob\r\nas fish" since this won't be recognised, solve the problem by inserting the original alias
        if (_querySyntaxHelper.AliasPrefix.StartsWith(" ", StringComparison.Ordinal))
        {
            var before = QueryEditor.Text;
            var after = Regex.Replace(before, $"^{_querySyntaxHelper.AliasPrefix.TrimStart()}",
                _querySyntaxHelper.AliasPrefix, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            if (!before.Equals(after))
                QueryEditor.Text = after;
        }

        SubstituteQueryEditorTextIfContainsLineComments();

        return true;
    }


    private void QueryEditorOnTextChanged(object sender, EventArgs eventArgs)
    {
        if (_isLoading)
            return;

        try
        {
            //ensure it's all on one line
            _querySyntaxHelper.SplitLineIntoSelectSQLAndAlias(QueryEditor.Text, out var sql, out var alias);

            ExtractionInformation.SelectSQL = sql;
            ExtractionInformation.Alias = alias;

            ExtractionInformation.Check(ThrowImmediatelyCheckNotifier.Quiet);
            ExtractionInformation.GetRuntimeName();
            ragSmiley1.Reset();
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }
    }

    public override void SetDatabaseObject(IActivateItems activator, ExtractionInformation databaseObject)
    {
        _isLoading = true;
        ExtractionInformation = databaseObject;
        base.SetDatabaseObject(activator, databaseObject);

        Setup(databaseObject);

        ObjectSaverButton1.BeforeSave += objectSaverButton1OnBeforeSave;

        CommonFunctionality.Add(ragSmiley1);

        CommonFunctionality.AddHelp(cbHashOnDataRelease, "IColumn.HashOnDataRelease", "Hash on Data Release");
        CommonFunctionality.AddHelp(cbIsExtractionIdentifier, "IColumn.IsExtractionIdentifier",
            "Is Extraction Identifier");
        CommonFunctionality.AddHelp(cbIsPrimaryKey, "IColumn.IsPrimaryKey", "Is Extraction Primary Key");
        CommonFunctionality.AddHelpString(lblIsTransform, "Transforms Data",
            "When the extraction SQL is different from the column SQL then it is considered to 'transform' the data.  For example 'UPPER([mydb].[mycol]) as mycol'.  Transforms must always have an alias.");

        lblIsTransform.Text = $"Transforms Data: {(ExtractionInformation.IsProperTransform() ? "Yes" : "No")}";

        _isLoading = false;
    }

    private bool objectSaverButton1OnBeforeSave(DatabaseEntity databaseEntity)
    {
        if (_namesMatchedWhenDialogWasLaunched)
        {
            var cataItem = ExtractionInformation.CatalogueItem;

            try
            {
                if (!cataItem.Name.Equals(ExtractionInformation.GetRuntimeName()))
                    //which now has a different name (usually alias)
                    if (
                        Activator.YesNo(
                            $"Rename CatalogueItem {cataItem.Name} to match the new Alias? ({ExtractionInformation.GetRuntimeName()})",
                            "Update CatalogueItem name?"))
                    {
                        cataItem.Name = ExtractionInformation.GetRuntimeName();
                        cataItem.SaveToDatabase();
                    }
            }
            catch (RuntimeNameException)
            {
                // there was a problem working out the runtime name.  Maybe it is missing an alias or whatever
                // so we can't do this rename - no big deal
            }
        }

        return true;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ExtractionInformation databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbId, "Text", "ID", ei => ei.ID);
        Bind(tbDefaultOrder, "Text", "Order", ei => ei.Order);
        Bind(tbAlias, "Text", "Alias", ei => ei.Alias);

        Bind(cbHashOnDataRelease, "Checked", "HashOnDataRelease", ei => ei.HashOnDataRelease);
        Bind(cbIsExtractionIdentifier, "Checked", "IsExtractionIdentifier", ei => ei.IsExtractionIdentifier);
        Bind(cbIsPrimaryKey, "Checked", "IsPrimaryKey", ei => ei.IsPrimaryKey);

        Bind(ddExtractionCategory, "SelectedItem", "ExtractionCategory", ei => ei.ExtractionCategory);
    }

    private void Setup(ExtractionInformation extractionInformation)
    {
        ExtractionInformation = extractionInformation;

        if (isFirstTimeSetupCalled)
        {
            //if the catalogue item has same name as the extraction information (alias)
            if (ExtractionInformation.CatalogueItem.Name.Equals(ExtractionInformation.ToString()))
                _namesMatchedWhenDialogWasLaunched = true;

            _querySyntaxHelper = ExtractionInformation.GetQuerySyntaxHelper();

            QueryEditor = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory(), SyntaxLanguage.SQL,
                _querySyntaxHelper);
            QueryEditor.TextChanged += QueryEditorOnTextChanged;

            var autoComplete = new AutoCompleteProviderWin(_querySyntaxHelper);
            autoComplete.Add(ExtractionInformation.CatalogueItem.Catalogue);

            autoComplete.RegisterForEvents(QueryEditor);
            isFirstTimeSetupCalled = false;
        }

        var colInfo = ExtractionInformation.ColumnInfo;

        //deal with empty values in database (shouldn't be any but could be)
        if (string.IsNullOrWhiteSpace(ExtractionInformation.SelectSQL) && colInfo != null)
        {
            ExtractionInformation.SelectSQL = colInfo.Name.Trim();
            ExtractionInformation.SaveToDatabase();
        }

        QueryEditor.Text = ExtractionInformation.SelectSQL + (!string.IsNullOrWhiteSpace(ExtractionInformation.Alias)
            ? _querySyntaxHelper.AliasPrefix + ExtractionInformation.Alias
            : "");


        lblFromTable.Text = colInfo == null ? "MISSING ColumnInfo" : colInfo.TableInfo.Name;


        if (!pSql.Controls.Contains(QueryEditor))
            pSql.Controls.Add(QueryEditor);
    }

    private void cbHashOnDataRelease_CheckedChanged(object sender, EventArgs e)
    {
        //Create alias if it doesn't have one yet
        if (string.IsNullOrWhiteSpace(ExtractionInformation.Alias) && cbHashOnDataRelease.Checked)
        {
            ExtractionInformation.HashOnDataRelease = true;
            ExtractionInformation.Alias = ExtractionInformation.GetRuntimeName();
            Setup(ExtractionInformation);
        }
    }

    /// <summary>
    /// Scans the query text for line comments and replaces any with block comments so the query will still work when flattened to a single line
    /// </summary>
    private void SubstituteQueryEditorTextIfContainsLineComments()
    {
        // regex:
        // \s* = don't capture whitespace before or after the comment so we can consistently add a single space front and back for the block comment
        // .*? = lazy capture of comment text, so we don't eat repeated whitespace at the end of the comment (matched by the second \s* outside the capture group)
        var commentRegex = new Regex($@"--\s*(?<comment>.*?)\s*{Environment.NewLine}");
        if (commentRegex.Matches(QueryEditor.Text).Count > 0)
        {
            MessageBox.Show(
                "Line comments are not allowed in the query, these will be automatically converted to block comments.",
                "Line comments");
            QueryEditor.Text = commentRegex.Replace(QueryEditor.Text, $"/* ${{comment}} */{Environment.NewLine}");
        }
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        e.Cancel = false;

        if (ExtractionInformation == null)
            return;

        if (string.IsNullOrWhiteSpace(ExtractionInformation.Alias) && ExtractionInformation.HashOnDataRelease)
        {
            MessageBox.Show(
                "You must put in an Alias ( AS XYZ ) at the end of your query if you want to hash it on extraction (to a researcher)");
            e.Cancel = true;
        }
    }

    public override string GetTabName()
    {
        if (ExtractionInformation == null)
            return "Loading...";

        string name;

        try
        {
            name = ExtractionInformation.GetRuntimeName();
        }
        catch (RuntimeNameException)
        {
            name = "unknown";
        }

        return $"{name} ({ExtractionInformation.CatalogueItem.Catalogue.Name} Extraction Logic)";
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionInformationUI_Design, UserControl>))]
public abstract class ExtractionInformationUI_Design : RDMPSingleDatabaseObjectControl<ExtractionInformation>
{
}