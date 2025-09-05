// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision;

public class DataExportIconProvider : CatalogueIconProvider
{
    public DataExportIconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IIconProvider[] pluginIconProviders) : base(repositoryLocator, pluginIconProviders)
    {
        //Calls to the Resource manager cause file I/O (I think or at the least CPU use anyway) so cache them all at once
        StateBasedIconProviders.Add(new ExtractableDataSetStateBasedIconProvider(CatalogueStateBasedIconProvider));
        StateBasedIconProviders.Add(new ExtractionConfigurationStateBasedIconProvider(this));
    }

    protected override Image<Rgba32> GetImageImpl(object concept, OverlayKind kind = OverlayKind.None)
    {
        if (concept is LinkedCohortNode)
            return base.GetImageImpl(RDMPConcept.ExtractableCohort, OverlayKind.Link);

        return concept as Type == typeof(SelectedDataSets)
            ? base.GetImageImpl(RDMPConcept.ExtractableDataSet)
            : concept switch
            {
                SelectedDataSets sds => base.GetImageImpl(sds.ExtractableDataSet),
                ExtractableDataSetProject edsp=> base.GetImageImpl(edsp.Project),
                PackageContentNode pcn => base.GetImageImpl(pcn.DataSet),
                ProjectCohortIdentificationConfigurationAssociation association =>
                    association.CohortIdentificationConfiguration != null
                        ? GetImageImpl(association.CohortIdentificationConfiguration, OverlayKind.Link)
                        :
                        //it's an orphan or user cannot fetch the cic for some reason
                        GetImageImpl(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link),
                _ => base.GetImageImpl(concept, kind)
            };

        //fallback on parent implementation if none of the above unique snowflake cases are met
    }
}