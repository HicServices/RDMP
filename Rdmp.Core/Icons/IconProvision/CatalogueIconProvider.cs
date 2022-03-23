// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using FAnsi;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;
using ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;

namespace Rdmp.Core.Icons.IconProvision
{
    public class CatalogueIconProvider : ICoreIconProvider
    {
        private readonly IIconProvider[] _pluginIconProviders;
        public IconOverlayProvider OverlayProvider { get; private set; }

        protected List<IObjectStateBasedIconProvider> StateBasedIconProviders = new List<IObjectStateBasedIconProvider>();

        protected readonly EnumImageCollection<RDMPConcept> ImagesCollection;
        protected readonly CatalogueStateBasedIconProvider CatalogueStateBasedIconProvider;
        private DatabaseTypeIconProvider _databaseTypeIconProvider = new DatabaseTypeIconProvider();

        public Bitmap ImageUnknown => ImagesCollection[RDMPConcept.NoIconAvailable];

        public CatalogueIconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
            IIconProvider[] pluginIconProviders)
        {
            _pluginIconProviders = pluginIconProviders;
            OverlayProvider = new IconOverlayProvider();
            ImagesCollection = new EnumImageCollection<RDMPConcept>(CatalogueIcons.ResourceManager);

            StateBasedIconProviders.Add(CatalogueStateBasedIconProvider = new CatalogueStateBasedIconProvider(repositoryLocator.DataExportRepository, OverlayProvider));
            StateBasedIconProviders.Add(new ExtractionInformationStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExtractableColumnStateBasedIconProvider(OverlayProvider));
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
            StateBasedIconProviders.Add(new PipelineComponentStateBasedIconProvider());
            StateBasedIconProviders.Add(new FilterStateBasedIconProvider(OverlayProvider));

            StateBasedIconProviders.Add(new ExtractCommandStateBasedIconProvider());
        }

        public virtual Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept is IDisableable d && d.IsDisabled)
                return OverlayProvider.GetGrayscale(GetImageImpl(concept, kind));

            return GetImageImpl(concept, kind);
        }

        protected virtual Bitmap GetImageImpl(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept == null)
                return null;

            //the only valid strings are "Catalogue" etc where the value exactly maps to an RDMPConcept
            if (concept is string)
            {
                RDMPConcept result;
                if (Enum.TryParse((string)concept, true, out result))
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
                return GetImageImpl(GetConceptForCollection((RDMPCollection)concept), kind);

            if (concept is RDMPConcept)
                return GetImageImpl(ImagesCollection[(RDMPConcept)concept], kind);

            if (concept is LinkedColumnInfoNode)
                return GetImageImpl(ImagesCollection[RDMPConcept.ColumnInfo], OverlayKind.Link);

            if (concept is CatalogueUsedByLoadMetadataNode usedByLmd)
                return GetImageImpl(usedByLmd.ObjectBeingUsed, OverlayKind.Link);

            if (concept is DataAccessCredentialUsageNode)
                return GetImageImpl(ImagesCollection[RDMPConcept.DataAccessCredentials], OverlayKind.Link);

            if (ConceptIs(typeof(ISqlParameter), concept))
                return GetImageImpl(RDMPConcept.ParametersNode, kind);

            if (ConceptIs(typeof(IContainer), concept))
                return GetImageImpl(RDMPConcept.FilterContainer, kind);

            if (ConceptIs(typeof(JoinableCohortAggregateConfiguration), concept))
                return GetImageImpl(RDMPConcept.PatientIndexTable);

            if (ConceptIs(typeof(JoinableCohortAggregateConfigurationUse), concept))
                return GetImageImpl(RDMPConcept.PatientIndexTable, OverlayKind.Link);

            if (concept is PermissionWindowUsedByCacheProgressNode)
                return GetImageImpl(((PermissionWindowUsedByCacheProgressNode)concept).GetImageObject(), OverlayKind.Link);

            if (ConceptIs(typeof(DashboardObjectUse), concept))
                return GetImageImpl(RDMPConcept.DashboardControl, OverlayKind.Link);

            if (concept is DatabaseType)
                return _databaseTypeIconProvider.GetImage((DatabaseType)concept);

            if (concept is ArbitraryFolderNode)
                return GetImageImpl(RDMPConcept.CatalogueFolder, kind);

            if (concept is DiscoveredDatabase)
                return GetImageImpl(RDMPConcept.Database);

            if (concept is DiscoveredTable)
                return GetImageImpl(RDMPConcept.TableInfo);

            if (concept is DiscoveredColumn)
                return GetImageImpl(RDMPConcept.ColumnInfo);

            if (concept is FlatFileToLoad)
                return GetImageImpl(RDMPConcept.File);

            if (concept is CohortCreationRequest)
                return GetImageImpl(RDMPConcept.ExtractableCohort, OverlayKind.Add);

            //This is special case when asking for icon for the Type, since the node itself is an IMasqueradeAs
            if (ReferenceEquals(concept, typeof(PipelineCompatibleWithUseCaseNode)))
                return GetImageImpl(RDMPConcept.Pipeline);

            foreach (var stateBasedIconProvider in StateBasedIconProviders)
            {
                var bmp = stateBasedIconProvider.GetImageIfSupportedObject(concept);
                if (bmp != null)
                    return GetImageImpl(bmp, kind);
            }

            string conceptTypeName = concept.GetType().Name;

            RDMPConcept t;

            //It is a System.Type, get the name and see if there's a corresponding image
            if (concept is Type type)
                if (TryParseTypeNameToRdmpConcept(type,out t))
                    return GetImageImpl(ImagesCollection[t], kind);

            //It is an instance of something, get the System.Type and see if there's a corresponding image
            if (Enum.TryParse(conceptTypeName, out t))
                return GetImageImpl(ImagesCollection[t], kind);

            //if the object is masquerading as something else
            if (concept is IMasqueradeAs)
            {
                //get what it's masquerading as
                var masqueradingAs = ((IMasqueradeAs)concept).MasqueradingAs();

                //provided we don't have a circular reference here!
                if (!(masqueradingAs is IMasqueradeAs))
                    return GetImageImpl(masqueradingAs, kind); //get an image for what your pretending to be
            }

            if(concept is IAtomicCommand cmd)
            {
                return (Bitmap)cmd.GetImage(this);
            }


            return ImageUnknown;

        }

        private bool TryParseTypeNameToRdmpConcept(Type type, out RDMPConcept t)
        {
            // is it a known Type like Project
            if(Enum.TryParse(type.Name, out t))
            {
                return true;
            }

            // try trimming the I off of IProject
            if(type.IsInterface && Enum.TryParse(type.Name.Substring(1), out t))
            {
                return true;
            }

            // we don't have a known icon for the Type yet
            return false;
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

        /// <summary>
        /// Returns true if the <paramref name="concept"/> is an instance of, System.Type or assignable to Type <paramref name="t"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="concept"></param>
        /// <returns></returns>
        public static bool ConceptIs(Type t, object concept)
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
        public Dictionary<string, Bitmap> GetImageList(bool addFavouritesOverlayKeysToo)
        {
            Dictionary<string, Bitmap> imageList = new Dictionary<string, Bitmap>();


            foreach (RDMPConcept concept in Enum.GetValues(typeof(RDMPConcept)))
            {
                var img = GetImage(concept);
                imageList.Add(concept.ToString(), img);

                if (addFavouritesOverlayKeysToo)
                    imageList.Add(concept + "Favourite", OverlayProvider.GetOverlay(img, OverlayKind.FavouredItem));
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
