// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision;

public class IconProvider : ICoreIconProvider
{
    private readonly IIconProvider[] _pluginIconProviders;


    //protected readonly CatalogueStateBasedIconProvider CatalogueStateBasedIconProvider;

    public Image<Rgba32> ImageUnknown => Image.Load<Rgba32>(CatalogueIcons.NoIconAvailable);

    public IconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IIconProvider[] pluginIconProviders)
    {
        _pluginIconProviders = pluginIconProviders;
    }

    public virtual Image<Rgba32> GetImage(object concept, OverlayKind kind = OverlayKind.None)
    {
        if(concept is RDMPConcept)
        {

        }
        return ImageUnknown;
    }

    //Used for testing
    public bool HasIcon(object o)
    {
        return GetImage(o) != ImageUnknown;
    }
}