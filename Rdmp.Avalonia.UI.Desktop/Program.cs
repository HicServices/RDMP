using System;

using Avalonia;
using Avalonia.ReactiveUI;
using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using Rdmp.Core.ReusableLibraryCode;
using RDMP.Avalonia.UI.Services.BootstrapService;

namespace RDMP.Avalonia.UI.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)  {
        UsefulStuff.GetParser().ParseArguments<RDMPBootstrapOptions>(args).MapResult(Setup, _ => -1);

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
}


    private static object Setup(RDMPBootstrapOptions args)
    {
        ImplementationManager.Load<MicrosoftSQLImplementation>();
        ImplementationManager.Load<MySqlImplementation>();
        ImplementationManager.Load<OracleImplementation>();
        ImplementationManager.Load<PostgreSqlImplementation>();
        BootstrapService.Init(args);
        return 0;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
