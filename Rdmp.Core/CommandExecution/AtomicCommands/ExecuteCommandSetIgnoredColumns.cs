// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Repositories.Construction;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandSetIgnoredColumns : BasicCommandExecution
{
    private readonly LoadMetadata _loadMetadata;
    private readonly ColumnInfo[] _columnsToIgnore;
    private readonly bool _explicitChoiceMade;

    [UseWithObjectConstructor]
    public ExecuteCommandSetIgnoredColumns(IBasicActivateItems activator, LoadMetadata loadMetadata, ColumnInfo[] columnsToIgnore):base(activator)
    {
        this._loadMetadata = loadMetadata;
        this._columnsToIgnore = columnsToIgnore;
        _explicitChoiceMade = true;
    }

    public ExecuteCommandSetIgnoredColumns(IBasicActivateItems activator, LoadMetadata loadMetadata):base(activator)
    {
        this._loadMetadata = loadMetadata;
    }

    public override void Execute()
    {
        base.Execute();

        var availableColumns = _loadMetadata.GetDistinctTableInfoList(true).SelectMany(t=>t.ColumnInfos).ToArray();

        if(_explicitChoiceMade)
        {
            var ignore = _columnsToIgnore ?? Array.Empty<ColumnInfo>();

            foreach (var c in availableColumns)
            {
                c.IgnoreInLoads = ignore.Contains(c);
                c.SaveToDatabase();
            }
        }
        else
        {
            var chosen = BasicActivator.SelectMany("Ignore Columns (choice will replace old set)",typeof(ColumnInfo),availableColumns);

            if(chosen != null)
            {
                foreach (var c in availableColumns)
                {
                    c.IgnoreInLoads = chosen.Contains(c);
                    c.SaveToDatabase();
                }
            }
        }

        Publish(_loadMetadata);
    }

}