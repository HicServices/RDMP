// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.Validation;

/// <summary>
/// Regular expressions are a great way of validating the content of your datasets.  For example you could have a regex pattern ^[MFU]$ which would force a cells contents to be either
/// M, F or U with nothing else allowed.  Rather than having each data analyst type the same regular expression into the validation rules of each column you can create a StandardRegex.
/// This StandardRegex will then be available as a validation rule for any column (See <see cref="ValidationSetupUI"/>).
/// 
/// <para>Because regular expressions can get pretty complicated both a concept name and a verbose description that explains what the pattern matches and what it won't match.  You can also
/// test your implementation by typing values into the 'Testing Box' and clicking Test.  For example if you typed in 'Male' with the above pattern it would fail validation because it
/// is not either an M or a F or a U.  If your pattern was [MFU] then it would pass because it contains an M! </para>
/// </summary>
public partial class StandardRegexUI : StandardRegexUI_Design, ISaveableUI
{
    private StandardRegex _standardRegex;

    public StandardRegexUI()
    {
        InitializeComponent();
    }

    public override void SetDatabaseObject(IActivateItems activator, StandardRegex databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _standardRegex = databaseObject;

        CommonFunctionality.AddChecks(_standardRegex);
        CommonFunctionality.StartChecking();
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, StandardRegex databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", r => r.ID);
        Bind(tbConceptName, "Text", "ConceptName", r => r.ConceptName);
        Bind(tbRegex, "Text", "Regex", r => r.Regex);
        Bind(tbDescription, "Text", "Description", r => r.Description);
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbTesting.Text))
        {
            lblResultOfTest.Text =
                "The test text box is blank, null values will automatically pass validation (use a NotNull constraint to do null related evaluations)";
            lblResultOfTest.ForeColor = Color.Green;
        }
        else if (Regex.IsMatch(tbTesting.Text, _standardRegex.Regex))
        {
            lblResultOfTest.Text =
                $"The text '{tbTesting.Text}' matches the Regex pattern '{_standardRegex.Regex}' meaning that the value will pass validation and not be flagged as a validation failure";
            lblResultOfTest.ForeColor = Color.Green;
        }
        else
        {
            lblResultOfTest.Text =
                $"The text '{tbTesting.Text}' failed to match Regex pattern '{_standardRegex.Regex}' meaning that the value will fail validation and will be flagged as a validation failure";
            lblResultOfTest.ForeColor = Color.Red;
        }
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<StandardRegexUI_Design, UserControl>))]
public abstract class StandardRegexUI_Design : RDMPSingleDatabaseObjectControl<StandardRegex>;