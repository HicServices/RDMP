// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.IO;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Reports;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports
{
    class MetadataReportTests:UnitTests
    {
        [Test]
        public void Test_MetadataReport_Basic()
        {
            var cata = WhenIHaveA<Catalogue>();
            var reporter = new MetadataReport(Repository, new MetadataReportArgs(new[] {cata}));
            cata.Description = "The Quick Brown Fox Was Quicker Than The slow tortoise";

            //setup delegate for returning images
            var bmp = new Bitmap(200,500);
            using (var g = Graphics.FromImage(bmp))
                g.DrawRectangle(new Pen(Color.Black),10,10,50,50);
            
            reporter.RequestCatalogueImages += (s) => { return new BitmapWithDescription[] {new BitmapWithDescription(bmp,"MyPicture","Something interesting about it"),  }; };

            var file = reporter.GenerateWordFile(new ThrowImmediatelyDataLoadEventListener(), false);
            
            Assert.IsNotNull(file);
            Assert.IsTrue(File.Exists(file.FullName));
            
            //refreshes the file stream status
            Assert.Greater(new FileInfo(file.FullName).Length,0);
        }
    }
}
