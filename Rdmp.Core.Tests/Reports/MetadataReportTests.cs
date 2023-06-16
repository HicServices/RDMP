// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports;

internal class MetadataReportTests:UnitTests
{
    [Test]
    public void Test_MetadataReport_Basic()
    {
        var cata = WhenIHaveA<Catalogue>();
        var reporter = new MetadataReport(Repository, new MetadataReportArgs(new[] { cata }));
        cata.Description = "The Quick Brown Fox Was Quicker Than The slow tortoise";

        //setup delegate for returning images
        var bmp = new Image<Rgba32>(200, 200);
        bmp.Mutate(x=>x.Fill(Color.Black,new RectangleF(10.0f,10.0f,50.0f,50.0f)));
            
        reporter.RequestCatalogueImages += (s) => { return new BitmapWithDescription[] {new(bmp,"MyPicture","Something interesting about it") }; };

        var file = reporter.GenerateWordFile(ThrowImmediatelyDataLoadEventListener.Quiet, false);
            
        Assert.IsNotNull(file);
        Assert.IsTrue(File.Exists(file.FullName));

        //refreshes the file stream status
        Assert.Greater(new FileInfo(file.FullName).Length, 0);
    }

    [Test]
    public void Test_OrphanExtractionInformation()
    {
        var ei = WhenIHaveA<ExtractionInformation>();

        //make it an orphan
        ei.CatalogueItem.ColumnInfo.DeleteInDatabase();
        ei.CatalogueItem.ColumnInfo_ID = null;
        ei.CatalogueItem.SaveToDatabase();
        ei.CatalogueItem.ClearAllInjections();
        ei.ClearAllInjections();

        var reporter = new MetadataReport(Repository,
            new MetadataReportArgs(new[] { ei.CatalogueItem.Catalogue })
        );
        var file = reporter.GenerateWordFile(ThrowImmediatelyDataLoadEventListener.Quiet, false);

        Assert.IsNotNull(file);
        Assert.IsTrue(File.Exists(file.FullName));
    }
}