// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public sealed class ExternalDatabaseServerStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _default;

    private readonly Dictionary<string, Image<Rgba32>> _assemblyToIconDictionary = new();
    private readonly DatabaseTypeIconProvider _typeSpecificIconsProvider;

    public ExternalDatabaseServerStateBasedIconProvider()
    {
        _default = Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer);

        _assemblyToIconDictionary.Add(new DataQualityEnginePatcher().Name,
            Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_DQE));
        _assemblyToIconDictionary.Add(new ANOStorePatcher().Name,
            Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_ANO));
        _assemblyToIconDictionary.Add(new IdentifierDumpDatabasePatcher().Name,
            Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_IdentifierDump));
        _assemblyToIconDictionary.Add(new QueryCachingPatcher().Name,
            Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_Cache));
        _assemblyToIconDictionary.Add(new LoggingDatabasePatcher().Name,
            Image.Load<Rgba32>(CatalogueIcons.ExternalDatabaseServer_Logging));

        _typeSpecificIconsProvider = new DatabaseTypeIconProvider();
    }

    public Image<Rgba32> GetIconForAssembly(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        return _assemblyToIconDictionary.GetValueOrDefault(assemblyName, _default);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        var server = o as ExternalDatabaseServer;
        var dumpServerUsage = o as IdentifierDumpServerUsageNode;

        if (dumpServerUsage != null)
            server = dumpServerUsage.IdentifierDumpServer;

        //if it's not a server we aren't responsible for providing an icon for it
        if (server == null)
            return null;

        //the untyped server icon (e.g. user creates a reference to a server that he is going to use but isn't created/managed by a .Database assembly)
        var toReturn = _default;

        //if it is a .Database assembly managed database then use the appropriate icon instead (ANO, LOG, IDD etc)
        if (!string.IsNullOrWhiteSpace(server.CreatedByAssembly) &&
            _assemblyToIconDictionary.TryGetValue(server.CreatedByAssembly, out var value))
            toReturn = value;

        //add the database type overlay
        toReturn = IconOverlayProvider.GetOverlay(toReturn, _typeSpecificIconsProvider.GetOverlay(server.DatabaseType));

        if (dumpServerUsage != null)
            toReturn = IconOverlayProvider.GetOverlay(toReturn, OverlayKind.Link);

        return toReturn;
    }
}