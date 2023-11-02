using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandExportPlatformDatabasesToYaml : BasicCommandExecution, IAtomicCommand
    {
        private TableRepository _catalogueRepository;
        private TableRepository _dataExportRepository;
        private string _outputFile;
        public ExecuteCommandExportPlatformDatabasesToYaml(IBasicActivateItems activator, [DemandsInitialization("Where the yaml file should be created")] string outputfile)
        {

            _catalogueRepository = activator.RepositoryLocator.CatalogueRepository as TableRepository;
            _dataExportRepository = activator.RepositoryLocator.DataExportRepository as TableRepository;
            _outputFile = outputfile;
            Execute();
        }

        public override void Execute()
        {
            base.Execute();

            var toSerialize = new ConnectionStringsYamlFile
            {
                CatalogueConnectionString = _catalogueRepository?.ConnectionString,
                DataExportConnectionString = _dataExportRepository?.ConnectionString
            };

            var serializer = new Serializer();
            var yaml = serializer.Serialize(toSerialize);
            File.WriteAllText(_outputFile, yaml);
        }
    }
}
