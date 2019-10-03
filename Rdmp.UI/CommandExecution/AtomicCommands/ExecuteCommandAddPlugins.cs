// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandLine.Runners;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    class ExecuteCommandAddPlugins : BasicUICommandExecution, IAtomicCommand
    {
        private FileInfo[] _files;

        public ExecuteCommandAddPlugins(IActivateItems itemActivator):base(itemActivator)
        {

        }

        public ExecuteCommandAddPlugins(IActivateItems itemActivator, FileCollectionCommand fileCommand):base(itemActivator)
        {
            if(!fileCommand.Files.All(f=>f.Extension == PackPluginRunner.PluginPackageSuffix))
            {
                SetImpossible("Plugins must " + PackPluginRunner.PluginPackageSuffix); 
                return;
            }

            var existing = Activator.RepositoryLocator.CatalogueRepository.PluginManager.GetCompatiblePlugins();

            _files = fileCommand.Files;

            var collision = existing.FirstOrDefault(p=>_files.Any(f=>f.Name.Equals(p.Name)));
            if(collision != null)
                SetImpossible("There is already a plugin called '" + collision + "'");

        }

        public override void Execute()
        {
            base.Execute();

            if(_files == null)
            {
                
                var f = base.SelectOpenFile(string.Format("Plugins (*{0})|*{0}" , PackPluginRunner.PluginPackageSuffix));
                if(f != null)
                    _files = new FileInfo[]{ f };
                else return;
            }


            foreach(FileInfo f in _files)
            {
                var runner = new PackPluginRunner(new Core.CommandLine.Options.PackOptions(){File = f.FullName});
                runner.Run(Activator.RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(),new ThrowImmediatelyCheckNotifier(),new Core.DataFlowPipeline.GracefulCancellationToken());
            }
                
            MessageBox.Show("Changes will take effect on restart");
            var p = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Rdmp.Core.Curation.Data.Plugin>().FirstOrDefault();
            
            if(p!= null)
                Publish(p);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Plugin,OverlayKind.Add);
        }
    }
}
