using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Windows.Forms;
using CachingEngine;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision.Exceptions;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.PluginChildProvision;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Icons.IconProvision;
using ScintillaNET;

namespace CatalogueManager.Icons.IconProvision
{
    public class CatalogueIconProvider: ICoreIconProvider
    {
        private readonly IIconProvider[] _pluginIconProviders;
        public IconOverlayProvider OverlayProvider { get; private set; }
        
        protected List<IObjectStateBasedIconProvider> StateBasedIconProviders = new List<IObjectStateBasedIconProvider>();

        protected readonly EnumImageCollection<RDMPConcept> ImagesCollection;

        /// <summary>
        /// This special snowflake needs to know lots of complex info about the environment it is in that is expensive to compute so this cached knowledge for all CatalogueItems gets passed in,
        /// and must be regularly refreshed - see SetClassifications method
        /// </summary>
        private CatalogueItemStateBasedIconProvider _catalogueItemStateBasedIconProvider;

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
            StateBasedIconProviders.Add(new CatalogueItemIssueStateBasedIconProvider());
            StateBasedIconProviders.Add(new ColumnInfoStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new TableInfoStateBasedIconProvider());
            StateBasedIconProviders.Add(new AggregateConfigurationStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new CohortIdentificationConfigurationStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExternalDatabaseServerStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new LoadStageNodeStateBasedIconProvider(this));
            StateBasedIconProviders.Add(new ProcessTaskStateBasedIconProvider());
            StateBasedIconProviders.Add(new HICProjectDirectoryStateBasedIconProvider(OverlayProvider));
            StateBasedIconProviders.Add(new TableInfoServerNodeStateBasedIconProvider(OverlayProvider));
            
            _catalogueItemStateBasedIconProvider = new CatalogueItemStateBasedIconProvider(OverlayProvider);
            StateBasedIconProviders.Add(_catalogueItemStateBasedIconProvider);
        }
        
        public void SetClassifications(Dictionary<int, CatalogueItemClassification> catalogueItemClassifications)
        {
            _catalogueItemStateBasedIconProvider.SetClassifications(catalogueItemClassifications);
        }

        public virtual Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if(IsLockedLockable(concept) && (kind == OverlayKind.None || kind == OverlayKind.Link))
                kind = OverlayKind.Locked;

            //the only valid strings are "Catalogue" etc where the value exactly maps to an RDMPConcept
            if(concept is string)
            {
                RDMPConcept result;
                if (RDMPConcept.TryParse((string) concept, true, out result)) 
                    concept = result;
                else 
                    return null; //it's a string but an unhandled one so give them null back
            }

            //if there are plugins injecting random objects into RDMP tree views etc then we need the ability to provide icons for them
            if (_pluginIconProviders != null)
                foreach (IIconProvider plugin in _pluginIconProviders)
                {
                    var img = plugin.GetImage(concept, kind);
                    if (img != null)
                        return img;
                }

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
            
            if (concept is PermissionWindowUsedByCacheProgressNode)
                return GetImage(((PermissionWindowUsedByCacheProgressNode)concept).PermissionWindow, OverlayKind.Link);

            foreach (var stateBasedIconProvider in StateBasedIconProviders)
            {
                var bmp = stateBasedIconProvider.GetImageIfSupportedObject(concept);
                if (bmp != null)
                    return GetImage(bmp,kind);
            }

            string conceptTypeName = concept.GetType().Name;
            
            RDMPConcept t;

            if(Enum.TryParse(conceptTypeName,out t))
                return GetImage(ImagesCollection[t],kind);

            return ImagesCollection[RDMPConcept.NoIconAvailable];
            
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

        private bool IsLockedLockable(object concept)
        {
            var lockable = concept as ILockable;
            return lockable != null && lockable.LockedBecauseRunning;
        }

        /// <summary>
        /// Returns an image list dictionary with string keys that correspond to the names of all the RDMPConcept Enums.
        /// </summary>
        /// <param name="addFavouritesOverlayKeysToo">Pass true to also generate Images for every concept with a star overlay with the key being EnumNameFavourite (where EnumName is the RDMPConcept name)</param>
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
