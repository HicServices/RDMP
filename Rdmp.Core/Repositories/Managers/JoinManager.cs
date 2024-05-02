// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Handles creation, discovery and deletion of JoinInfos.  JoinInfos are not IMapsDirectlyToDatabaseTable classes
///     because they are mostly just a m-m relationship
///     table between ColumnInfos (with join direction / collation).
/// </summary>
public class JoinManager : IJoinManager
{
    private readonly ICatalogueRepository _repository;

    public JoinManager(ICatalogueRepository repository)
    {
        _repository = repository;
    }

    public JoinInfo[] GetAllJoinInfosBetweenColumnInfoSets(JoinInfo[] joinInfos, ColumnInfo[] set1, ColumnInfo[] set2)
    {
        //assemble the IN SQL arrays
        if (set1.Length == 0)
            throw new NullReferenceException("Cannot find joins because column set 1 was empty");
        if (set2.Length == 0)
            throw new NullReferenceException("Cannot find joins because column set 2 was empty");

        var idSet1 = new HashSet<int>(set1.Select(o => o.ID));
        var idSet2 = new HashSet<int>(set2.Select(o => o.ID));

        return
            joinInfos
                .Where(j =>
                    (idSet1.Contains(j.ForeignKey_ID) && idSet2.Contains(j.PrimaryKey_ID))
                    ||
                    (idSet1.Contains(j.PrimaryKey_ID) && idSet2.Contains(j.ForeignKey_ID)))
                .ToArray();
    }

    public JoinInfo[] GetAllJoinInfosBetweenColumnInfoSets(ColumnInfo[] set1, ColumnInfo[] set2)
    {
        return GetAllJoinInfosBetweenColumnInfoSets(_repository.GetAllObjects<JoinInfo>(), set1, set2);
    }

    public JoinInfo[] GetAllJoinInfosWhereTableContains(ITableInfo tableInfo, JoinInfoType type)
    {
        var ids = new HashSet<int>(tableInfo.ColumnInfos.Select(c => c.ID));

        return type switch
        {
            JoinInfoType.AnyKey => _repository.GetAllObjects<JoinInfo>()
                .Where(j => ids.Contains(j.ForeignKey_ID) || ids.Contains(j.PrimaryKey_ID))
                .ToArray(),
            JoinInfoType.ForeignKey => _repository.GetAllObjects<JoinInfo>()
                .Where(j => ids.Contains(j.ForeignKey_ID))
                .ToArray(),
            JoinInfoType.PrimaryKey => _repository.GetAllObjects<JoinInfo>()
                .Where(j => ids.Contains(j.PrimaryKey_ID))
                .ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}