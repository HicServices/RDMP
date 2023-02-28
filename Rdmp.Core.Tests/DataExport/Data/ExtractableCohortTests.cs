// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Settings;
using System;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Data
{
    class ExtractableCohortTests : TestsRequiringACohort
    {
        [Test]
        public void TestExtractableCohort_Identifiable()
        {
            var cohort = _extractableCohort;

            Assert.IsNotNull(cohort.GetPrivateIdentifier());
            Assert.AreNotEqual(cohort.GetReleaseIdentifier(),cohort.GetPrivateIdentifier());

            var ect = cohort.ExternalCohortTable;
            ect.ReleaseIdentifierField = ect.PrivateIdentifierField;
            ect.SaveToDatabase();

            UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Fail);

            var ex = Assert.Throws<Exception>(()=>cohort.GetReleaseIdentifier());

            Assert.AreEqual("R004 PrivateIdentifierField and ReleaseIdentifierField are the same, this means your cohort will extract identifiable data (no cohort identifier substitution takes place)", ex.Message);

            UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Warning);

            Assert.AreEqual(cohort.GetReleaseIdentifier(),cohort.GetPrivateIdentifier());

            UserSettings.SetErrorReportingLevelFor(ErrorCodes.ExtractionIsIdentifiable, CheckResult.Fail);
        }
    }
}
