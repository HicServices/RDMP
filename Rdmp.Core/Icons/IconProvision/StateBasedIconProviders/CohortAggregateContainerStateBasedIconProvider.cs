// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cohort;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class CohortAggregateContainerStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _union;
    private readonly Image<Rgba32> _intersect;
    private readonly Image<Rgba32> _except;

    public CohortAggregateContainerStateBasedIconProvider()
    {
        _union = Image.Load<Rgba32>(CatalogueIcons.UNION);
        _intersect = Image.Load<Rgba32>(CatalogueIcons.INTERSECT);
        _except = Image.Load<Rgba32>(CatalogueIcons.EXCEPT);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        return o switch
        {
            Type when o.Equals(typeof(CohortAggregateContainer)) => _intersect,
            SetOperation operation => GetImage(operation),
            _ => o is not CohortAggregateContainer container ? null : GetImage(container.Operation)
        };
    }

    private Image<Rgba32> GetImage(SetOperation operation)
    {
        return operation switch
        {
            SetOperation.UNION => _union,
            SetOperation.INTERSECT => _intersect,
            SetOperation.EXCEPT => _except,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}