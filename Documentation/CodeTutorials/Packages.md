

# Packages Used By RDMP

### Risk Assessment common to all:
1. Packages on NuGet are virus scanned by the NuGet site.
2. This package is widely used and is actively maintained.
3. It is open source.

| Package | Source Code | License | Purpose | Additional Risk Assessment |
| ------- | ------------| ------- | ------- | -------------------------- |
| coverlet.collector | [GitHub](https://github.com/coverlet-coverage/coverlet) | [MIT](https://opensource.org/licenses/MIT) | Collects code coverage information | |
| [DockPanelSuite](http://dockpanelsuite.com/) | [GitHub](https://github.com/dockpanelsuite/dockpanelsuite) | [MIT](https://opensource.org/licenses/MIT)  | Provides Window layout and docking for RDMP. | There are no powershell initialization files in the package which can be run by the NuGet installer.|
| [ObjectListView.Official](http://objectlistview.sourceforge.net/cs/index.html) | [Svn](http://objectlistview.sourceforge.net/cs/download.html#bleeding-edge-source) | [GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.html) | Provides tree layout for user interfaces in main client application |
| fernandreu.ScintillaNET | [GitHub](https://github.com/fernandreu/ScintillaNET) | [MIT](https://opensource.org/licenses/MIT) | Provides text editor component with highlighting etc |
| NHunspell | [SourceForge](https://sourceforge.net/p/nhunspell/code/ci/default/tree/) | LGPL / MPL  | Adds spell check support to ScintillaNET text editor |
| [FAM FAM FAM Icons](https://web.archive.org/web/20070824000227/http://www.famfamfam.com/lab/icons/silk/) | N\A | [CC 2.5](https://creativecommons.org/licenses/by/2.5/) | Icons for user interfaces |
| Xam.Plugins.Settings | [GitHub](https://github.com/jamesmontemagno/SettingsPlugin)| [MIT](https://opensource.org/licenses/MIT)| Read/Write user settings for main client application |
| CommandLineParser | [GitHub](https://github.com/commandlineparser/commandline) | [MIT](https://opensource.org/licenses/MIT) | Allows command line arguments for main client application and CLI executables |
| CsvHelper | [GitHub](https://github.com/JoshClose/CsvHelper) | MS-PL / Apache 2.0 | Enables reading/writing CSV files |
| NPOI | [GitHub](https://github.com/tonyqus/npoi) | Apache 2.0 | Enables reading/writing Microsoft Excel files |
| ExcelNumberFormat | [GitHub](https://github.com/andersnm/ExcelNumberFormat) |[MIT](https://opensource.org/licenses/MIT)  | Handles translating number formats from Excel formats into usable values | |
| [NLog](https://nlog-project.org/) | [GitHub](https://github.com/NLog/NLog) | [BSD 3-Clause](https://github.com/NLog/NLog/blob/dev/LICENSE.txt) | Flexible user configurable logging | |
| HIC.FAnsiSql |[GitHub](https://github.com/HicServices/FAnsiSql) | [GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.html) | [DBMS] abstraction layer |
| HIC.BadMedicine | [GitHub](https://github.com/HicServices/BadMedicine) | [GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.html) | Generate Test Datasets for tests/exericses |
| SSH.NET  | [GitHub](https://github.com/sshnet/SSH.NET)  | [MIT](https://github.com/sshnet/SSH.NET/blob/develop/LICENSE) | Enables fetching files from SFTP servers |
| Moq 4 | [GitHub](https://github.com/moq/moq4) |[BSD 3](https://github.com/moq/moq4/blob/master/License.txt)  | Mock objects during unit testing |
| [Nunit](https://nunit.org/) |[GitHub](https://github.com/nunit/nunit) | [MIT](https://opensource.org/licenses/MIT) | Unit testing |
| Microsoft.NET.Test.Sdk | [GitHub](https://github.com/microsoft/vstest/) | [MIT](https://opensource.org/licenses/MIT) | Required for running tests| |
| NUnit3TestAdapter | [GitHub](https://github.com/nunit/nunit3-vs-adapter)| [MIT](https://opensource.org/licenses/MIT) | Run unit tests from within Visual Studio |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | [GitHub](https://github.com/JamesNK/Newtonsoft.Json) | [MIT](https://opensource.org/licenses/MIT) | Serialization of objects for sharing/transmission |
| YamlDotNet | [GitHub](https://github.com/aaubry/YamlDotNet)  | [MIT](https://opensource.org/licenses/MIT) |Loading configuration files|
| [SecurityCodeScan.VS2019](https://security-code-scan.github.io/) | [GitHub](https://github.com/security-code-scan/security-code-scan) | [GPL 3.0](https://www.gnu.org/licenses/gpl-3.0.html)| Performs static build time analysis for vulnerabilities in the codebase (e.g. Sql injection)| |
| SixLabors.ImageSharp | [GitHub](https://github.com/SixLabors/ImageSharp) | [Apache 2.0](https://github.com/SixLabors/ImageSharp/blob/main/LICENSE) | Platform-independent replacement for legacy Windows-only System.Drawing.Common | |
| SixLabors.ImageSharp.Drawing | [GitHub](https://github.com/SixLabors/ImageSharp.Drawing) | [Apache 2.0](https://github.com/SixLabors/ImageSharp/blob/main/LICENSE) | Font handling for ImageSharp | |
| System.Runtime.Loader | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT)  | Allows loading assemblies in dot net core| |
| System.Diagnostics.Debug | [GitHub](https://github.com/dotnet/corefx)  |[MIT](https://opensource.org/licenses/MIT) | Interact with Processes / Debug / Console |  |
| System.IO.FileSystem.Primitives | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides common enumerations and exceptions for path-based I/O libraries |  |
| System.IO.FileSystem | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides types that allow reading and writing to files |  |
| System.Runtime.Extensions | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides commonly-used classes for performing mathematical functions, conversions, string comparisons etc |  |
| System.Threading | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides the fundamental synchronization primitives |  |
| System.Threading.AccessControl | [GitHub](https://github.com/dotnet/runtime) |[MIT](https://opensource.org/licenses/MIT) | Required by Scintilla for sync primitives |  |
| System.Threading.ThreadPool | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Required to compile native linux binaries |  |
| System.Globalization | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides classes that define culture-related information |  |
| System.Net.NameResolution | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides the System.Net.Dns class, which enables developers to perform simple domain name resolution |  |
| System.Net.Primitives | [GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides common types for network-based libraries |  |
| System.Security.Permissions |[GitHub](https://github.com/dotnet/corefx) |[MIT](https://opensource.org/licenses/MIT) | Provides common types for Xml doc reading in UI code |  |
| [AutoComplete Console](https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications) by Jasper Lammers | Embedded | [CPOL](https://www.codeproject.com/info/cpol10.aspx) | Provides interactive autocomplete in console input | |
| System.Resources.Extensions | [GitHub](https://github.com/dotnet/corefx) | [MIT](https://opensource.org/licenses/MIT) | Allows [publishing with dotnet publish on machines with netcoreapp3.0 SDK installed](https://github.com/microsoft/msbuild/issues/4704#issuecomment-530034240) | |
| Spectre.Console | [GitHub](https://github.com/spectreconsole/spectre.console) | [MIT](https://opensource.org/licenses/MIT) | Allows richer command line interactions| |
| HIC.System.Windows.Forms.DataVisualization | [GitHub](https://github.com/HicServices/winforms-datavisualization) |[MIT](https://opensource.org/licenses/MIT) | Dotnet core support for DQE charts |  |
| System.DirectoryServices.Protocols | [GitHub](https://github.com/dotnet/runtime) | MIT | Required dependency of Oracle when using LDAP auth |
| Autoupdater.NET | [GitHub](https://github.com/ravibpatel/AutoUpdater.NET) | MIT | Manages updating of the RDMP windows client directly from the RDMP GitHub Releases|
| ConsoleControl | [GitHub](https://github.com/dwmkerr/consolecontrol)  | MIT | Runs RDMP cli subprocesses|


[DBMS]: ./Glossary.md#DBMS
