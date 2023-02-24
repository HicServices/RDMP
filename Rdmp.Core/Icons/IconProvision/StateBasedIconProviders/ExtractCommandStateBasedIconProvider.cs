// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class ExtractCommandStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _waiting;
    private readonly Image<Rgba32> _warning;
    private readonly Image<Rgba32> _writing;
    private readonly Image<Rgba32> _failed;
    private readonly Image<Rgba32> _tick;

    public ExtractCommandStateBasedIconProvider()
    {
        _waiting = Image.Load<Rgba32>(CatalogueIcons.Waiting);
        _warning = Image.Load<Rgba32>(CatalogueIcons.Warning);
        _writing = Image.Load<Rgba32>(CatalogueIcons.Writing);
        _failed = Image.Load<Rgba32>(CatalogueIcons.Failed);
        _tick = Image.Load<Rgba32>(CatalogueIcons.Tick);
    }
    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not ExtractCommandState ecs)
            return null;

        return ecs switch
        {
            ExtractCommandState.NotLaunched => _waiting,
            ExtractCommandState.WaitingForSQLServer => _waiting,
            ExtractCommandState.WritingToFile => _writing,
            ExtractCommandState.Crashed => _failed,
            ExtractCommandState.UserAborted => _failed,
            ExtractCommandState.Completed => _tick,
            ExtractCommandState.Warning => _warning,
            ExtractCommandState.WritingMetadata => _writing,
            ExtractCommandState.WaitingToExecute => _waiting,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}