// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;

/// <summary>
///     Provides overlay and basic icons for all DatabaseTypes
/// </summary>
public class DatabaseTypeIconProvider
{
    private readonly Image<Rgba32> _msBig;
    private readonly Image<Rgba32> _msOverlay;

    private readonly Image<Rgba32> _oraBig;
    private readonly Image<Rgba32> _oraOverlay;

    private readonly Image<Rgba32> _mysBig;
    private readonly Image<Rgba32> _mysOverlay;

    private readonly Image<Rgba32> _postgresBig;
    private readonly Image<Rgba32> _postgresOverlay;

    private readonly Image<Rgba32> _unknownBig;
    private readonly Image<Rgba32> _unknownOverlay;

    public DatabaseTypeIconProvider()
    {
        _msBig = Image.Load<Rgba32>(DatabaseProviderIcons.Microsoft);
        _msOverlay = Image.Load<Rgba32>(DatabaseProviderIcons.MicrosoftOverlay);

        _mysBig = Image.Load<Rgba32>(DatabaseProviderIcons.MySql);
        _mysOverlay = Image.Load<Rgba32>(DatabaseProviderIcons.MySqlOverlay);

        _oraBig = Image.Load<Rgba32>(DatabaseProviderIcons.Oracle);
        _oraOverlay = Image.Load<Rgba32>(DatabaseProviderIcons.OracleOverlay);

        _postgresBig = Image.Load<Rgba32>(DatabaseProviderIcons.PostgreSql);
        _postgresOverlay = Image.Load<Rgba32>(DatabaseProviderIcons.PostgreSqlOverlay);

        _unknownBig = Image.Load<Rgba32>(DatabaseProviderIcons.Unknown);
        _unknownOverlay = Image.Load<Rgba32>(DatabaseProviderIcons.UnknownOverlay);
    }

    public Image<Rgba32> GetOverlay(DatabaseType type)
    {
        return type switch
        {
            DatabaseType.MicrosoftSQLServer => _msOverlay,
            DatabaseType.MySql => _mysOverlay,
            DatabaseType.Oracle => _oraOverlay,
            DatabaseType.PostgreSql => _postgresOverlay,
            _ => _unknownOverlay
        };
    }

    public Image<Rgba32> GetImage(DatabaseType type)
    {
        return type switch
        {
            DatabaseType.MicrosoftSQLServer => _msBig,
            DatabaseType.MySql => _mysBig,
            DatabaseType.Oracle => _oraBig,
            DatabaseType.PostgreSql => _postgresBig,
            _ => _unknownBig
        };
    }
}