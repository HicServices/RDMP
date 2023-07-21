// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandExportPlugins : BasicCommandExecution
{
    private DirectoryInfo _outDir;
    private readonly Curation.Data.Plugin[] _plugins;

    public ExecuteCommandExportPlugins(IBasicActivateItems activator) : this(activator, null)
    {
    }

    [UseWithObjectConstructor]
    public ExecuteCommandExportPlugins(IBasicActivateItems activator, DirectoryInfo outputDirectory) : base(activator)
    {
        _outDir = outputDirectory;
        _plugins = BasicActivator.RepositoryLocator.CatalogueRepository.PluginManager.GetCompatiblePlugins();

        if (!_plugins.Any())
            SetImpossible("There are no compatible plugins (for the version of RDMP you are running)");
    }


    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Plugin, OverlayKind.Shortcut);
    }

    public override void Execute()
    {
        base.Execute();

        _outDir ??= BasicActivator.SelectDirectory("Output directory");

        if (_outDir == null)
            return;

        foreach (var p in _plugins)
            p.LoadModuleAssemblies.FirstOrDefault()?.DownloadAssembly(_outDir);
    }
}