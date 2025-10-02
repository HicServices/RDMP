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
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Rdmp.Core.Icons.IconProvision;

public class CatalogueIconProvider : ICoreIconProvider
{
    private readonly IIconProvider[] _pluginIconProviders;

    protected readonly List<IObjectStateBasedIconProvider> StateBasedIconProviders = new();

    protected readonly EnumImageCollection<RDMPConcept> ImagesCollection;
    protected readonly CatalogueStateBasedIconProvider CatalogueStateBasedIconProvider;
    private DatabaseTypeIconProvider _databaseTypeIconProvider = new();

    public Image<Rgba32> ImageUnknown => ImagesCollection[RDMPConcept.NoIconAvailable];

    public CatalogueIconProvider(IRDMPPlatformRepositoryServiceLocator repositoryLocator,
        IIconProvider[] pluginIconProviders)
    {
        _pluginIconProviders = pluginIconProviders;
        ImagesCollection = new EnumImageCollection<RDMPConcept>(CatalogueIcons.ResourceManager);

        StateBasedIconProviders.Add(CatalogueStateBasedIconProvider =
            new CatalogueStateBasedIconProvider(repositoryLocator.DataExportRepository));
        StateBasedIconProviders.Add(new ExtractionInformationStateBasedIconProvider());
        StateBasedIconProviders.Add(new ExtractableColumnStateBasedIconProvider());
        StateBasedIconProviders.Add(new CheckResultStateBasedIconProvider());
        StateBasedIconProviders.Add(new CohortAggregateContainerStateBasedIconProvider());
        StateBasedIconProviders.Add(new SupportingObjectStateBasedIconProvider());
        StateBasedIconProviders.Add(new ColumnInfoStateBasedIconProvider());
        StateBasedIconProviders.Add(new TableInfoStateBasedIconProvider());
        StateBasedIconProviders.Add(new AggregateConfigurationStateBasedIconProvider());
        StateBasedIconProviders.Add(new CohortIdentificationConfigurationStateBasedIconProvider());
        StateBasedIconProviders.Add(new ExternalDatabaseServerStateBasedIconProvider());
        StateBasedIconProviders.Add(new LoadStageNodeStateBasedIconProvider(this));
        StateBasedIconProviders.Add(new ProcessTaskStateBasedIconProvider());
        StateBasedIconProviders.Add(new TableInfoServerNodeStateBasedIconProvider());
        StateBasedIconProviders.Add(new CatalogueItemStateBasedIconProvider());
        StateBasedIconProviders.Add(new CatalogueItemsNodeStateBasedIconProvider());
        StateBasedIconProviders.Add(new ReleaseabilityStateBasedIconProvider());
        StateBasedIconProviders.Add(new ExtractableCohortStateBasedIconProvider());
        StateBasedIconProviders.Add(new PipelineComponentStateBasedIconProvider());
        StateBasedIconProviders.Add(new FilterStateBasedIconProvider());

        StateBasedIconProviders.Add(new ExtractCommandStateBasedIconProvider());
    }

    public virtual Image<Rgba32> GetImage(object concept, OverlayKind kind = OverlayKind.None) =>
        concept is IDisableable { IsDisabled: true }
            ? IconOverlayProvider.GetGreyscale(GetImageImpl(concept, kind))
            : GetImageImpl(concept, kind);

    protected virtual Image<Rgba32> GetImageImpl(object concept, OverlayKind kind = OverlayKind.None)
    {
        Image<Rgba32> x;

        switch (concept)
        {
            case null:
                return null;

            //the only valid strings are "Catalogue" etc where the value exactly maps to an RDMPConcept
            case string str when Enum.TryParse(str, true, out RDMPConcept result):
                concept = result;
                break;
            case string str:
                return null; //it's a string but an unhandled one so give them null back
        }

        if (concept.GetType().IsGenericType && concept.GetType().GetGenericTypeDefinition() == typeof(FolderNode<>))
        {

            x = GetImage(RDMPConcept.CatalogueFolder, kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }
        //if they already passed in an image just return it back (optionally with the overlay).
        if (concept is Image<Rgba32> image)
        {
            x = GetActualImage(image, kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        //if there are plugins injecting random objects into RDMP tree views etc then we need the ability to provide icons for them
        if (_pluginIconProviders != null)
            foreach (var plugin in _pluginIconProviders)
            {
                var img = plugin.GetImage(concept, kind);
                if (img != null)
                {
                    img.Mutate(x => x.Resize(16, 16));
                    return img;
                }
            }

        switch (concept)
        {
            case RDMPCollection collection:
                x = GetImageImpl(GetConceptForCollection(collection), kind);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case RDMPConcept rdmpConcept:
                x = GetImageImpl(ImagesCollection[rdmpConcept], kind);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case LinkedColumnInfoNode:
                x = GetImageImpl(ImagesCollection[RDMPConcept.ColumnInfo], OverlayKind.Link);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case CatalogueUsedByLoadMetadataNode usedByLmd:
                x = GetImageImpl(usedByLmd.ObjectBeingUsed, OverlayKind.Link);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case DataAccessCredentialUsageNode:
                x = GetImageImpl(ImagesCollection[RDMPConcept.DataAccessCredentials], OverlayKind.Link);
                x.Mutate(x => x.Resize(16, 16));
                return x;
        }

        if (ConceptIs(typeof(ISqlParameter), concept))
        {
            x = GetImageImpl(RDMPConcept.ParametersNode, kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        if (ConceptIs(typeof(IContainer), concept))
        {
            x = GetImageImpl(RDMPConcept.FilterContainer, kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        if (ConceptIs(typeof(JoinableCohortAggregateConfiguration), concept))
        {
            x = GetImageImpl(RDMPConcept.PatientIndexTable);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        if (ConceptIs(typeof(JoinableCohortAggregateConfigurationUse), concept))
        {
            x = GetImageImpl(RDMPConcept.PatientIndexTable, OverlayKind.Link);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        if (concept is PermissionWindowUsedByCacheProgressNode node)
        {
            x = GetImageImpl(node.GetImageObject(), OverlayKind.Link);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        if (ConceptIs(typeof(DashboardObjectUse), concept))
        {
            x = GetImageImpl(RDMPConcept.DashboardControl, OverlayKind.Link);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        switch (concept)
        {
            case DatabaseType databaseType:
                x = _databaseTypeIconProvider.GetImage(databaseType);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case ArbitraryFolderNode:
                x = GetImageImpl(RDMPConcept.CatalogueFolder, kind);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case DiscoveredDatabase:
                x = GetImageImpl(RDMPConcept.Database);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case DiscoveredTable:
                x = GetImageImpl(RDMPConcept.TableInfo);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case DiscoveredColumn:
                x = GetImageImpl(RDMPConcept.ColumnInfo);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case FlatFileToLoad:
                x = GetImageImpl(RDMPConcept.File);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            case CohortCreationRequest:
                x = GetImageImpl(RDMPConcept.ExtractableCohort, OverlayKind.Add);
                x.Mutate(x => x.Resize(16, 16));
                return x;
        }

        //This is special case when asking for icon for the Type, since the node itself is an IMasqueradeAs
        if (ReferenceEquals(concept, typeof(PipelineCompatibleWithUseCaseNode)))
        {
            x = GetImageImpl(RDMPConcept.Pipeline);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        foreach (var bmp in StateBasedIconProviders
                     .Select(stateBasedIconProvider => stateBasedIconProvider.GetImageIfSupportedObject(concept))
                     .Where(bmp => bmp != null))
        {
            x = GetImageImpl(bmp, kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        var conceptTypeName = concept.GetType().Name;

        RDMPConcept t;

        //It is a System.Type, get the name and see if there's a corresponding image
        if (concept is Type type)
            if (TryParseTypeNameToRdmpConcept(type, out t))
            {
                x = GetImageImpl(ImagesCollection[t], kind);
                x.Mutate(x => x.Resize(16, 16));
                return x;
            }

        //It is an instance of something, get the System.Type and see if there's a corresponding image
        if (Enum.TryParse(conceptTypeName, out t))
        {
            x = GetImageImpl(ImagesCollection[t], kind);
            x.Mutate(x => x.Resize(16, 16));
            return x;
        }

        switch (concept)
        {
            //if the object is masquerading as something else
            case IMasqueradeAs @as:
                {
                    //get what it's masquerading as
                    var masqueradingAs = @as.MasqueradingAs();

                    //provided we don't have a circular reference here!
                    if (masqueradingAs is not IMasqueradeAs)
                    {
                        x = GetImageImpl(masqueradingAs, kind); //get an image for what your pretending to be
                        x.Mutate(x => x.Resize(16, 16));
                        return x;
                    }
                    break;
                }
            case IAtomicCommand cmd:
                x = cmd.GetImage(this);
                x.Mutate(x => x.Resize(16, 16));
                return x;
        }


        return ImageUnknown;
    }

    private static bool TryParseTypeNameToRdmpConcept(Type type, out RDMPConcept t)
    {
        // is it a known Type like Project
        if (Enum.TryParse(type.Name, out t)) return true;

        // try trimming the I off of IProject
        if (type.IsInterface && Enum.TryParse(type.Name[1..], out t)) return true;

        // we don't have a known icon for the Type yet
        return false;
    }

    /// <inheritdoc/>
    public bool HasIcon(object o) => GetImage(o) != ImagesCollection[RDMPConcept.NoIconAvailable];

    public static RDMPConcept GetConceptForCollection(RDMPCollection rdmpCollection)
    {
        return rdmpCollection switch
        {
            RDMPCollection.None => RDMPConcept.NoIconAvailable,
            RDMPCollection.Tables => RDMPConcept.TableInfo,
            RDMPCollection.Catalogue => RDMPConcept.Catalogue,
            RDMPCollection.DataExport => RDMPConcept.Project,
            RDMPCollection.SavedCohorts => RDMPConcept.AllCohortsNode,
            RDMPCollection.Favourites => RDMPConcept.Favourite,
            RDMPCollection.Cohort => RDMPConcept.CohortIdentificationConfiguration,
            RDMPCollection.DataLoad => RDMPConcept.LoadMetadata,
            RDMPCollection.Datasets => RDMPConcept.Dataset,
            _ => throw new ArgumentOutOfRangeException(nameof(rdmpCollection))
        };
    }

    /// <summary>
    /// Returns true if the <paramref name="concept"/> is an instance of, System.Type or assignable to Type <paramref name="t"/>
    /// </summary>
    /// <param name="t"></param>
    /// <param name="concept"></param>
    /// <returns></returns>
    public static bool ConceptIs(Type t, object concept) =>
        t.IsInstanceOfType(concept) || (concept is Type type && t.IsAssignableFrom(type));


    /// <summary>
    /// Returns an image list dictionary with string keys that correspond to the names of all the RDMPConcept Enums.
    /// </summary>
    /// <param name="addFavouritesOverlayKeysToo">Pass true to also generate Images for every concept with a star overlay with the key being EnumNameFavourite (where EnumName is the RDMPConcept name e.g. CatalogueFavourite for the icon RDMPConcept.Catalogue and the favourite star)</param>
    /// <returns></returns>
    public Dictionary<string, Image<Rgba32>> GetImageList(bool addFavouritesOverlayKeysToo)
    {
        var imageList = new Dictionary<string, Image<Rgba32>>();

        foreach (RDMPConcept concept in Enum.GetValues(typeof(RDMPConcept)))
        {
            var img = GetImage(concept);
            imageList.Add(concept.ToString(), img);

            if (addFavouritesOverlayKeysToo)
                imageList.Add($"{concept}Favourite", IconOverlayProvider.GetOverlay(img, OverlayKind.FavouredItem));
        }

        return imageList;
    }

    private static Image<Rgba32> GetActualImage(Image<Rgba32> img, OverlayKind kind) =>
        kind == OverlayKind.None ? img : IconOverlayProvider.GetOverlay(img, kind);
}