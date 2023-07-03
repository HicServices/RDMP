// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data.Pipelines;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class PipelineComponentStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Image<Rgba32> _component;
    private readonly Image<Rgba32> _source;
    private readonly Image<Rgba32> _destination;

    public PipelineComponentStateBasedIconProvider()
    {
        _component = Image.Load<Rgba32>(CatalogueIcons.PipelineComponent);
        _source = Image.Load<Rgba32>(CatalogueIcons.PipelineComponentSource);
        _destination = Image.Load<Rgba32>(CatalogueIcons.PipelineComponentDestination);
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        if (o is not PipelineComponent pc) return null;

        if (pc.Class != null && pc.Class.EndsWith("Source"))
            return _source;
        return pc.Class != null && pc.Class.EndsWith("Destination") ? _destination : _component;
    }
}