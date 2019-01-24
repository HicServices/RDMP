using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    public class RuleBasedErrorProvider
    {
        private readonly IActivateItems _activator;
        
        public RuleBasedErrorProvider(IActivateItems activator)
        {
            _activator = activator;
        }

        public void EnsureNameUnique(Control control, INamed named)
        {
            new UniqueRule<INamed>( _activator, named, (o)=>o.Name, control);
        }

        public void EnsureAcronymUnique(Control control, ICatalogue catalogue)
        {
            new UniqueRule<ICatalogue>(_activator, catalogue, (o)=>o.Acronym,control);
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

                //get all other objects which share our Type and contain equal values
                if (_activator.CoreChildProvider.GetAllSearchables().Keys.OfType<T>().Except(new[] { _toTest }).Any(AreEqual))
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