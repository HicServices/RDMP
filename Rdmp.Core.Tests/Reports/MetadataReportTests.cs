using System.Drawing;
using System.IO;
using NPOI.XWPF.UserModel;
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
