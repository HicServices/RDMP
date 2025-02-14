// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.MainFormUITabs;

/// <summary>
/// Each dataset (Catalogue) includes one or more virtual columns called CatalogueItems.  Each CatalogueItem is powered by an underlying columns in your data repository but there
/// can be multiple CatalogueItems per column (for example if the DateOfBirth column is extractable either rounded to the nearest quarter or verbatim).  Thus CatalogueItems are both
/// an extraction transform/rule set (See ExtractionInformationUI) and a descriptive entity which describes what the researcher will receive if they are given the column in an extract.
/// This helpfully also lets you delete/restructure your data tables underneath without losing the descriptive data, validation rules, logging history etc of your datasets.
/// 
/// <para>This control lets you view/edit the descriptive metadata of a CatalogueItem in a dataset (Catalogue).</para>
/// </summary>
public partial class CatalogueItemUI : CatalogueItemUI_Design, ISaveableUI
{
    internal Scintilla _scintillaDescription;
    private CatalogueItem _catalogueItem;

    public CatalogueItemUI()
    {
        InitializeComponent();
        ObjectSaverButton1.BeforeSave += objectSaverButton1_BeforeSave;
        AssociatedCollection = RDMPCollection.Catalogue;

        ci_ddPeriodicity.DataSource = Enum.GetValues(typeof(Catalogue.CataloguePeriodicity));

    }

    private bool objectSaverButton1_BeforeSave(DatabaseEntity databaseEntity)
    {
        //see if we need to display the dialog that lets the user sync up descriptions of multiuse columns e.g. CHI
        var propagate =
            new PropagateCatalogueItemChangesToSimilarNamedUI(Activator, _catalogueItem,
                out var shouldDialogBeDisplayed);

        //there are other CatalogueItems that share the same name as this one so give the user the option to propagate his changes to those too
        if (shouldDialogBeDisplayed)
            if (Activator.ShowDialog(propagate) == DialogResult.Cancel)
                return false;

        return true;
    }

    public override void SetDatabaseObject(IActivateItems activator, CatalogueItem databaseObject)
    {
        _catalogueItem = databaseObject;
        if (_catalogueItem != null)
        {
            var columnInfoDatasetValue  = _catalogueItem?.ColumnInfo?.Dataset_ID;
            if (columnInfoDatasetValue != null)
            {
                lbDatasetValue.Visible = true;
                lbDataset.Visible = true;
                var dataset = _catalogueItem.CatalogueRepository.GetAllObjects<Dataset>()
                    .FirstOrDefault(ds => ds.ID == columnInfoDatasetValue);
                if (dataset != null)
                {
                    lbDatasetValue.Text = dataset.Name;
                }
            }
            else
            {
                lbDatasetValue.Visible = false;
                lbDataset.Visible = false;
            }
        }

        if (_scintillaDescription == null)
        {
            var f = new ScintillaTextEditorFactory();
            _scintillaDescription = f.Create(null, SyntaxLanguage.None, null, true, false, activator.CurrentDirectory);
            _scintillaDescription.Font = System.Drawing.SystemFonts.DefaultFont;
            _scintillaDescription.WrapMode = WrapMode.Word;
            panel1.Controls.Add(_scintillaDescription);
        }

        base.SetDatabaseObject(activator, databaseObject);


        if (_catalogueItem?.ExtractionInformation == null)
            CommonFunctionality.AddToMenu(new ExecuteCommandMakeCatalogueItemExtractable(activator, _catalogueItem),
                "Make Extractable");
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, CatalogueItem databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(ci_tbID, "Text", "ID", ci => ci.ID);
        Bind(ci_tbName, "Text", "Name", ci => ci.Name);
        Bind(ci_tbStatisticalConsiderations, "Text", "Statistical_cons", ci => ci.Statistical_cons);
        Bind(ci_tbResearchRelevance, "Text", "Research_relevance", ci => ci.Research_relevance);
        Bind(_scintillaDescription, "Text", "Description", ci => ci.Description);
        Bind(ci_tbTopics, "Text", "Topic", ci => ci.Topic);
        Bind(ci_ddPeriodicity, "SelectedItem", "Periodicity", ci => ci.Periodicity);
        Bind(ci_tbAggregationMethod, "Text", "Agg_method", ci => ci.Agg_method);
        Bind(ci_tbLimitations, "Text", "Limitations", ci => ci.Limitations);
        Bind(ci_tbComments, "Text", "Comments", ci => ci.Comments);
    }

    public override string GetTabName() => $"{base.GetTabName()} ({_catalogueItem.Catalogue.Name})";

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }

    private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
    {

    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CatalogueItemUI_Design, UserControl>))]
public abstract class CatalogueItemUI_Design : RDMPSingleDatabaseObjectControl<CatalogueItem>;