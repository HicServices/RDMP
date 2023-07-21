// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.UI.SimpleDialogs.Revertable;

/// <summary>
/// The RDMP uses a database (Catalogue Manager Database / Data Export Manager Database) to store all information about your datasets (Technical, descriptive, validation, attachments
/// etc).  Because the system is designed to run with multiple users accessing this database the same time (or by you having multiple applications running at once - like running
/// CatalogueManager and Data Export Manager at the same time) it is possible that two users/applications will attempt to modify the same record at the same time.
/// 
/// <para>This dialog is shown any time the software is confused about which version of a database object is correct (the one it has in memory or the one it finds in the database).  The
/// form will show you every property which has been changed and you must pick which is the correct version of the record.  Once you have selected the correct one the software will
/// update the database (if the one in memory is favoured) or discard its memory copy for a new database copy (if the database copy is preserved).</para>
/// 
/// <para>If the above sounded too complicated, just look at the values of the properties and press the button for which is correct.</para>
/// </summary>
public partial class OfferChanceToSaveDialogUI : Form
{
    private readonly IRevertable _revertable;

    private OfferChanceToSaveDialogUI(IRevertable revertable, RevertableObjectReport differences)
    {
        _revertable = revertable;
        InitializeComponent();

        if (revertable == null)
            return;

        Text = $"Save changes to {revertable.GetType().Name} {revertable} (ID = {revertable.ID})";

        lblFirstPrompt.Text =
            $"Would you like to save changes to {revertable.GetType().Name} '{revertable}' (ID={revertable.ID})";

        tableLayoutPanel1.RowCount = differences.Differences.Count;
        for (var index = 0; index < differences.Differences.Count; index++)
        {
            var d = differences.Differences[index];
            var toAdd = new RevertablePropertyDifferenceUI(d)
            {
                Dock = DockStyle.Fill
            };
            tableLayoutPanel1.Controls.Add(toAdd, 0, index);
        }

        for (var i = 0; i < tableLayoutPanel1.RowStyles.Count; i++)
            tableLayoutPanel1.RowStyles[i].SizeType = SizeType.AutoSize;
    }

    /// <summary>
    /// Shows a yes no to saving and describes differences in an IMapsDirectlyToDatabaseTable object which supports IRevertable
    /// </summary>
    /// <param name="revertable"></param>
    public static DialogResult? ShowIfRequired(IRevertable revertable)
    {
        var differences = revertable?.HasLocalChanges();

        return differences?.Evaluation == ChangeDescription.DatabaseCopyDifferent
            ? new OfferChanceToSaveDialogUI(revertable, differences).ShowDialog()
            : null;
    }

    private void btnYesSave_Click(object sender, EventArgs e)
    {
        _revertable.SaveToDatabase();
        DialogResult = DialogResult.Yes;
        Close();
    }

    private void btnNo_Click(object sender, EventArgs e)
    {
        _revertable.RevertToDatabaseState();
        DialogResult = DialogResult.No;
        Close();
    }

    private void btnViewStackTrace_Click(object sender, EventArgs e)
    {
        var dialog = new ExceptionViewerStackTraceWithHyperlinks(Environment.StackTrace);
        dialog.Show();
    }
}