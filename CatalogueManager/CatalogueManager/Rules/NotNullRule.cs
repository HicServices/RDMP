using System;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    class NotNullRule<T> : BinderRule<T> where T : IMapsDirectlyToDatabaseTable
    {
        public NotNullRule(IActivateItems activator, T databaseObject, Func<T, object> getter, Control control) : base(activator,databaseObject,getter,control)
        {
            
        }

        protected override string IsValid(object currentValue, Type typeToTest)
        {
            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                return "Value cannot be null";

            return null;
        }
    }
}