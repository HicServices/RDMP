// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataViewing;
using Rdmp.UI.AutoComplete;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.ExtractionUIs.FilterUIs;

/// <summary>
/// One major problem with research data curation/provision is that data analysts who routinely work with datasets build up an in-depth knowledge of the data and how to identify
/// interesting subsets (e.g. 'How to identify all lab test codes for Creatinine').  This can involve complicated SQL which can end up buried in undocumented extraction scripts
/// or kept in the head of the data analyst and lost if he ever leaves the organisation.
/// 
/// <para>RDMP Filters are an attempt to reduce this risk by centralising SQL 'WHERE' logic into clearly defined and documented reusable blocks (called Filters).  These named filters can
/// then be combined/used by new data analyst who don't necessarily understand the exact implementation.  For this to work it is vital that you accurately name and describe what each
/// filter does, including any peculiarities and that you robustly test the SQL in the implementation to make sure it actually works.</para>
/// 
/// <para>To write the actual implementation type into the SQL prompt (omitting the 'WHERE' keyword).  For example you could create a Filter called 'Active Records Only' with a description
/// 'This filter throws out all records which have expired because a clinician has deleted them or the patient has withdrawn consent' and implementation SQL of 'MyTable.IActive=1'. Make
/// sure to fully specify the names of columns in your WHERE SQL in case the filter is used as part of a join across multiple tables with columns that contain the same name (e.g. it might
/// be that many other tables also include a field called IsActive).  Make sure you fully explore your dataset before finalising your filter and consider edge cases e.g. what does it mean
/// when IsActive is null? are there any values above 1? and if so what does that mean?</para>
/// 
/// <para>If you want to parameterise your query (e.g. a filter for 'Approved name of drug like X') then just type a parameter like you normally would e.g. 'Prescription.DrugName like @drugName'
/// and save. This will automatically create an empty parameter (See ParameterCollectionUI).</para>
/// </summary>
public partial class ExtractionFilterUI : ExtractionFilterUI_Design, ILifetimeSubscriber, ISaveableUI
{
    private IFilter _extractionFilter;

    private AutoCompleteProviderWin _autoCompleteProvider;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ISqlParameter[] GlobalFilterParameters { get; private set; }

    private Scintilla QueryEditor;
    private bool _loading;

    public ExtractionFilterUI()
    {
        InitializeComponent();

        ObjectSaverButton1.BeforeSave += BeforeSave;
        UseCommitSystem = true;
    }

    private void QueryEditor_TextChanged(object sender, EventArgs e)
    {
        if (_loading)
            return;

        _extractionFilter.WhereSQL = QueryEditor.Text;
    }

    private void FigureOutGlobalsAndAutoComplete()
    {
        var factory = new FilterUIOptionsFactory();
        var options = FilterUIOptionsFactory.Create(_extractionFilter);
        GlobalFilterParameters = options.GetGlobalParametersInFilterScope();

        if (QueryEditor != null) return;

        var querySyntaxHelper = _extractionFilter.GetQuerySyntaxHelper();

        QueryEditor =
            new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory(), SyntaxLanguage.SQL,
                querySyntaxHelper);
        QueryEditor.TextChanged += QueryEditor_TextChanged;
        pQueryEditor.Controls.Add(QueryEditor);
        QueryEditor.Dock = DockStyle.Fill;

        _autoCompleteProvider = new AutoCompleteProviderWin(querySyntaxHelper);

        foreach (var t in options.GetTableInfos())
            _autoCompleteProvider.Add(t);

        foreach (var c in options.GetIColumnsInFilterScope())
            _autoCompleteProvider.Add(c);

        foreach (var parameter in GlobalFilterParameters)
            _autoCompleteProvider.Add(parameter);

        _autoCompleteProvider.RegisterForEvents(QueryEditor);
    }

    private bool BeforeSave(DatabaseEntity databaseEntity)
    {
        SubstituteQueryEditorTextIfContainsLineComments();
        OfferWrappingIfUserIncludesANDOrOR();

        //update SQL
        _extractionFilter.WhereSQL = QueryEditor.Text.TrimEnd();

        var creator = new ParameterCreator(_extractionFilter.GetFilterFactory(), GlobalFilterParameters, null);
        creator.CreateAll(_extractionFilter, null);

        return true;
    }

    private void OfferWrappingIfUserIncludesANDOrOR()
    {
        if (QueryEditor.Text.ToLower().Contains(" and ") || QueryEditor.Text.ToLower().Contains(" or "))
        {
            //user is creating a filter with boolean logic in it! better wrap their function if it isn't already
            if (QueryEditor.Text.Trim().StartsWith("(")) //it already does so no worries
                return;

            MessageBox.Show("Your Filter SQL has an AND / OR in it, so we are going to wrap it in brackets for you",
                "Filter contains AND/OR");

            QueryEditor.Text = $"({QueryEditor.Text})";
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
                "Line comments are not allowed in the filter query, these will be automatically converted to block comments.",
                "Line comments");
            QueryEditor.Text = commentRegex.Replace(QueryEditor.Text, $"/* ${{comment}} */{Environment.NewLine}");
        }
    }


    public override void SetDatabaseObject(IActivateItems activator, ConcreteFilter databaseObject)
    {
        _loading = true;
        base.SetDatabaseObject(activator, databaseObject);
        Catalogue = databaseObject.GetCatalogue();
        _extractionFilter = databaseObject;

        ParameterCollectionUIOptions options;
        try
        {
            var factory = new ParameterCollectionUIOptionsFactory();
            options = factory.Create(databaseObject, activator.CoreChildProvider);
        }
        catch (Exception e)
        {
            Activator.KillForm(ParentForm, e);
            return;
        }

        //collapse panel 1 unless there are parameters
        splitContainer1.Panel1Collapsed =
            !options.ParameterManager.ParametersFoundSoFarInQueryGeneration.Values.Any(v => v.Any());

        parameterCollectionUI1.SetUp(options, Activator);

        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.TOP_100));
        CommonFunctionality.AddToMenu(
            new ExecuteCommandViewFilterMatchData(Activator, databaseObject, ViewType.Aggregate));
        CommonFunctionality.AddToMenu(new ExecuteCommandViewFilterMatchGraph(Activator, databaseObject));
        CommonFunctionality.AddToMenu(new ExecuteCommandBrowseLookup(Activator, databaseObject));
        CommonFunctionality.AddToMenu(new ExecuteCommandPublishFilter(Activator, databaseObject,
            databaseObject.GetCatalogue()));

        FigureOutGlobalsAndAutoComplete();

        QueryEditor.Text = _extractionFilter.WhereSQL;

        CommonFunctionality.AddChecks(databaseObject);
        CommonFunctionality.StartChecking();

        QueryEditor.ReadOnly = ReadOnly;
        tbFilterName.ReadOnly = ReadOnly;
        tbFilterDescription.ReadOnly = ReadOnly;
        cbIsMandatory.Enabled = !ReadOnly;

        _loading = false;
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ConcreteFilter databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbFilterName, "Text", "Name", f => f.Name);
        Bind(tbFilterDescription, "Text", "Description", f => f.Description);
        Bind(cbIsMandatory, "Checked", "IsMandatory", f => f.IsMandatory);
    }

    /// <summary>
    /// Used for publishing IFilters created here back to the main Catalogue
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Catalogue Catalogue { get; set; }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        if (e.Object is not IFilter filter || !filter.Equals(_extractionFilter))
            return;

        if (!filter.Exists()) //it's deleted
            ParentForm?.Close();
        else
            _extractionFilter = filter;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionFilterUI_Design, UserControl>))]
public abstract class ExtractionFilterUI_Design : RDMPSingleDatabaseObjectControl<ConcreteFilter>
{
}