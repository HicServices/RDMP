// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// This entity wraps references to plugin .nupkg files on disk.
/// </summary>
public sealed class LoadModuleAssembly
{
    private static readonly bool IsWin = AppDomain.CurrentDomain.GetAssemblies()
        .Any(static a => a.FullName?.StartsWith("Rdmp.UI", StringComparison.Ordinal) == true);

    private static readonly string PluginsList = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rdmpplugins.txt");

    internal static readonly List<LoadModuleAssembly> Assemblies = [];
    private readonly FileInfo _file;

    private LoadModuleAssembly(FileInfo file)
    {
        _file = file;
    }


    private static bool IsReleaseCandidcate(string filename)
    {
        return filename.Split('.')[^2].Contains("-rc");
    }

    private static int GetReleaseCandidateId(string filename)
    {
        //expects the rc to be something like My.Plugin.3.0.0-rc2
        int.TryParse(filename.Split("-rc")[1].Split('.')[0], out var result);
        return result;
    }

    private static Dictionary<string, string> HandlePluginVersioning()
    {
        var PluginVersionLookup = new Dictionary<string, string>();
        var PluginPathLookup = new Dictionary<string, string>();
        var pluginFiles = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.rdmp");
        foreach (var pluginFile in pluginFiles)
        {
            using FileStream fs = new(pluginFile, FileMode.Open,FileAccess.Read);
            using ZipFile zip = new(fs);
            foreach (ZipEntry ze in zip)
            {
                if (ze.Name.Contains(".nuspec"))
                {
                    using StreamReader sr = new StreamReader(zip.GetInputStream(ze.ZipFileIndex));
                    var content = sr.ReadToEnd();
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(content);

                    var name = xmlDoc.GetElementsByTagName("id").Item(0).InnerText;
                    var version = xmlDoc.GetElementsByTagName("version").Item(0).InnerText;
                    if (PluginVersionLookup.TryGetValue(name, out var currentVersion))
                    {
                        //already found a version
                        var result = new Version(currentVersion).CompareTo(new Version(version));
                        if (result < 0)
                        {
                            //this version is the latest version
                            PluginVersionLookup[name] = version;
                            PluginPathLookup[name] = pluginFile;
                        }
                        if(result == 0)
                        {
                            //check for RC in the name
                            var currentIsReleaseCandidate = IsReleaseCandidcate(PluginPathLookup[name]);
                            var newIsReleaseCandidate = IsReleaseCandidcate(pluginFile);
                            if(newIsReleaseCandidate && currentIsReleaseCandidate)
                            {
                                var newIsLatest = GetReleaseCandidateId(pluginFile) > GetReleaseCandidateId(PluginPathLookup[name]);
                                if (newIsLatest)
                                {
                                    PluginVersionLookup[name] = version;
                                    PluginPathLookup[name] = pluginFile;
                                }
                            } else if (newIsReleaseCandidate)
                            {
                                //new version is the latest
                                PluginVersionLookup[name] = version;
                                PluginPathLookup[name] = pluginFile;
                            }
                        }
                    }
                    else
                    {
                        PluginVersionLookup[name] = version;
                        PluginPathLookup[name] = pluginFile;
                    }
                    break;
                }

            }

        }

        return PluginPathLookup;
    }

    /// <summary>
    /// List the plugin files to load
    /// </summary>
    /// <returns></returns>
    internal static IEnumerable<string> PluginFiles()
    {
        if (Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.rdmp").Count() > 0)
        {
            //use the modern .RDMP plugins
            var plugins = HandlePluginVersioning();
            return plugins.Values.ToArray();

        }
        return File.Exists(PluginsList)
            ? File.ReadAllLines(PluginsList)
                .Select(static name => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name))
            : Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.nupkg");
    }

    /// <summary>
    /// Unpack the plugin DLL files, excluding any Windows UI specific dlls when not running a Windows GUI
    /// </summary>
    internal static IEnumerable<(string, MemoryStream)> GetContents(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length < 100) yield break; // Ignore missing or empty files

        var pluginStream = info.OpenRead();
        if (!pluginStream.CanSeek)
            throw new ArgumentException("Seek needed", nameof(path));

        Assemblies.Add(new LoadModuleAssembly(info));

        using var zip = new ZipFile(pluginStream);
        foreach (var e in zip.Cast<ZipEntry>()
                     .Where(static e => e.IsFile && e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
                                        (IsWin || !e.Name.Contains("/windows/"))))
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
    public void DownloadAssembly(DirectoryInfo downloadDirectory)
    {
        if (!downloadDirectory.Exists)
            downloadDirectory.Create();
        var targetFile = Path.Combine(downloadDirectory.FullName, _file.Name);
        _file.CopyTo(targetFile, true);
    }

    /// <summary>
    /// Delete the plugin file from disk, and remove it from rdmpplugins.txt if in use
    /// </summary>
    public void Delete()
    {
        _file.Delete();
        if (!File.Exists(PluginsList)) return;

        var tmp = $"{PluginsList}.tmp";
        File.WriteAllLines(tmp, File.ReadAllLines(PluginsList).Where(l => !l.Contains(_file.Name)));
        File.Move(tmp, PluginsList, true);
    }

    public override string ToString() => _file.Name;

    public static void UploadFile(ICheckNotifier checkNotifier, FileInfo toCommit)
    {
        try
        {
            toCommit.CopyTo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, toCommit.Name), true);
            if (!File.Exists(PluginsList)) return;

            // Now the tricky bit: add this new file, and remove any other versions of the same
            // e.g. adding DummyPlugin-1.2.3 should delete DummyPlugin-2.0 and DummyPlugin-1.0 if present
            var list = File.ReadAllLines(PluginsList);
            var tmp = $"{PluginsList}.tmp";

            var versionPos = toCommit.Name.IndexOf('-');
            if (versionPos != -1)
            {
                var stub = toCommit.Name[..(versionPos + 1)];
                list = list.Where(l => !l.StartsWith(stub, StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            File.WriteAllLines(tmp, list.Union([toCommit.Name]));
            File.Move(tmp, PluginsList, true);
        }
        catch (Exception e)
        {
            checkNotifier.OnCheckPerformed(new CheckEventArgs($"Failed copying plugin {toCommit.Name}",
                CheckResult.Fail, e));
            throw;
        }
    }
}