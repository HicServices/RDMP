// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using FAnsi;

namespace ReusableLibraryCode.Icons.IconProvision
{
    /// <summary>
    /// Provides overlay and basic icons for all DatabaseTypes
    /// </summary>
    public class DatabaseTypeIconProvider
    {
        private Bitmap _msBig;
        private Bitmap _msOverlay;

        private Bitmap _oraBig;
        private Bitmap _oraOverlay;

        private Bitmap _mysBig;
        private Bitmap _mysOverlay;

        public DatabaseTypeIconProvider()
        {
            _msBig = DatabaseProviderIcons.Microsoft;
            _msOverlay = DatabaseProviderIcons.MicrosoftOverlay;

            _mysBig = DatabaseProviderIcons.MySql;
            _mysOverlay = DatabaseProviderIcons.MySqlOverlay;

            _oraBig = DatabaseProviderIcons.Oracle;
            _oraOverlay = DatabaseProviderIcons.OracleOverlay;
        }

        public Bitmap GetOverlay(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return _msOverlay;
                case DatabaseType.MySql:
                    return _mysOverlay;
                case DatabaseType.Oracle:
                    return _oraOverlay;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public Bitmap GetImage(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MicrosoftSQLServer:
                    return _msBig;
                case DatabaseType.MySql:
                    return _mysBig;
                case DatabaseType.Oracle:
                    return _oraBig;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
