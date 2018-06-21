using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using Sharing.Dependency.Gathering;

namespace Sharing.CommandExecution
{
    /// <summary>
    /// Gathers dependencies of the supplied objects and extracts the share definitions to a directory.  This will have the side effect of creating an ObjectExport declaration
    /// if none yet exists which will prevent accidental deletion of the object and enable updating people who receive the definition later on via the sharing Guid.
    /// </summary>
    public class ExecuteCommandExportObjectsToFile : BasicCommandExecution
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly IMapsDirectlyToDatabaseTable[] _toExport;
        public DirectoryInfo TargetDirectoryInfo;
        private readonly ShareManager _shareManager;
        private readonly Gatherer _gatherer;

        public ExecuteCommandExportObjectsToFile(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IMapsDirectlyToDatabaseTable[] toExport, DirectoryInfo targetDirectoryInfo = null)
        {
            _repositoryLocator = repositoryLocator;
            _toExport = toExport;
            TargetDirectoryInfo = targetDirectoryInfo;
            _shareManager = new ShareManager(repositoryLocator);

            if (toExport == null || !toExport.Any())
            {
                SetImpossible("No objects exist to be exported");
                return;
            }
            _gatherer = new Gatherer(repositoryLocator);

            var incompatible = toExport.FirstOrDefault(o => !_gatherer.CanGatherDependencies(o));

            if (incompatible != null)
                SetImpossible("Object " + incompatible.GetType() + " is not supported by Gatherer");
        }

        public override void Execute()
        {
            base.Execute();

            if (TargetDirectoryInfo == null)
                throw new Exception("No output directory set");

            foreach (var o in _toExport)
            {
                var d = _gatherer.GatherDependencies(o);
                var filename = QuerySyntaxHelper.MakeHeaderNameSane(o.ToString()) + ".sd";

                var shareDefinitions = d.ToShareDefinitionWithChildren(_shareManager);
                string serial = JsonConvertExtensions.SerializeObject(shareDefinitions, _repositoryLocator);
                var f = Path.Combine(TargetDirectoryInfo.FullName, filename);
                File.WriteAllText(f, serial);
            }
        }
    }
}
