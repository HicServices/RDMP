// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Moq;
using NUnit.Framework;
using Rdmp.Core.Reports;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Tests.Common;
using Color = SixLabors.ImageSharp.Color;

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

            Image img = new Image<Argb32>(19, 19);
            img.Mutate(x=>x.Fill(Color.DarkMagenta));

            var iconProvider = Mock.Of<IIconProvider>(m=>m.GetImage(It.IsAny<object>(),It.IsAny<OverlayKind>()) == img);

            reporter.GenerateReport(store, new ThrowImmediatelyCheckNotifier(), iconProvider, MEF,false);


        }
    }
}
