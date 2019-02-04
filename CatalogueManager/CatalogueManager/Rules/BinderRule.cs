using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    abstract class BinderRule<T> where T : IMapsDirectlyToDatabaseTable
    {
        protected readonly IActivateItems Activator;
        protected readonly T ToTest;
        protected readonly ErrorProvider ErrorProvider = new ErrorProvider();
        protected readonly Func<T, object> PropertyToCheck;
        protected readonly Control Control;

        public BinderRule(IActivateItems activator, T toTest, Func<T, object> propertyToCheck, Control control)
        {
            Activator = activator;
            ToTest = toTest;
            PropertyToCheck = propertyToCheck;
            Control = control;

            toTest.PropertyChanged += ToTest_PropertyChanged;
        }

        private void ToTest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var currentValue = PropertyToCheck(ToTest);
            var typeToTest = ToTest.GetType();

            string valid = IsValid(currentValue, typeToTest);

            if (!string.IsNullOrWhiteSpace(valid))
                ErrorProvider.SetError(Control, valid);
            else
                ErrorProvider.Clear(); //No error
        }

        /// <summary>
        /// Return null if the <paramref name="currentValue"/> is valid or a message describing the problem
        /// if it is not.
        /// </summary>
        /// <param name="currentValue"></param>
        /// <param name="typeToTest"></param>
        /// <returns></returns>
        protected abstract string IsValid(object currentValue, Type typeToTest);
    }
}