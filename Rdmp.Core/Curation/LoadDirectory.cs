// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;

namespace Rdmp.Core.Curation;

/// <summary>
/// Basic implementation of ILoadDirectory including support for creating new templates on the file system.
/// </summary>
public class LoadDirectory : ILoadDirectory
{
    /// <inheritdoc/>
    public DirectoryInfo ForLoading { get; private set; }

    /// <inheritdoc/>
    public DirectoryInfo ForArchiving { get; private set; }

    /// <inheritdoc/>
    public DirectoryInfo Cache { get; private set; }

    /// <inheritdoc/>
    public DirectoryInfo RootPath { get; private set; }

    /// <inheritdoc/>
    public DirectoryInfo DataPath { get; private set; }

    /// <inheritdoc/>
    public DirectoryInfo ExecutablesPath { get; private set; }


    internal const string ExampleFixedWidthFormatFileContents = @"From,To,Field,Size,DateFormat
1,7,gmc,7,
8,12,gp_code,5,
13,32,surname,20,
33,52,forename,20,
53,55,initials,3,
56,60,practice_code,5,
61,68,date_into_practice,8,yyyyMMdd
69,76,date_out_of_practice,8,yyyyMMdd
";

    /// <summary>
    /// Declares that a new directory contains the folder structure required by the DLE.  Throws Exceptions if this folder doesn't exist or isn't set up yet.
    /// 
    /// <para>Use static method <see cref="CreateDirectoryStructure"/> if you want to create a new folder hierarchy on disk</para>
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="validate"></param>
    public LoadDirectory(string rootPath, bool validate=true)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new Exception("Root path was blank, there is no LoadDirectory path specified?");

        RootPath = new DirectoryInfo(rootPath);

        if (!validate) return;

        if (RootPath.Name.Equals("Data", StringComparison.CurrentCultureIgnoreCase))
            throw new ArgumentException("LoadDirectory should be passed the root folder, not the Data folder");

        DataPath = new DirectoryInfo(Path.Combine(RootPath.FullName, "Data"));

        if (!DataPath.Exists)
            throw new DirectoryNotFoundException(
                $"Could not find directory '{DataPath.FullName}', every LoadDirectory must have a Data folder, the root folder was:{RootPath}");
        ForLoading = FindFolderInPathOrThrow(DataPath, "ForLoading");
        ForArchiving = FindFolderInPathOrThrow(DataPath, "ForArchiving");
        ExecutablesPath = FindFolderInPathOrThrow(RootPath, "Executables");
        Cache = FindFolderInPath(DataPath, "Cache");
    }

    public LoadDirectory(string ForLoadingPath, string ForArchivingPath, string ExecutablesPathString, string cachePath)
    {
        if (string.IsNullOrWhiteSpace(ForLoadingPath) || string.IsNullOrWhiteSpace(ForArchivingPath) || string.IsNullOrWhiteSpace(ExecutablesPathString))
            throw new Exception($"One if the LoadDirectory Paths was blank. ForLoading: {ForLoading}. ForArchiving: {ForArchivingPath}. Extractables:{ExecutablesPath}");
        ForLoading = new DirectoryInfo(ForLoadingPath);
        ForArchiving = new DirectoryInfo(ForArchivingPath);
        ExecutablesPath =new DirectoryInfo(ExecutablesPathString);
        Cache = new DirectoryInfo(cachePath);
    }

    private static DirectoryInfo FindFolderInPath(DirectoryInfo path, string folderName) =>
        path.EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).FirstOrDefault();

    private DirectoryInfo FindFolderInPathOrThrow(DirectoryInfo path, string folderName)
    {
        var d = path.EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).FirstOrDefault() ??
                throw new DirectoryNotFoundException(
                    $"This dataset requires the directory '{folderName}' located at {Path.Combine(path.FullName, folderName)}");
        return d;
    }

    /// <summary>
    /// Creates a new directory on disk compatible with <see cref="LoadDirectory"/>.
    /// </summary>
    /// <param name="parentDir">Parent folder to create the tree in e.g. c:\temp</param>
    /// <param name="dirName">Root folder name for the DLE e.g. LoadingBiochem</param>
    /// <param name="overrideExistsCheck">Determines behaviour if the folder already exists and contains files.  True to carry on, False to throw an Exception</param>
    /// <returns></returns>
    public static LoadDirectory CreateDirectoryStructure(DirectoryInfo parentDir, string dirName,
        bool overrideExistsCheck = false)
    {
        if (!parentDir.Exists)
            parentDir.Create();

        var projectDir = new DirectoryInfo(Path.Combine(parentDir.FullName, dirName));

        if (!overrideExistsCheck && projectDir.Exists && projectDir.GetFileSystemInfos().Any())
            throw new Exception(
                $"The directory {projectDir.FullName} already exists (and we don't want to accidentally nuke anything)");

        projectDir.Create();

        var dataDir = projectDir.CreateSubdirectory("Data");
        dataDir.CreateSubdirectory("ForLoading");
        dataDir.CreateSubdirectory("ForArchiving");
        dataDir.CreateSubdirectory("Cache");

        var swExampleFixedWidth = new StreamWriter(Path.Combine(dataDir.FullName, "ExampleFixedWidthFormatFile.csv"));
        swExampleFixedWidth.Write(ExampleFixedWidthFormatFileContents);
        swExampleFixedWidth.Flush();
        swExampleFixedWidth.Close();

        projectDir.CreateSubdirectory("Executables");

        return new LoadDirectory(projectDir.FullName);
    }
}