// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class LoadStageNodeStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly ICoreIconProvider _iconProvider;

    public LoadStageNodeStateBasedIconProvider(ICoreIconProvider iconProvider)
    {
        _iconProvider = iconProvider;
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        return o switch
        {
            LoadStage stage => GetImageForStage(stage),
            LoadBubble bubble => GetImageForStage(bubble.ToLoadStage()),
            LoadStageNode node => GetImageForStage(node.LoadStage),
            _ => null
        };
    }

    private Image<Rgba32> GetImageForStage(LoadStage loadStage)
    {
        return loadStage switch
        {
            LoadStage.GetFiles => _iconProvider.GetImage(RDMPConcept.GetFilesStage),
            LoadStage.Mounting => _iconProvider.GetImage(RDMPConcept.LoadBubbleMounting),
            LoadStage.AdjustRaw => _iconProvider.GetImage(RDMPConcept.LoadBubble),
            LoadStage.AdjustStaging => _iconProvider.GetImage(RDMPConcept.LoadBubble),
            LoadStage.PostLoad => _iconProvider.GetImage(RDMPConcept.LoadFinalDatabase),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}