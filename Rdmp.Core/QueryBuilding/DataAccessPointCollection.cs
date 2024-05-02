// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Tracks a collection of <see cref="IDataAccessPoint" /> and optionally ensures that they all open connections
///     to the same server (and server type e.g. MySql on localhost).
/// </summary>
public class DataAccessPointCollection
{
    /// <summary>
    ///     True to require all <see cref="IDataAccessPoint" /> added to be on the same server
    /// </summary>
    public bool SingleServer { get; }

    /// <summary>
    ///     All <see cref="IDataAccessPoint" /> that have been added so far.
    /// </summary>
    public IReadOnlyCollection<IDataAccessPoint> Points => _points;

    public DataAccessContext DataAccessContext { get; }

    private HashSet<IDataAccessPoint> _points = new();

    /// <summary>
    ///     Creates a new collection of <see cref="IDataAccessPoint" /> for collecting dependencies e.g.
    ///     when building a query in which there are subqueries run on different databases
    /// </summary>
    /// <param name="singleServer">True to require all <see cref="Points" /> to be on the same server (and type).</param>
    /// <param name="context"></param>
    public DataAccessPointCollection(bool singleServer,
        DataAccessContext context = DataAccessContext.InternalDataProcessing)
    {
        SingleServer = singleServer;
        DataAccessContext = context;
    }

    /// <summary>
    ///     Adds the given <paramref name="point" /> to the collection. Throws InvalidOperationException if
    ///     <see cref="SingleServer" />
    ///     is set and the new <paramref name="point" /> is on a different server or <see cref="DatabaseType" />
    /// </summary>
    /// <param name="point"></param>
    public void Add(IDataAccessPoint point)
    {
        AddRange(new[] { point });
    }

    /// <summary>
    ///     Attempts to add <paramref name="point" /> to the collection returning true if it was successfully added.  Returns
    ///     false
    ///     if not added (e.g. if <see cref="SingleServer" /> is true and <paramref name="point" /> is to a different
    ///     server/type).
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool TryAdd(IDataAccessPoint point)
    {
        return TryAddRange(new[] { point });
    }

    /// <summary>
    ///     Attempts to add <paramref name="points" /> to the collection returning true if it was successfully added.  Returns
    ///     false
    ///     if not added (e.g. if <see cref="SingleServer" /> is true and <paramref name="points" /> is to a different
    ///     server/type).
    ///     <para>Either all or none of the <paramref name="points" /> will be added (i.e. not half)</para>
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public bool TryAddRange(IDataAccessPoint[] points)
    {
        try
        {
            AddRange(points);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }


    /// <summary>
    ///     Adds the given <paramref name="points" /> to the collection. Throws InvalidOperationException if
    ///     <see cref="SingleServer" />
    ///     is set and the new <paramref name="points" /> is on a different server or <see cref="DatabaseType" />
    /// </summary>
    /// <param name="points"></param>
    public void AddRange(IDataAccessPoint[] points)
    {
        //if we already have all the points then don't bother checking
        if (points.All(p => _points.Contains(p)))
            return;

        if (SingleServer)
        {
            var tempList = new HashSet<IDataAccessPoint>(_points);

            foreach (var p in points)
                tempList.Add(p);

            try
            {
                DataAccessPortal
                    .ExpectDistinctServer(tempList.ToArray(), DataAccessContext, false);

                //now add to the proper collection
                foreach (var p in points)
                    _points.Add(p);
            }
            catch (Exception e)
            {
                if (e is CryptographicException)
                    throw;

                throw new InvalidOperationException(
                    $"Could not identify single set of server/credentials to use with points:{string.Join(Environment.NewLine, tempList)}",
                    e);
            }
        }
        else
        {
            foreach (var p in points)
                _points.Add(p);
        }
    }

    /// <summary>
    ///     Clears <see cref="Points" />
    /// </summary>
    public void Clear()
    {
        _points.Clear();
    }

    /// <summary>
    ///     Returns comma separated list of <see cref="Points" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Join(",", Points.Select(p => p.ToString()));
    }

    /// <summary>
    ///     Returns as single server on which all <see cref="Points" /> can be reached (even if they are in
    ///     separate databases on that server).  Only valid if <see cref="SingleServer" /> is set (otherwise
    ///     throws <see cref="NotSupportedException" />
    /// </summary>
    /// <returns></returns>
    public DiscoveredServer GetDistinctServer()
    {
        if (!SingleServer)
            throw new NotSupportedException("Only valid when SingleServer flag is set");

        //they all have to be in the same server but do they also reside in the same database?
        var allOnSameDatabase = Points.Select(p => p.Database).Distinct().Count() == 1;

        return DataAccessPortal.ExpectDistinctServer(Points.ToArray(),
            DataAccessContext, allOnSameDatabase);
    }

    /// <summary>
    ///     Returns a new collection with a new set of <see cref="Points" /> matching the old set (but not instance).
    /// </summary>
    /// <returns></returns>
    public DataAccessPointCollection Clone()
    {
        var col = new DataAccessPointCollection(SingleServer, DataAccessContext)
        {
            _points = new HashSet<IDataAccessPoint>(_points)
        };
        return col;
    }

    /// <summary>
    ///     Tests whether the supplied <paramref name="point" /> could be added to the current collection (without
    ///     actually adding it).
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool AddWouldBePossible(IDataAccessPoint point)
    {
        return Clone().TryAdd(point);
    }
}