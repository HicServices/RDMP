// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Repositories;
using Rdmp.UI.Icons.IconProvision.StateBasedIconProviders;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.Icons.IconProvision
{
    public class DataExportIconProvider : CatalogueIconProvider
    {
        public DataExportIconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,IIconProvider[] pluginIconProviders): base(repositoryLocator,pluginIconProviders)
        {
            //Calls to the Resource manager cause file I/O (I think or at the least CPU use anyway) so cache them all at once  
            StateBasedIconProviders.Add(new ExtractableDataSetStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExtractionConfigurationStateBasedIconProvider(this));
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept is LinkedCohortNode)
                return base.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Link);

            if (concept as Type == typeof(SelectedDataSets))
                return base.GetImage(RDMPConcept.Catalogue, OverlayKind.Link);

            if (concept is SelectedDataSets)
                return base.GetImage(((SelectedDataSets)concept).ExtractableDataSet.Catalogue, OverlayKind.Link);

            if (concept is PackageContentNode)
                return base.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link);
            
            if (concept is ProjectCohortIdentificationConfigurationAssociation)
            {
                var cic = ((ProjectCohortIdentificationConfigurationAssociation) concept).CohortIdentificationConfiguration;
                //return image based on cic (will include frozen graphic if frozen)
                return cic != null ? GetImage(cic,OverlayKind.Link):
                    //it's an orphan or user cannot fetch the cic for some reason
                    GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);
            }

            //fallback on parent implementation if none of the above unique snowflake cases are met
            return base.GetImage(concept, kind);
        }
    }
}
