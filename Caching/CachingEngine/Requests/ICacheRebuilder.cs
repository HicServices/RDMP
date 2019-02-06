// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Threading;
using System.Threading.Tasks;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Requests
{
    /// <summary>
    /// Interface for attempting to rebuild the .\Data\Cache (or alternative) Cache directory based on files currently in the ForArchiving directory.
    /// </summary>
    [Obsolete("Not tied into any UI or engine classes.  Plugins may still implement this but it will never be called")]
    public interface ICacheRebuilder
    {
        Task RebuildCacheFromArchiveFiles(string[] filenameList, string destinationPath, IDataLoadEventListener listener, CancellationToken token);
    }
}