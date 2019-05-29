using Rdmp.Core.CommandLine.Runners;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
                SetImpossible("Plugins must " + PackPluginRunner.PluginPackageSuffix);

            _files = fileCommand.Files;
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
