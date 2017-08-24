using System.Linq;
using CatalogueLibrary.Data;
using HIC.Common.Validation;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.Validation
{
    public class StandardRegexTests:DatabaseTests
    {
        [Test]
        public void CreateNew_UseConstraint()
        {
            // Clean up any existing regexes
            CatalogueRepository.GetAllObjects<StandardRegex>("WHERE ConceptName = 'Fish'").ToList().ForEach(r => r.DeleteInDatabase());

            var regex = new StandardRegex(CatalogueRepository);
            try
            {
                Assert.IsNotNull(regex.ConceptName);
                Assert.IsNullOrEmpty(regex.Description);

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
