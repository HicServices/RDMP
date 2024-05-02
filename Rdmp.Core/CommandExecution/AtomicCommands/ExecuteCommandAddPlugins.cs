// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddPlugins : BasicCommandExecution, IAtomicCommand
{
    private FileInfo[] _files;

    public ExecuteCommandAddPlugins(IBasicActivateItems itemActivator) : base(itemActivator)
    {
    }

    public ExecuteCommandAddPlugins(IBasicActivateItems itemActivator, FileCollectionCombineable fileCombineable) :
        base(itemActivator)
    {
        _files = fileCombineable.Files;
        if (!_files.Any(static f => f.Extension != PackPluginRunner.PluginPackageSuffix)) return;

        SetImpossible($"Plugins must end {PackPluginRunner.PluginPackageSuffix}");
    }

    public override void Execute()
    {
        base.Execute();

        if (_files == null)
        {
            var f = BasicActivator.SelectFile("Plugin to add",
                $"Plugins (*{PackPluginRunner.PluginPackageSuffix})", $"*{PackPluginRunner.PluginPackageSuffix}");
            if (f != null)
                _files = new[] { f };
            else return;
        }


        foreach (var f in _files)
        {
            var runner = new PackPluginRunner(new PackOptions { File = f.FullName });
            runner.Run(BasicActivator.RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken());
        }

        Show("Changes will take effect on restart");
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Plugin, OverlayKind.Add);
    }
}