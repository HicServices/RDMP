using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.Startup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RDMP.Avalonia.UI.Services.BootstrapService;
public static class BootstrapService
{
    internal static string? _catalogueConnection { get; set; }
    internal static string? _dataExportConnection { get; set; }

    public static Startup? _startup { get; set; }
    internal static RDMPBootstrapOptions? _args { get; set; }


    public static string? GetVersion()
    {
        try
        {
            return typeof(BootstrapService).Assembly.CustomAttributes
                .FirstOrDefault(a => a.AttributeType == typeof(AssemblyInformationalVersionAttribute))
                ?.ToString().Split('"')[1];
        }
        catch (Exception)
        {
            return "unknown version";
        }
    }

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
        _catalogueConnection = c?.ConnectionString;
        _dataExportConnection = d?.ConnectionString;

        if (!string.IsNullOrWhiteSpace(_args.Dir))
        {
            _startup.RepositoryLocator = _args.GetRepositoryLocator();
        }
        else if (!string.IsNullOrWhiteSpace(_catalogueConnection) &&
                 !string.IsNullOrWhiteSpace(_dataExportConnection))
        {
            _startup.RepositoryLocator = new LinkedRepositoryProvider(_catalogueConnection, _dataExportConnection);//todo this is causing problems
            _startup.RepositoryLocator.CatalogueRepository.TestConnection();
            _startup.RepositoryLocator.DataExportRepository.TestConnection();
        }
        RepositoryLocatorService.RepositoryLocatorService rps = new RepositoryLocatorService.RepositoryLocatorService();
        rps.StartScan();
    }

}

