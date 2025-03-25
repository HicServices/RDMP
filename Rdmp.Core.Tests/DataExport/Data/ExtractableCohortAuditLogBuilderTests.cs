// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
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

        Assert.That(fi2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(fi2, Is.InstanceOf<FileInfo>());
            Assert.That(((FileInfo)fi2).FullName, Is.EqualTo(fi.FullName));
        });
    }


    [Test]
    public void AuditLogReFetch_CohortIdentificationConfiguration()
    {
        var cic = WhenIHaveA<CohortIdentificationConfiguration>();
        var desc = ExtractableCohortAuditLogBuilder.GetDescription(cic);

        var moqCohort = Substitute.For<IExtractableCohort>();
        moqCohort.AuditLog.Returns(desc);
        var cic2 = ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator);
        Assert.That(cic2, Is.Not.Null);
        Assert.That(cic2, Is.InstanceOf<CohortIdentificationConfiguration>());
        Assert.That(cic2, Is.EqualTo(cic));
    }

    [Test]
    public void AuditLogReFetch_ExtractionInformation()
    {
        var ei = WhenIHaveA<ExtractionInformation>();
        var desc = ExtractableCohortAuditLogBuilder.GetDescription(ei);

        var moqCohort = Substitute.For<IExtractableCohort>();
        moqCohort.AuditLog.Returns(desc);
        var ei2 = ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator);
        Assert.That(ei2, Is.Not.Null);
        Assert.That(ei2, Is.InstanceOf<ExtractionInformation>());
        Assert.That(ei2, Is.EqualTo(ei));
    }

    [Test]
    public void AuditLogReFetch_WhenAuditLogIsNull()
    {
        var moqCohort = Substitute.For<IExtractableCohort>();
        moqCohort.AuditLog.Returns(x => null);
        Assert.That(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator), Is.Null);
    }

    [Test]
    public void AuditLogReFetch_WhenAuditLogIsRubbish()
    {
        var moqCohort = Substitute.For<IExtractableCohort>();
        moqCohort.AuditLog.Returns("troll doll dur I invented this cohort myself");
        Assert.That(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator), Is.Null);
    }

    [Test]
    public void AuditLogReFetch_WhenSourceIsDeleted()
    {
        var ei = WhenIHaveA<ExtractionInformation>();
        var desc = ExtractableCohortAuditLogBuilder.GetDescription(ei);

        var moqCohort = Substitute.For<IExtractableCohort>();
        moqCohort.AuditLog.Returns(desc);

        // delete the source
        ei.DeleteInDatabase();

        // should now return null
        Assert.That(ExtractableCohortAuditLogBuilder.GetObjectIfAny(moqCohort, RepositoryLocator), Is.Null);
    }
}