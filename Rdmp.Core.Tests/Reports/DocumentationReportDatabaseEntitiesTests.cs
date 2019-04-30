using System.Drawing;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Reports;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.Icons.IconProvision;
using Tests.Common;

namespace Rdmp.Core.Tests.Reports
{
    class DocumentationReportDatabaseEntitiesTests:UnitTests
    {
        [Test]
        public void Test_DocumentationReportDatabaseEntities_Normal()
        {
            var store = new CommentStore();
            store.ReadComments(TestContext.CurrentContext.TestDirectory);

            SetupMEF();

            var reporter = new DocumentationReportDatabaseEntities();

            Bitmap bmp = new Bitmap(19,19);
            using(var g =Graphics.FromImage(bmp))
                g.DrawRectangle(new Pen(Color.DarkMagenta),5,5,5,5 );

            var iconProvider = Mock.Of<IIconProvider>(m=>m.GetImage(It.IsAny<object>(),It.IsAny<OverlayKind>()) == bmp);

            reporter.GenerateReport(store, new ThrowImmediatelyCheckNotifier(), iconProvider, MEF,false);


        }
    }
}
