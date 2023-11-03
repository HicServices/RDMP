using Microsoft.IdentityModel.Protocols;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Startup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{

    internal class Args : RDMPCommandLineOptions { }
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
            Args args = new Args()
            {
                Dir = _outputFile,
            };
            var yamlRespositoryLocator  = args.GetRepositoryLocator();
            //want to grab all object types from catalogue then loop over them and add them to the yaml database

            //var b = _catalogueRepository.GetAllObjects<IMapsDirectlyToDatabaseTable>();
            //var c = _dataExportRepository.GetAllObjects<IMapsDirectlyToDatabaseTable>();
            //var a = _catalogueRepository.GetConstuctorList();
            //foreach (var t in a)
            //{
            //    //var y = t;
            //    var b = _catalogueRepository.GetAllObjects<t.Key>();
            //}

            var x = _catalogueRepository.GetAllObjects <AggregateConfiguration> ();//this should be all types
            foreach (var item in x)
            {
                yamlRespositoryLocator.CatalogueRepository.SaveToDatabase(item);
            }
            Console.WriteLine(x);
        }
    }
}
