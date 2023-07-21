// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.Copying;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
///     Any column in a data extraction which is marked with 'Hash On Data Release' (See ExtractionInformationUI) will be
///     wrapped with this SQL string.  Use this to call a scalar valued
///     function which generates hash strings based on the column value and the project number (salt).
///     <para>
///         For example Work.dbo.HicHash({0},{1}) would wrap column names such that the column name appeared in the {0} and
///         the project number appeared in {1}.  For this to work you must have
///         a database Work and a scalar function called HicHash (this is just an example, you can call the function
///         whatever you want and adjust it accordingly).  You don't have to use the
///         salt if you don't want to either, if you don't add a {1} then you won't get a salt argument into your scalar
///         function.
///     </para>
///     <para>
///         This is quite technical if you don't know what a Scalar Function is in SQL then you probably don't want to do
///         hashing and instead you might want to just not extract these columns
///         or configure them with the RDMP ANO system (See ANOTable).
///     </para>
/// </summary>
public partial class ConfigureHashingAlgorithmUI : RDMPForm
{
    public ConfigureHashingAlgorithmUI(IActivateItems activator) : base(activator)
    {
        InitializeComponent();

        if (VisualStudioDesignMode)
            return;

        QueryPreview = new ScintillaTextEditorFactory().Create(new RDMPCombineableFactory());
        QueryPreview.ReadOnly = true;

        panel2.Controls.Add(QueryPreview);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Scintilla QueryPreview { get; set; }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (VisualStudioDesignMode)
            return;

        //get the current hashing algorithm
        var value =
            Activator.RepositoryLocator.DataExportRepository.DataExportPropertyManager.GetValue(DataExportProperty
                .HashingAlgorithmPattern);
        tbHashingAlgorithm.Text = value;
    }

    private void tbHashingAlgorithm_TextChanged(object sender, EventArgs e)
    {
        var pattern = tbHashingAlgorithm.Text;

        try
        {
            QueryPreview.ReadOnly = false;
            QueryPreview.Text = pattern.Replace("{0}", "[TEST]..[ExampleColumn]").Replace("{1}", "123");
            Activator.RepositoryLocator.DataExportRepository.DataExportPropertyManager.SetValue(
                DataExportProperty.HashingAlgorithmPattern, pattern);
        }
        catch (Exception exception)
        {
            QueryPreview.Text = ExceptionHelper.ExceptionToListOfInnerMessages(exception);
        }
        finally
        {
            QueryPreview.ReadOnly = true;
        }
    }

    private void btnReferenceColumn_Click(object sender, EventArgs e)
    {
        tbHashingAlgorithm.Text = $"{tbHashingAlgorithm.Text}{{0}}";
    }

    private void btnReferenceSalt_Click(object sender, EventArgs e)
    {
        tbHashingAlgorithm.Text = $"{tbHashingAlgorithm.Text}{{1}}";
    }
}