// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.Sharing.Dependency.Gathering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Gathers dependencies of the supplied objects and extracts the share definitions to a directory.  This will have the
///     side effect of creating an ObjectExport declaration
///     if none yet exists which will prevent accidental deletion of the object and enable updating people who receive the
///     definition later on via the sharing Guid.
/// </summary>
public class ExecuteCommandExportObjectsToFile : BasicCommandExecution
{
    private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
    private readonly IMapsDirectlyToDatabaseTable[] _toExport;

    public DirectoryInfo TargetDirectoryInfo;
    public FileInfo TargetFileInfo;

    private readonly ShareManager _shareManager;
    private readonly Gatherer _gatherer;

    public bool ShowInExplorer { get; set; }

    public bool IsSingleObject => _toExport.Length == 1;

    public ExecuteCommandExportObjectsToFile(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable toExport,
        FileInfo targetFileInfo = null) : this(activator, new[] { toExport })
    {
        TargetFileInfo = targetFileInfo;
    }

    public ExecuteCommandExportObjectsToFile(IBasicActivateItems activator, IMapsDirectlyToDatabaseTable[] toExport,
        DirectoryInfo targetDirectoryInfo = null) : base(activator)
    {
        _toExport = toExport;
        ShowInExplorer = true;

        _repositoryLocator = activator.RepositoryLocator;
        _toExport = toExport;

        TargetDirectoryInfo = targetDirectoryInfo;

        _shareManager = new ShareManager(_repositoryLocator);

        if (toExport == null || !toExport.Any())
        {
            SetImpossible("No objects exist to be exported");
            return;
        }

        _gatherer = new Gatherer(_repositoryLocator);

        var incompatible = toExport.FirstOrDefault(o => !_gatherer.CanGatherDependencies(o));

        if (incompatible != null)
            SetImpossible($"Object {incompatible.GetType()} is not supported by Gatherer");
    }

    public override string GetCommandHelp()
    {
        return "Creates a share file with definitions for the supplied objects and all children";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(FamFamFamIcons.page_white_put);
    }


    public override string GetCommandName()
    {
        return "Export Object(s) to File...";
    }


    public override void Execute()
    {
        base.Execute();

        if (IsSingleObject)
        {
            //Extract a single object (to file)
            if (TargetFileInfo == null && BasicActivator.IsInteractive)
            {
                TargetFileInfo =
                    BasicActivator.SelectFile("Path to output share definition to", "Share Definition", "*.sd");

                if (TargetFileInfo == null)
                    return;
            }
        }
        else
        {
            if (TargetDirectoryInfo == null && BasicActivator.IsInteractive)
            {
                TargetDirectoryInfo = BasicActivator.SelectDirectory("Output Directory");

                if (TargetDirectoryInfo == null)
                    return;
            }
        }

        if (TargetFileInfo != null && IsSingleObject)
        {
            var d = _gatherer.GatherDependencies(_toExport[0]);

            var shareDefinitions = d.ToShareDefinitionWithChildren(_shareManager);
            var serial = JsonConvertExtensions.SerializeObject(shareDefinitions, _repositoryLocator);
            File.WriteAllText(TargetFileInfo.FullName, serial);

            return;
        }

        if (TargetDirectoryInfo == null)
            throw new Exception("No output directory set");

        foreach (var o in _toExport)
        {
            var d = _gatherer.GatherDependencies(o);
            var filename = $"{QuerySyntaxHelper.MakeHeaderNameSensible(o.ToString())}.sd";

            var shareDefinitions = d.ToShareDefinitionWithChildren(_shareManager);
            var serial = JsonConvertExtensions.SerializeObject(shareDefinitions, _repositoryLocator);
            var f = Path.Combine(TargetDirectoryInfo.FullName, filename);
            File.WriteAllText(f, serial);
        }


        if (ShowInExplorer && TargetDirectoryInfo != null)
            UsefulStuff.ShowPathInWindowsExplorer(TargetDirectoryInfo);
    }
}