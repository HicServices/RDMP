// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Extensions;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

/// <summary>
/// Uploads a packed plugin (.nupkg) into a consumable plugin for RDMP
/// </summary>
public sealed partial class PackPluginRunner : IRunner
{
    private readonly PackOptions _packOpts;
    public const string PluginPackageSuffix = ".rdmp";
    private const string PluginPackageManifest = ".nuspec";

    private static readonly Regex VersionSuffix = VersionSuffixRe();

    public PackPluginRunner(PackOptions packOpts)
    {
        _packOpts = packOpts;
    }

    public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        var toCommit = new FileInfo(_packOpts.File);

        if (!toCommit.Exists)
            throw new FileNotFoundException($"Could not find file '{toCommit}'");

        if (!toCommit.Name.EndsWith(PluginPackageSuffix, StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Plugins must be packaged as {PluginPackageSuffix}");

        //the version of the plugin e.g. MyPlugin.nupkg version 1.0.0.0
        Version pluginVersion;

        //the version of rdmp on which the package depends on (e.g. 3.0)
        Version rdmpDependencyVersion;

        if (_packOpts.Prune)
        {
            var cmd = new ExecuteCommandPrunePlugin(_packOpts.File);
            cmd.Execute();
        }

        //find the manifest that lists name, version etc
        using (var zf = ZipFile.OpenRead(toCommit.FullName))
        {
            var manifests = zf.Entries.Where(static e => e.FullName.EndsWith(PluginPackageManifest, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (manifests.Length != 1)
                throw new Exception(
                    $"Found {manifests.Length} files in plugin with the extension {PluginPackageManifest}");

            using var s = manifests[0].Open();
            var doc = XDocument.Load(s);

            var ns = doc.Root
                .GetDefaultNamespace(); // XNamespace.Get("http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd");
            var versionNode = doc.Root.Element(ns + "metadata").Element(ns + "version") ??
                              throw new Exception("Could not find version tag");
            pluginVersion = new Version(VersionSuffix.Replace(versionNode.Value, ""));

            var rdmpDependencyNode =
                doc.Descendants(ns + "dependency")
                    .FirstOrDefault(static e => e?.Attribute("id")?.Value == "HIC.RDMP.Plugin") ?? throw new Exception(
                    "Expected a single <dependency> tag with id = HIC.RDMP.Plugin (in order to determine plugin compatibility).  Ensure your nuspec file includes a dependency on this package.");
            rdmpDependencyVersion =
                new Version(VersionSuffix.Replace(rdmpDependencyNode?.Attribute("version")?.Value ?? "", ""));
        }

        var runningSoftwareVersion = typeof(PackPluginRunner).Assembly.GetName().Version;
        if (!rdmpDependencyVersion.IsCompatibleWith(runningSoftwareVersion, 2))
            throw new NotSupportedException(
                $"Plugin version {pluginVersion} is incompatible with current running version of RDMP ({runningSoftwareVersion}).");

        LoadModuleAssembly.UploadFile(checkNotifier, toCommit);

        return 0;
    }

    [GeneratedRegex("-.*$")]
    private static partial Regex VersionSuffixRe();
}