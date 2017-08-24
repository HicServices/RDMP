using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIC.Common.Validation.Constraints;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests.ValidationPluginTests
{
    public class PluginValidationSerializationTest
    {
        [Test]
        public void TestSerialization()
        {
            Validator v = new Validator();
            var iv = new ItemValidator("fish");
            iv.PrimaryConstraint = new FishConstraint();

            //validation should be working
            Assert.IsNull(iv.ValidateAll("Fish", new object[0], new string[0]));
            Assert.IsNotNull(iv.ValidateAll("Potato", new object[0], new string[0]));

            v.ItemValidators.Add(iv);

            Assert.AreEqual(1, v.ItemValidators.Count);
            Assert.AreEqual(typeof(FishConstraint), v.ItemValidators[0].PrimaryConstraint.GetType());

            string xml = v.SaveToXml();

            var newV = Validator.LoadFromXml(xml);

            Assert.AreEqual(1,newV.ItemValidators.Count);
            Assert.AreEqual(typeof(FishConstraint), newV.ItemValidators[0].PrimaryConstraint.GetType());

        }
    }

    public class FishConstraint : PluginPrimaryConstraint
    {
        public override void RenameColumn(string originalName, string newName)
        {
            
        }

        public override string GetHumanReadableDescriptionOfValidation()
        {
            return "Fish Constraint For Testing";
        }

        public override ValidationFailure Validate(object value)
        {
            if (value == null)
                return null;

            string result = value as string ?? value.ToString();

            if (result.Equals("Fish"))
                return null;

            return new ValidationFailure("Value '" + value +"' was not 'Fish'!",this);
        }
    }
}
