using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.Icons.IconOverlays;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class LoadStageNodeStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly ICoreIconProvider _iconProvider;

        public LoadStageNodeStateBasedIconProvider(ICoreIconProvider iconProvider)
        {
            _iconProvider = iconProvider;
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var node = o as LoadStageNode;

            if (o is LoadStage)
                return GetImageForStage((LoadStage) o);

            if (o is LoadBubble)
                return GetImageForStage(LoadStageToNamingConventionMapper.LoadBubbleToLoadStage((LoadBubble) o));

            if (node != null)
                return GetImageForStage(node.LoadStage);
            
            return null;
        }

        private Bitmap GetImageForStage(LoadStage loadStage)
        {
            switch (loadStage)
            {
                case LoadStage.GetFiles:
                    return _iconProvider.GetImage(RDMPConcept.GetFilesStage);
                case LoadStage.Mounting:
                    return _iconProvider.GetImage(RDMPConcept.LoadBubbleMounting);
                case LoadStage.AdjustRaw:
                    return _iconProvider.GetImage(RDMPConcept.LoadBubble);
                case LoadStage.AdjustStaging:
                    return _iconProvider.GetImage(RDMPConcept.LoadBubble);
                case LoadStage.PostLoad:
                    return _iconProvider.GetImage(RDMPConcept.LoadFinalDatabase);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}