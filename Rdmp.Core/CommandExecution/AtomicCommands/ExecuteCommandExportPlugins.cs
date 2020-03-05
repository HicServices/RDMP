using System.Drawing;
using System.IO;
using System.Linq;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.Repositories.Managers;
using ReusableLibraryCode.Icons.IconProvision;
using Plugin = Rdmp.Core.Curation.Data.Plugin;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExportPlugins : BasicCommandExecution
    {
        private DirectoryInfo _outDir;
        private Curation.Data.Plugin[] _plugins;

        public ExecuteCommandExportPlugins(IBasicActivateItems activator): this(activator,null)
        {
            
        }
        
        [UseWithObjectConstructor]
        public ExecuteCommandExportPlugins(IBasicActivateItems activator, DirectoryInfo outputDirectory):base(activator)
        {
            _outDir = outputDirectory;
            _plugins = BasicActivator.RepositoryLocator.CatalogueRepository.PluginManager.GetCompatiblePlugins();

            if(!_plugins.Any())
                SetImpossible("There are no compatible plugins (for the version of RDMP you are running)");

        }


        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Plugin, OverlayKind.Shortcut);
        }

        public override void Execute()
        {
            base.Execute();

            if (_outDir == null)
                _outDir = BasicActivator.SelectDirectory("Output directory");

            if (_outDir == null)
                return;

            foreach (Curation.Data.Plugin p in _plugins)
                p.LoadModuleAssemblies.FirstOrDefault()?.DownloadAssembly(_outDir);
        }
    }
}
