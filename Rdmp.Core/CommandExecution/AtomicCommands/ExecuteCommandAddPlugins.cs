// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandAddPlugins : BasicCommandExecution, IAtomicCommand
{
    private FileInfo[] _files;

    public ExecuteCommandAddPlugins(IBasicActivateItems itemActivator):base(itemActivator)
    {

    }

    public ExecuteCommandAddPlugins(IBasicActivateItems itemActivator, FileCollectionCombineable fileCombineable):base(itemActivator)
    {
        if(!fileCombineable.Files.All(f=>f.Extension == PackPluginRunner.PluginPackageSuffix))
        {
            SetImpossible($"Plugins must {PackPluginRunner.PluginPackageSuffix}"); 
            return;
        }

        var existing = BasicActivator.RepositoryLocator.CatalogueRepository.PluginManager.GetCompatiblePlugins();

        _files = fileCombineable.Files;

        var collision = existing.FirstOrDefault(p=>_files.Any(f=>f.Name.Equals(p.Name)));
        if(collision != null)
            SetImpossible($"There is already a plugin called '{collision}'");

    }

    public override void Execute()
    {
        base.Execute();

        if(_files == null)
        {
                
            var f = BasicActivator.SelectFile("Plugin to add",
                $"Plugins (*{PackPluginRunner.PluginPackageSuffix})", $"*{PackPluginRunner.PluginPackageSuffix}");
            if(f != null)
                _files = new FileInfo[]{ f };
            else return;
        }


        foreach(var f in _files)
        {
            var runner = new PackPluginRunner(new CommandLine.Options.PackOptions(){File = f.FullName});
            runner.Run(BasicActivator.RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(),new ThrowImmediatelyCheckNotifier(),new DataFlowPipeline.GracefulCancellationToken());
        }
                
        Show("Changes will take effect on restart");
        var p = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Plugin>().FirstOrDefault();
            
        if(p!= null)
            Publish(p);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Plugin,OverlayKind.Add);
    }
}