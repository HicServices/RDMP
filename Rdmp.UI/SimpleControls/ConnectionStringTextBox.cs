// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FAnsi;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// Text box for entering Sql Server connection strings, includes autocomplete support for keywords (e.g. Database)
/// </summary>
[System.ComponentModel.DesignerCategory("")]
public class ConnectionStringTextBox : TextBox
{
    private DatabaseType _databaseType;
    private List<string> supportedKeywords = new();
    private bool suppressAutocomplete;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public DatabaseType DatabaseType
    {
        get => _databaseType;
        set
        {
            _databaseType = value;
            SetSupportedKeywords();
        }
    }

    private void SetSupportedKeywords()
    {
        supportedKeywords.Clear();
        switch (_databaseType)
        {
            case DatabaseType.MicrosoftSQLServer:

                supportedKeywords.Add("User ID");
                supportedKeywords.Add("TrustServerCertificate");
                supportedKeywords.Add("Pooling");
                supportedKeywords.Add("Password");
                supportedKeywords.Add("Trusted_Connection");
                supportedKeywords.Add("Integrated Security");
                supportedKeywords.Add("Initial Catalog");
                supportedKeywords.Add("Database");
                supportedKeywords.Add("Encrypt");
                supportedKeywords.Add("Data Source");
                supportedKeywords.Add("Server");
                supportedKeywords.Add("Address");
                supportedKeywords.Add("Addr");
                supportedKeywords.Add("Network Address");
                supportedKeywords.Add("Application Name");


                break;
            case DatabaseType.MySql:
                throw new NotImplementedException();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public ConnectionStringTextBox()
    {
        DatabaseType = DatabaseType.MicrosoftSQLServer;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        try
        {
            base.OnTextChanged(e);

            //set text color to black by default
            ForeColor = Color.Black;

            if (suppressAutocomplete)
                return;

            //if user is typing somewhere that is not at the end of the text box
            if (SelectionStart + SelectionLength != TextLength)
                return;

            //the potential autocomplete keywords
            var candidates = new List<string>();
            candidates.AddRange(supportedKeywords.ToArray());

            //get all the keywords they have typed so far
            var keywords = Text.Split(';').ToArray();
            //set text color to black by default
            ForeColor = Color.Black;

            foreach (var keyword in keywords)
            {
                var kvp = keyword.Split('=').ToArray();

                //if it is something user has entered
                if (kvp.Length == 2)
                    if (!supportedKeywords.Any(k => k.ToLower().Equals(kvp[0].ToLower().Trim())))
                        ForeColor = Color.DarkRed; //he entered something unsupported
            }

            //get last thing user is typing
            var lastBitBeingTyped = Text[(Text.LastIndexOf(";", StringComparison.Ordinal) + 1)..];

            if (string.IsNullOrWhiteSpace(lastBitBeingTyped) //user has not typed anything or has just put in a ;
                ||
                lastBitBeingTyped
                    .Contains('=')) //user has typed Password=bobsca <- i.e. he is midway through typing a value not a key
                return;

            //we will suggest Server because user typed se
            var keywordToSuggest = candidates.FirstOrDefault(c => c.ToLower().StartsWith(lastBitBeingTyped.ToLower()));

            if (keywordToSuggest == null) //nothing to suggest, user is typing crazy fool stuff
            {
                ForeColor = Color.DarkRed;
                return;
            }

            //don't suggest the whole thing, just suggest the bit they haven't typed yet
            var bitToSuggest = keywordToSuggest[lastBitBeingTyped.Length..];
            if (string.IsNullOrWhiteSpace(bitToSuggest))
                return;

            //suppress further text changed calls
            suppressAutocomplete = true;
            var whereUserIsCurrently = SelectionStart;

            Text = Text.Insert(whereUserIsCurrently, bitToSuggest);
            SelectionStart = whereUserIsCurrently;
            SelectionLength = TextLength - whereUserIsCurrently;
            suppressAutocomplete = false; //we are done
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled) //someone else suppressed it
            return;

        //if user is doing control c or control v or something
        if (e.Control)
            suppressAutocomplete = true;
        else if (char.IsLetterOrDigit((char)e.KeyCode) || e.KeyCode == Keys.Space)
            suppressAutocomplete = false;
        else
            suppressAutocomplete =
                true; //user is pressing some arrow keys or delete keys or something, don't suggest anything until
    }
}