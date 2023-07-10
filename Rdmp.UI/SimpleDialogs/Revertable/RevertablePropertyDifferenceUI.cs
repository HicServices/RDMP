// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.ScintillaHelper;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ScintillaNET;

namespace Rdmp.UI.SimpleDialogs.Revertable;

/// <summary>
/// Used by OfferChanceToSaveDialog to tell you about a property difference between an RDMP object that is visible in an RDMP application but which has unaccountably become different
/// from the database version (for example because another user has modified the record in the database while we had an older copy of it). See OfferChanceToSaveDialog for full details
/// 
/// </summary>
public partial class RevertablePropertyDifferenceUI : RDMPUserControl
{
    private readonly RevertablePropertyDifference _difference;

    public RevertablePropertyDifferenceUI(RevertablePropertyDifference difference)
    {
        _difference = difference;
        InitializeComponent();

        if (VisualStudioDesignMode) //don't add the QueryEditor if we are in design time (visual studio) because it breaks
            return;

        //For documentation/control previewing
        _difference ??=
            new RevertablePropertyDifference(typeof(Catalogue).GetProperty("Name"), "Biochemistry", "byochemistry");

        CreateScintillaComponents(
            _difference.DatabaseValue != null ? _difference.DatabaseValue.ToString() : "<Null>",
            _difference.LocalValue != null ? _difference.LocalValue.ToString() : "<Null>");

        lblDbProperty.Text = $"{_difference.Property.Name} in Database";
        lblMemoryProperty.Text = $"{_difference.Property.Name} in Memory";
    }

    private Scintilla QueryEditorBefore;
    private Scintilla QueryEditorAfter;


    public void CreateScintillaComponents(string textBefore, string textAfter,
        SyntaxLanguage language = SyntaxLanguage.SQL)
    {
        QueryEditorBefore = new ScintillaTextEditorFactory().Create();
        QueryEditorBefore.Text = textBefore;
        QueryEditorBefore.ReadOnly = true;

        splitContainer1.Panel1.Controls.Add(QueryEditorBefore);

        QueryEditorAfter = new ScintillaTextEditorFactory().Create();
        QueryEditorAfter.Text = textAfter;
        QueryEditorAfter.ReadOnly = true;

        splitContainer1.Panel2.Controls.Add(QueryEditorAfter);

        //compute difference
        textBefore ??= "";
        textAfter ??= "";

        var diff = new Diff();

        var highlighter = new ScintillaLineHighlightingHelper();

        ScintillaLineHighlightingHelper.ClearAll(QueryEditorAfter);
        ScintillaLineHighlightingHelper.ClearAll(QueryEditorBefore);

        foreach (var item in Diff.DiffText(textBefore, textAfter))
        {
            for (var i = item.StartA; i < item.StartA + item.deletedA; i++)
                highlighter.HighlightLine(QueryEditorBefore,i, Color.Pink);

            for (var i = item.StartB; i < item.StartB+item.insertedB; i++)
                highlighter.HighlightLine(QueryEditorAfter, i, Color.LawnGreen);
        }
            
    }

            for (var i = item.StartB; i < item.StartB + item.insertedB; i++)
                ScintillaLineHighlightingHelper.HighlightLine(QueryEditorAfter, i, Color.LawnGreen);
        }
    }
}