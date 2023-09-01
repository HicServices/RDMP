// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using System.IO;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.Data;

internal class ExtractableCohortAuditLogBuilderTests : UnitTests
{
        [Test]
        public void AuditLogReFetch_FileInfo()
        {

                var fi = new FileInfo("durdur.txt");
                var desc = ExtractableCohortAuditLogBuilder.GetDescription(fi);

                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns(desc);
                var fi2 = ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator);

                Assert.IsNotNull(fi2);
                Assert.IsInstanceOf<FileInfo>(fi2);
                Assert.AreEqual(fi.FullName, ((FileInfo)fi2).FullName);
        }


        [Test]
        public void AuditLogReFetch_CohortIdentificationConfiguration()
        {

                var cic = WhenIHaveA<CohortIdentificationConfiguration>();
                var desc = ExtractableCohortAuditLogBuilder.GetDescription(cic);

                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns(desc);
                var cic2 = ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator);
                Assert.IsNotNull(cic2);
                Assert.IsInstanceOf<CohortIdentificationConfiguration>(cic2);
                Assert.AreEqual(cic, cic2);
        }

        [Test]
        public void AuditLogReFetch_ExtractionInformation()
        {

                var ei = WhenIHaveA<ExtractionInformation>();
                var desc = ExtractableCohortAuditLogBuilder.GetDescription(ei);

                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns(desc);
                var ei2 = ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator);
                Assert.IsNotNull(ei2);
                Assert.IsInstanceOf<ExtractionInformation>(ei2);
                Assert.AreEqual(ei, ei2);
        }

        [Test]
        public void AuditLogReFetch_WhenAuditLogIsNull()
        {
                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns(x => null);
                Assert.IsNull(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator));
        }

        [Test]
        public void AuditLogReFetch_WhenAuditLogIsRubbish()
        {
                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns("troll doll dur I invented this cohort myself");
                Assert.IsNull(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator));
        }

        [Test]
        public void AuditLogReFetch_WhenSourceIsDeleted()
        {
                _ = new ExtractableCohortAuditLogBuilder();

                var ei = WhenIHaveA<ExtractionInformation>();
                var desc = ExtractableCohortAuditLogBuilder.GetDescription(ei);

                var moqCohort = Substitute.For<IExtractableCohort>();
                moqCohort.AuditLog.Returns(desc);

                // delete the source
                ei.DeleteInDatabase();

                // should now return null
                Assert.IsNull(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator));
        }
}