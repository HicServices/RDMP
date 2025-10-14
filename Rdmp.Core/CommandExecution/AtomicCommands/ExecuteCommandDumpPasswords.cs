using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.DataExport.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandDumpPasswords: BasicCommandExecution, IAtomicCommand
    {

        private readonly IBasicActivateItems _activator;

        public ExecuteCommandDumpPasswords(IBasicActivateItems activator): base(activator) { 
            _activator = activator;
        }


        public override void Execute()
        {
            base.Execute();
            var edb = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>();
            Console.WriteLine("- External Database Servers - ");
            foreach(var eds in edb)
            {
                Console.WriteLine($"{eds.Name}({eds.ID}) - {eds.GetDecryptedPassword()}");
            }
            var ect = _activator.RepositoryLocator.DataExportRepository.GetAllObjects<ExternalCohortTable>();
            Console.WriteLine("- External Cohort Tables- ");
            foreach (var eds in ect)
            {
                Console.WriteLine($"{eds.Name}({eds.ID}) - {eds.GetDecryptedPassword()}");
            }
            var dac = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();
            Console.WriteLine("- Data Access Credentials - ");
            foreach (var eds in dac)
            {
                Console.WriteLine($"{eds.Name}({eds.ID}) - {eds.GetDecryptedPassword()}");
            }
            var rrdmp = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<RemoteRDMP>();
            Console.WriteLine("- External Database Servers - ");
            foreach (var eds in rrdmp)
            {
                Console.WriteLine($"{eds.Name}({eds.ID}) - {eds.GetDecryptedPassword()}");
            }
            //argument
            //WebServiceConfiguration
            //encryptes destings
        }
    }
}
