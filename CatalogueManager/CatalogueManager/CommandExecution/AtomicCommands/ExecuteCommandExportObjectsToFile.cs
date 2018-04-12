using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportObjectsToFile : BasicUICommandExecution,IAtomicCommand
    {
        private readonly IMapsDirectlyToDatabaseTable[] _toExport;
        private ShareManager _shareManager;
        private Gatherer _gatherer;

        public ExecuteCommandExportObjectsToFile(IActivateItems activator, IMapsDirectlyToDatabaseTable[] toExport): base(activator)
        {
            _toExport = toExport;
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

            var sfd = new SaveFileDialog();
            sfd.Filter = "Object Export Collection (*.oe)|*.oe";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var fi = new FileInfo(sfd.FileName);
                
                var allDependencies = new HashSet<IMapsDirectlyToDatabaseTable>();

                foreach (var d in _toExport.Select(_gatherer.GatherDependencies))
                    foreach (var o in d.Flatten())
                        allDependencies.Add(o);

                var exportDictionary = new Dictionary<IMapsDirectlyToDatabaseTable, ObjectExport>();

                foreach (var o in allDependencies)
                    exportDictionary.Add(o, _shareManager.GetExportFor(o));

                
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_put;
        }
    }
}