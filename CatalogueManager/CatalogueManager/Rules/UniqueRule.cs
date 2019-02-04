using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    class UniqueRule<T> : BinderRule<T> where T : IMapsDirectlyToDatabaseTable
    {
        private readonly string _problemDescription;

        public UniqueRule(IActivateItems activator, T toTest, Func<T, object> propertyToCheck, Control control)
            : base(activator, toTest, propertyToCheck, control)
        {
            _problemDescription = "Must be unique amongst all " + toTest.GetType().Name + "s";

        }

        protected override string IsValid(object currentValue, Type typeToTest)
        {
            //never check for uniqueness on null values
            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                return null;

            if (
                Activator.CoreChildProvider.GetAllSearchables()
                    .Keys.OfType<T>()
                    .Except(new[] { ToTest })
                    .Where(t => t.GetType() == typeToTest)
                    .Any(v => AreEqual(v, currentValue)))
                return _problemDescription;

            return null;
        }

        private bool AreEqual(T arg, object currentValue)
        {
            string s = currentValue as string;

            if (s != null)
                return string.Equals(s, PropertyToCheck(arg) as string, StringComparison.CurrentCultureIgnoreCase);

            return Equals(currentValue, PropertyToCheck(arg));
        }
    }
}