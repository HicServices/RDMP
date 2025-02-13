// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data.Cache;

namespace Rdmp.Core.Curation;

/// <summary>
/// Defines a rigid file structure in which there is a \Data\ directory \ForLoading directory \ForArchiving\ etc.  This structure is used to drive the DLE and allows
/// for standardisation of structure within a dataset directory (i.e. we always load from ForLoading and then move them after loading into ForArchiving).
/// </summary>
public interface ILoadDirectory
{
    /// <summary>
    /// The directory for storing files that should be loaded during DLE execution.  The contents of this folder will be archived after a successful
    /// data load.
    /// </summary>
    DirectoryInfo ForLoading { get; }

    /// <summary>
    /// The directory for storing archived source files that were loaded during a DLE execution.
    /// </summary>
    DirectoryInfo ForArchiving { get; }

    /// <summary>
    /// The directory for storing time based cached files that will ultimately be loaded by the DLE.  Caching should occur independently of data loading
    /// and is designed to be a long running task (See <see cref="CacheProgress"/>).
    /// </summary>
    DirectoryInfo Cache { get; }

    /// <summary>
    /// The base working directory for the RDMP Data Load Engine.  This folder should contain the <see cref="DataPath"/> and the <see cref="ExecutablesPath"/>.
    /// </summary>
    DirectoryInfo RootPath { get; }

    /// <summary>
    /// The subdirectory of <see cref="RootPath"/> which contains <see cref="ForLoading"/>, <see cref="ForArchiving"/> and <see cref="Cache"/>
    /// </summary>
    DirectoryInfo DataPath { get; }

    /// <summary>
    /// The subdirectory of <see cref="RootPath"/> which contains sql scripts and executables which can be run during the Data Load Engine execution.
    /// </summary>
    DirectoryInfo ExecutablesPath { get; }
}