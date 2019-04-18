// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.CohortCreationPipeline;
using FAnsi;
using FAnsi.Discovery;
using ReusableLibraryCode;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision
{
    public class CatalogueIconProvider: ICoreIconProvider
    {
        private readonly IIconProvider[] _pluginIconProviders;
        public IconOverlayProvider OverlayProvider { get; private set; }
        
        protected List<IObjectStateBasedIconProvider> StateBasedIconProviders = new List<IObjectStateBasedIconProvider>();

        protected readonly EnumImageCollection<RDMPConcept> ImagesCollection;

        private DatabaseTypeIconProvider _databaseTypeIconProvider = new DatabaseTypeIconProvider();

        public CatalogueIconProvider(IIconProvider[] pluginIconProviders)
        {
            _pluginIconProviders = pluginIconProviders;
            OverlayProvider = new IconOverlayProvider();
            ImagesCollection = new EnumImageCollection<RDMPConcept>(CatalogueIcons.ResourceManager);

            StateBasedIconProviders.Add(new CatalogueStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new ExtractionInformationStateBasedIconProvider());
            StateBasedIconProviders.Add(new CheckResultStateBasedIconProvider());
            StateBasedIconProviders.Add(new CohortAggregateContainerStateBasedIconProvider());
            StateBasedIconProviders.Add(new SupportingObjectStateBasedIconProvider());
            StateBasedIconProviders.Add(new ColumnInfoStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new TableInfoStateBasedIconProvider());
            StateBasedIconProviders.Add(new AggregateConfigurationStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new CohortIdentificationConfigurationStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExternalDatabaseServerStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new LoadStageNodeStateBasedIconProvider(this));
            StateBasedIconProviders.Add(new ProcessTaskStateBasedIconProvider());
            StateBasedIconProviders.Add(new TableInfoServerNodeStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new CatalogueItemStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new ReleaseabilityStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExtractableCohortStateBasedIconProvider(OverlayProvider));

            StateBasedIconProviders.Add(new ExtractCommandStateBasedIconProvider());
        }

        public virtual Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept == null)
                return null;

            //the only valid strings are "Catalogue" etc where the value exactly maps to an RDMPConcept
            if(concept is string)
            {
                RDMPConcept result;
                if (RDMPConcept.TryParse((string) concept, true, out result)) 
                    concept = result;
                else 
                    return null; //it's a string but an unhandled one so give them null back
            }

            //if they already passed in an image just return it back (optionally with the overlay).
            if (concept is Bitmap)
                return GetImage((Bitmap)concept, kind);
            
            //if there are plugins injecting random objects into RDMP tree views etc then we need the ability to provide icons for them
            if (_pluginIconProviders != null)
                foreach (IIconProvider plugin in _pluginIconProviders)
                {
                    var img = plugin.GetImage(concept, kind);
                    if (img != null)
                        return img;
                }

            if (concept is RDMPCollection)
                return GetImage(GetConceptForCollection((RDMPCollection)concept),kind);

            if (concept is RDMPConcept)
                return GetImage(ImagesCollection[(RDMPConcept) concept], kind);

            if (concept is LinkedColumnInfoNode)
                return GetImage(ImagesCollection[RDMPConcept.ColumnInfo], OverlayKind.Link);

            if (concept is CatalogueUsedByLoadMetadataNode)
                return GetImage(ImagesCollection[RDMPConcept.Catalogue], OverlayKind.Link);

            if (concept is DataAccessCredentialUsageNode)
                return GetImage(ImagesCollection[RDMPConcept.DataAccessCredentials], OverlayKind.Link);

            if (ConceptIs(typeof(IFilter),concept))
                return GetImage(RDMPConcept.Filter, kind);

            if (ConceptIs(typeof(ISqlParameter), concept))
                return GetImage(RDMPConcept.ParametersNode,kind);

            if (ConceptIs(typeof(IContainer), concept))
                return GetImage(RDMPConcept.FilterContainer, kind);

            if (ConceptIs(typeof (JoinableCohortAggregateConfiguration), concept))
                return GetImage(RDMPConcept.PatientIndexTable);

            if (ConceptIs(typeof(JoinableCohortAggregateConfigurationUse), concept))
                return GetImage(RDMPConcept.PatientIndexTable,OverlayKind.Link);

            if (concept is PermissionWindowUsedByCacheProgressNode)
                return GetImage(((PermissionWindowUsedByCacheProgressNode)concept).GetImageObject(), OverlayKind.Link);

            if (ConceptIs(typeof (DashboardObjectUse),concept))
                return GetImage(RDMPConcept.DashboardControl, OverlayKind.Link);

            if (concept is DatabaseType)
                return _databaseTypeIconProvider.GetImage((DatabaseType)concept);

            if (concept is ArbitraryFolderNode)
                return GetImage(RDMPConcept.CatalogueFolder, kind);

            if (concept is DiscoveredDatabase)
                return GetImage(RDMPConcept.Database);
            
            if (concept is DiscoveredTable)
                return GetImage(RDMPConcept.TableInfo);
            
            if (concept is DiscoveredColumn)
                return GetImage(RDMPConcept.ColumnInfo);

            if (concept is FlatFileToLoad)
                return GetImage(RDMPConcept.File);

            if (concept is CohortCreationRequest)
                return GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);

            //This is special case when asking for icon for the Type, since the node itself is an IMasqueradeAs
            if (ReferenceEquals(concept, typeof(PipelineCompatibleWithUseCaseNode)))
                return GetImage(RDMPConcept.Pipeline);

            foreach (var stateBasedIconProvider in StateBasedIconProviders)
            {
                var bmp = stateBasedIconProvider.GetImageIfSupportedObject(concept);
                if (bmp != null)
                    return GetImage(bmp,kind);
            }
            
            string conceptTypeName = concept.GetType().Name;
            
            RDMPConcept t;

            //It is a System.Type, get the name and see if theres a corresponding image
            if (concept is Type)
                if (Enum.TryParse(((Type)concept).Name, out t))
                    return GetImage(ImagesCollection[t], kind);

            //It is an instance of something, get the System.Type and see if theres a corresponding image
            if(Enum.TryParse(conceptTypeName,out t))
                return GetImage(ImagesCollection[t],kind);

            if (concept is IMasqueradeAs)
                return GetImage(((IMasqueradeAs)concept).MasqueradingAs(), kind);
            
            return ImagesCollection[RDMPConcept.NoIconAvailable];
            
        }

        /// <inheritdoc/>
        public bool HasIcon(object o)
        {
            return GetImage(o) != ImagesCollection[RDMPConcept.NoIconAvailable];
        }

        public RDMPConcept GetConceptForCollection(RDMPCollection rdmpCollection)
        {
            switch (rdmpCollection)
            {
                case RDMPCollection.None:
                    return RDMPConcept.NoIconAvailable;
                case RDMPCollection.Tables:
                    return RDMPConcept.TableInfo;
                case RDMPCollection.Catalogue:
                    return RDMPConcept.Catalogue;
                case RDMPCollection.DataExport:
                    return RDMPConcept.Project;
                case RDMPCollection.SavedCohorts:
                    return RDMPConcept.AllCohortsNode;
                case RDMPCollection.Favourites:
                    return RDMPConcept.Favourite;
                case RDMPCollection.Cohort:
                    return RDMPConcept.CohortIdentificationConfiguration;
                case RDMPCollection.DataLoad:
                    return RDMPConcept.LoadMetadata;
                default:
                    throw new ArgumentOutOfRangeException("rdmpCollection");
            }
        }

        private bool ConceptIs(Type t, object concept)
        {
            if (t.IsInstanceOfType(concept))
                return true;

            var type = concept as Type;

            if (type != null && t.IsAssignableFrom(type))
                return true;

            return false;
        }


        /// <summary>
        /// Returns an image list dictionary with string keys that correspond to the names of all the RDMPConcept Enums.
        /// </summary>
        /// <param name="addFavouritesOverlayKeysToo">Pass true to also generate Images for every concept with a star overlay with the key being EnumNameFavourite (where EnumName is the RDMPConcept name e.g. CatalogueFavourite for the icon RDMPConcept.Catalogue and the favourite star)</param>
        /// <returns></returns>
        public ImageList GetImageList(bool addFavouritesOverlayKeysToo)
        {
            ImageList imageList = new ImageList();


            foreach (RDMPConcept concept in Enum.GetValues(typeof(RDMPConcept)))
            {
                var img = GetImage(concept);
                imageList.Images.Add(concept.ToString(),img);

                if (addFavouritesOverlayKeysToo)
                    imageList.Images.Add(concept + "Favourite",OverlayProvider.GetOverlay(img, OverlayKind.FavouredItem));
            }

            return imageList;
        }

        private Bitmap GetImage(Bitmap img, OverlayKind kind)
        {
            if (kind == OverlayKind.None)
                return img;

            return OverlayProvider.GetOverlay(img, kind);
        }
    }
}
