// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// This entity wraps references to plugin .nupkg files on disk.
/// </summary>
public sealed class LoadModuleAssembly
{
    internal static readonly List<LoadModuleAssembly> Assemblies=new();
    private readonly FileInfo _file;

    private LoadModuleAssembly(FileInfo file)
    {
        _file = file;
    }

    /// <summary>
    /// Unpack the plugin DLL files, excluding any Windows UI specific dlls when not running a Windows GUI
    /// </summary>
    internal static IEnumerable<ValueTuple<string, MemoryStream>> GetContents(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length < 100) yield break; // Ignore missing or empty files

        var pluginStream = info.OpenRead();
        Assemblies.Add(new LoadModuleAssembly(info));

        var isWin = AppDomain.CurrentDomain.GetAssemblies()
            .Any(static a => a.FullName?.StartsWith("Rdmp.UI", StringComparison.Ordinal) == true);

        if (!pluginStream.CanSeek)
            throw new ArgumentException("Seek needed", nameof(path));

        using var zip = new ZipFile(pluginStream);
        foreach (var e in zip.Cast<ZipEntry>()
                     .Where(static e => e.IsFile && e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                     .Where(e => isWin || !e.Name.Contains("/windows/")))
        {
            using var s = zip.GetInputStream(e);
            using var ms2 = new MemoryStream();
            s.CopyTo(ms2);
            ms2.Position = 0;
            yield return (e.Name, ms2);
        }
    }

    /// <summary>
    /// Copy the plugin nupkg to the given directory
    /// </summary>
    /// <param name="downloadDirectory"></param>
    public string DownloadAssembly(DirectoryInfo downloadDirectory)
    {
        if (!downloadDirectory.Exists)
            downloadDirectory.Create();
        var targetFile=Path.Combine(downloadDirectory.FullName, _file.Name);
        _file.CopyTo(targetFile,true);
        return targetFile;
    }

    /// <inheritdoc/>
    public override string ToString() => $"LoadModuleAssembly_{_file.Name}";

    public string GetFriendlyName() => _file.Name;

    public void Delete() => _file.Delete();
    public override string ToString() => _file.Name;
}