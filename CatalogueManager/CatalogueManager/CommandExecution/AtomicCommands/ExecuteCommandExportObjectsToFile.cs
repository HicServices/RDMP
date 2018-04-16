using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using Newtonsoft.Json;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Icons.IconProvision;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportObjectsToFile : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toExport;
        private readonly DirectoryInfo _targetDirectoryInfo;
        private ShareManager _shareManager;
        private Gatherer _gatherer;

        public ExecuteCommandExportObjectsToFile(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toExport,DirectoryInfo targetDirectoryInfo = null): base(activator)
        {
            _toExport = toExport;
            _targetDirectoryInfo = targetDirectoryInfo;
            _shareManager = new ShareManager(activator.RepositoryLocator);

            if(toExport == null || !toExport.Any())
            {
                SetImpossible("No objects exist to be exported");
                return;
            }
            _gatherer = new Gatherer(activator.RepositoryLocator);

            var incompatible = toExport.FirstOrDefault(o => !_gatherer.CanGatherDependencies(o));
            
            if(incompatible != null)
                SetImpossible("Object " + incompatible.GetType() + " is not supported by Gatherer");
        }

        public override void Execute()
        {
            base.Execute();

            var dir = _targetDirectoryInfo;

            if(dir == null)
            {
                var fb = new FolderBrowserDialog();
                if (fb.ShowDialog() == DialogResult.OK)
                    dir = new DirectoryInfo(fb.SelectedPath);
            }
            
            if(dir != null)
            {
                foreach (var o in _toExport)
                {
                    var d = _gatherer.GatherDependencies(o);
                    var filename = QuerySyntaxHelper.MakeHeaderNameSane(o.ToString()) + ".sd";
                    
                    var shareDefinitions = d.ToShareDefinitionWithChildren(_shareManager);
                    string serial = JsonConvertExtensions.SerializeObject(shareDefinitions, Activator.RepositoryLocator);
                    var f = Path.Combine(dir.FullName, filename);
                    File.WriteAllText(f,serial);
                }

                Process.Start("explorer.exe", dir.FullName);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_put;
        }
    }
}