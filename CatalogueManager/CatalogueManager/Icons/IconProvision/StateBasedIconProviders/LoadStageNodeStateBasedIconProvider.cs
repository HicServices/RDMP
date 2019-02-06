// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.DataLoad.Extensions;
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
                return GetImageForStage(((LoadBubble) o).ToLoadStage());

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