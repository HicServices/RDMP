// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints.Secondary;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.Validation
{
    public class StandardRegexTests:DatabaseTests
    {
        [Test]
        public void CreateNew_UseConstraint()
        {
            // Clean SetUp any existing regexes
            CatalogueRepository.GetAllObjects<StandardRegex>().Where(r=>r.ConceptName == "Fish").ToList().ForEach(r => r.DeleteInDatabase());

            var regex = new StandardRegex(CatalogueRepository);
            try
            {
                Assert.IsNotNull(regex.ConceptName);
                Assert.IsTrue(string.IsNullOrEmpty(regex.Description));

                regex.ConceptName = "Fish";
                regex.Regex = "^(Fish)$";
                regex.SaveToDatabase();

                StandardRegexConstraint constraint = new StandardRegexConstraint(CatalogueRepository);
                
                constraint.CatalogueStandardRegex = regex;
                
                Assert.IsNull(constraint.Validate("Fish",null,null));
                ValidationFailure failure = constraint.Validate("FishFingers", null, null);
                Assert.IsNotNull(failure);
            }
            finally
            {
                regex.DeleteInDatabase();
            }
        }
    }
}
