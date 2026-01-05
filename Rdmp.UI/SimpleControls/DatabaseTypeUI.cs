// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using FAnsi;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.SimpleControls;

public partial class DatabaseTypeUI : UserControl
{
    private readonly DatabaseTypeIconProvider _databaseIconProvider;
    private DatabaseType _databaseType;
    public event EventHandler DatabaseTypeChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DatabaseType DatabaseType
    {
        get => _databaseType;
        set
        {
            _databaseType = value;

            if (bLoading)
                return;

            ddDatabaseType.SelectedItem = value;
            pbDatabaseProvider.Image = _databaseIconProvider.GetImage(value).ImageToBitmap();
        }
    }

    private bool bLoading = true;

    public DatabaseTypeUI()
    {
        InitializeComponent();
        ddDatabaseType.DataSource = Enum.GetValues(typeof(DatabaseType));

        _databaseIconProvider = new DatabaseTypeIconProvider();
        pbDatabaseProvider.Image = _databaseIconProvider.GetImage(DatabaseType.MicrosoftSQLServer).ImageToBitmap();

        bLoading = false;
    }

    public void LockDatabaseType(DatabaseType databaseType)
    {
        ddDatabaseType.SelectedItem = databaseType;
        ddDatabaseType.Enabled = false;
    }

    private bool changing;

    private void ddDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (changing)
            return;

        changing = true;

        DatabaseType = (DatabaseType)ddDatabaseType.SelectedItem;

        DatabaseTypeChanged?.Invoke(this, EventArgs.Empty);

        changing = false;
    }
}