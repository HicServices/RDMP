// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

public static class NewObjectPool
{
    private static Scope _currentScope;
    private static readonly Lock currentScopeLock = new();

    public static void Add(IMapsDirectlyToDatabaseTable toCreate)
    {
        lock (currentScopeLock)
        {
            _currentScope?.Objects.Add(toCreate);
        }
    }

    /// <summary>
    /// Returns the latest object in <paramref name="from"/> that was created during this session or null.
    /// Objects are ordered by creation time so that the return value always reflects the most recent (if
    /// there are multiple matches)
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public static IMapsDirectlyToDatabaseTable Latest(IEnumerable<IMapsDirectlyToDatabaseTable> from)
    {
        lock (currentScopeLock)
        {
            //prevent multiple enumeration
            var fromArray = from.ToArray();

            return _currentScope?.Objects.LastOrDefault(fromArray.Contains);
        }
    }

    /// <summary>
    /// Starts a new session tracking all new objects created.  Make sure you wrap the
    /// returned session in a using statement.
    /// </summary>
    /// <exception cref="Exception">If there is already a session ongoing</exception>
    /// <returns></returns>
    public static IDisposable StartSession()
    {
        lock (currentScopeLock)
        {
            return _currentScope != null
                ? throw new Exception("An existing session is already underway")
                : (IDisposable)(_currentScope = new Scope());
        }
    }

    private static void EndSession()
    {
        lock (currentScopeLock)
        {
            _currentScope = null;
        }
    }

    private sealed class Scope : IDisposable
    {
        public List<IMapsDirectlyToDatabaseTable> Objects { get; set; } = new();


        public void Dispose()
        {
            Objects.Clear();
            EndSession();
        }
    }
}