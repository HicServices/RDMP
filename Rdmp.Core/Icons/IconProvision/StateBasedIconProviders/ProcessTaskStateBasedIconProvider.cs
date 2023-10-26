// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class ProcessTaskStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _exe;
    private readonly Image<Rgba32> _sql;
    private readonly Image<Rgba32> _plugin;
    private readonly Image<Rgba32> _attacher;
    private readonly Image<Rgba32> _dataProvider;
    private readonly Image<Rgba32> _mutilateDataTables;

    public ProcessTaskStateBasedIconProvider()
    {
        _exe = Image.Load<Rgba32>(CatalogueIcons.Exe);
        _sql = Image.Load<Rgba32>(CatalogueIcons.SQL);
        _plugin = Image.Load<Rgba32>(CatalogueIcons.ProcessTask);

        _attacher = Image.Load<Rgba32>(CatalogueIcons.Attacher);
        _dataProvider = Image.Load<Rgba32>(CatalogueIcons.DataProvider);
        _mutilateDataTables = Image.Load<Rgba32>(CatalogueIcons.MutilateDataTables);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is Type && o.Equals(typeof(ProcessTask))) return _plugin;

        return o is not ProcessTask pt
            ? null
            : pt.ProcessTaskType switch
            {
                ProcessTaskType.Executable => _exe,
                ProcessTaskType.SQLFile => _sql,
                ProcessTaskType.SQLBakFile => _sql,
                ProcessTaskType.Attacher => _attacher,
                ProcessTaskType.DataProvider => _dataProvider,
                ProcessTaskType.MutilateDataTable => _mutilateDataTables,
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}