using Rdmp.Core.Startup;
using System.Reflection;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Startup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReactUI.Server
{
    public static class RDMPInitialiser
    {

        //private static string _CatalogueConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RDMP_Catalogue;Integrated Security=True;Trust Server Certificate=True";
        //private static string _DataExportConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=RDMP_DataExport;Integrated Security=True;Trust Server Certificate=True";
        private static string _CatalogueConnectionString = "Data Source=host.docker.internal\\sqlexpress01;Initial Catalog=RDMP4_Catalogue;Integrated Security=True;Trust Server Certificate=True";
        private static string _DataExportConnectionString = "Data Source=host.docker.internal\\sqlexpress01;Initial Catalog=RDMP4_DataExport;Integrated Security=True;Trust Server Certificate=True";

        public static void Init(RDMPBootstrapOptions args)
        {
            if (_startup is not null)
            {
                return;
            }
            _startup = new Startup { SkipPatching = args.SkipPatching };
            _args = args;
            _args.PopulateConnectionStringsFromYamlIfMissing(ThrowImmediatelyCheckNotifier.Quiet);
            _args.GetConnectionStrings(out var c, out var d);
            _catalogueConnection =  c?.ConnectionString;
            _dataExportConnection =  d?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(_args.Dir))
            {
                _startup.RepositoryLocator = _args.GetRepositoryLocator();
            }
            else if (!string.IsNullOrWhiteSpace(_catalogueConnection) &&
                     !string.IsNullOrWhiteSpace(_dataExportConnection))
            {
                //_startup.RepositoryLocator = new LinkedRepositoryProvider(_catalogueConnection, _dataExportConnection);//todo this is causing problems
                //_startup.RepositoryLocator.CatalogueRepository.TestConnection();
                //_startup.RepositoryLocator.DataExportRepository.TestConnection();
            }
            RepositoryLocatorService rps = new RepositoryLocatorService();
            rps.StartScan(_CatalogueConnectionString, _DataExportConnectionString);
        }

        internal static string? _catalogueConnection { get; set; }
        internal static string? _dataExportConnection { get; set; }

        public static Startup? _startup { get; set; }
        internal static RDMPBootstrapOptions? _args { get; set; }


        public static string? GetVersion()
        {
            try
            {
                return typeof(RDMPInitialiser).Assembly.CustomAttributes
                    .FirstOrDefault(a => a.AttributeType == typeof(AssemblyInformationalVersionAttribute))
                    ?.ToString().Split('"')[1];
            }
            catch (Exception)
            {
                return "unknown version";
            }
        }
    }
}
