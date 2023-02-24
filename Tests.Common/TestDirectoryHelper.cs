// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using NUnit.Framework;

namespace Tests.Common;

public class TestDirectoryHelper
{
    private readonly Type _type;
    public DirectoryInfo Directory { get; private set; }

    public TestDirectoryHelper(Type type)
    {
        _type = type;
    }

    public void SetUp()
    {
        if (Directory != null)
            throw new Exception("You should only call SetUp once");

        var rootDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        Directory = rootDir.CreateSubdirectory(_type.FullName);
    }

    public void TearDown()
    {
        if (Directory == null)
            throw new Exception("You have called TearDown without calling SetUp (the directory has not been initialised)");

        Directory.Delete(true);
    }

    public void DeleteAllEntriesInDir()
    {
        foreach (var entry in Directory.EnumerateDirectories())
            entry.Delete(true);

        foreach (var entry in Directory.EnumerateFiles())
            entry.Delete();
    }
}