using System.Collections.Generic;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Secondary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.Constraints.Primary
{
    
    class BoundsValidationIntegerTest
    {

        [Test]
        public void simple_integer_bounds()
        {
            var v = new Validator();

            var b = (BoundDouble)Validator.CreateConstraint("bounddouble",Consequence.Wrong);
            b.Lower = 5;
            b.Upper = 120;

            var i = new ItemValidator();
            i.AddSecondaryConstraint(b);
            v.AddItemValidator(i, "number", typeof(int));

            var d = new Dictionary<string, object>();
            d.Add("number", 119);

            Assert.IsNull(v.Validate(d));
        }
    }
}
