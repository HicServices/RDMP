using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Annotations;

namespace CatalogueManager.Rules
{
    public class BinderWithErrorProviderFactory
    {
        private readonly IActivateItems _activator;

        public BinderWithErrorProviderFactory(IActivateItems activator)
        {
            _activator = activator;
        }

        public void Bind<T>(Control c, string propertyName, T databaseObject, string dataMember, bool formattingEnabled, DataSourceUpdateMode updateMode,Func<T,object> getter) where T:IMapsDirectlyToDatabaseTable
        {
            c.DataBindings.Clear();
            c.DataBindings.Add(propertyName, databaseObject, dataMember, formattingEnabled, updateMode);

            var property = databaseObject.GetType().GetProperty(dataMember);

            if (property.GetCustomAttributes(typeof (UniqueAttribute), true).Any())
                new UniqueRule<T>(_activator, databaseObject, getter, c);
        }

        private class UniqueRule<T> where T : IMapsDirectlyToDatabaseTable
        {
            private readonly IActivateItems _activator;
            private readonly T _toTest;
            private readonly string _problemDescription;
            readonly ErrorProvider _errorProvider = new ErrorProvider();
            private readonly Func<T, object> _propertyToCheck;
            private readonly Control _control;

            public UniqueRule(IActivateItems activator, T toTest, Func<T,object> propertyToCheck,Control control)
            {
                _activator = activator;
                _toTest = toTest;
                _propertyToCheck = propertyToCheck;
                _control = control;
                _problemDescription = "Must be unique amongst all " + toTest.GetType().Name  +"s";

                toTest.PropertyChanged += toTest_PropertyChanged;
            }

            void toTest_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                _currentValue = _propertyToCheck(_toTest);
                var typeToTest = _toTest.GetType();

                //never check for uniqueness on null values
                if(_currentValue == null || string.IsNullOrWhiteSpace(_currentValue.ToString()))
                    return;

                //get all other objects which share our Type and contain equal values
                if (_activator.CoreChildProvider.GetAllSearchables().Keys.OfType<T>().Except(new[] { _toTest }).Where(t=>t.GetType() == typeToTest).Any(AreEqual))
                    _errorProvider.SetError(_control, _problemDescription);
                else
                    _errorProvider.Clear(); //No error
            }

            private object _currentValue;

            private bool AreEqual(T arg)
            {
                string s = _currentValue as string;
                
                if (s != null)
                    return string.Equals(s, _propertyToCheck(arg) as string, StringComparison.CurrentCultureIgnoreCase);

                return Equals(_currentValue, _propertyToCheck(arg));
            }
        }

    }
}