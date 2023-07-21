// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.Versioning;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.Tests;

[SupportedOSPlatform("windows7.0")]
internal class CatalogueIconProviderTests: UITests
{

    [Test]
    public void CatalogueIconProvider_HasImage_NoImage()
    {
        var provider = new CatalogueIconProvider(RepositoryLocator,null);

        var img = provider.GetImage(new object(), OverlayKind.None);

        Assert.IsFalse(provider.HasIcon(new object()));
    }

    [Test]
    public void CatalogueIconProvider_HasImage_AllObjectsHave()
    {
        var objectCount = 0;
        var provider = new DataExportIconProvider(RepositoryLocator,null);
            
        foreach (var obj in WhenIHaveAll())
        {
            var img = provider.GetImage(obj, OverlayKind.None);

            if (obj is IDisableable d)
            {
                d.IsDisabled = true;

                Assert.IsTrue(IsBlackAndWhite(provider.GetImage(obj,OverlayKind.Add)),$"Grayscaling failed for Object of Type '{obj.GetType().Name}' did not have an image");
                    
                d.IsDisabled = false;
                Assert.IsFalse(IsBlackAndWhite(provider.GetImage(obj,OverlayKind.Add)),$"Enabled Object of Type '{obj.GetType().Name}' was unexpectedly Grayscale");
            }
                    
            Assert.IsTrue(provider.HasIcon(obj),$"Object of Type '{obj.GetType().Name}' did not have an image");
            objectCount++;
        }

        Console.WriteLine($"Generated images for {objectCount} objects");
    }


    [Test]
    public void TestGrayscale()
    {
        var provider = new CatalogueIconProvider(RepositoryLocator,null);

        var ac = WhenIHaveA<AggregateConfiguration>();

        Assert.IsFalse(IsBlackAndWhite(provider.GetImage(ac)),"Image was unexpectedly Grayscale");

        ac.IsDisabled = true;
        Assert.IsTrue(IsBlackAndWhite(provider.GetImage(ac)),"Image was expected to be Grayscale but wasn't'");
    }

        
    /// <summary>
    /// Exposes a potential infinite loop / stack overflow where an object is masquerading as an IMasquerade
    /// </summary>
    [Test]
    public void Test_ObjectMasqueradingAsSelf()
    {
        var me = new IAmMe();

        var provider = new CatalogueIconProvider(RepositoryLocator,null);
        provider.GetImage(me, OverlayKind.Add);

        Assert.IsFalse(provider.HasIcon(me));
    }
    private class IAmMe : IMasqueradeAs
    {
        public object MasqueradingAs()
        {
            return this;
        }
    }


    private static bool IsBlackAndWhite(SixLabors.ImageSharp.Image<Rgba32> img)
    {
        var foundColoured = false;
        img.ProcessPixelRows(pixels =>
        {
            for (var y = 0; y < pixels.Height; y++)
            {
                foreach (ref var pixel in pixels.GetRowSpan(y))
                {
                    if (pixel.R != pixel.G || pixel.G != pixel.B)
                    {
                        foundColoured = true;
                        return;
                    }
                }
            }
        });
        return !foundColoured;
    }


}