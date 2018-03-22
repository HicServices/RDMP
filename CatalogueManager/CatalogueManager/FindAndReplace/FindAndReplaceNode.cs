using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.FindAndReplace
{
    internal class FindAndReplaceNode:IMasqueradeAs
    {
        private object _currentValue;
        public IMapsDirectlyToDatabaseTable Instance { get; set; }
        public PropertyInfo Property { get; set; }

        public FindAndReplaceNode(IMapsDirectlyToDatabaseTable instance, PropertyInfo property)
        {
            Instance = instance;
            Property = property;

            _currentValue = Property.GetValue(Instance);
        }

        public override string ToString()
        {
            return Instance.ToString();
        }

        public object MasqueradingAs()
        {
            return Instance;
        }

        public object GetCurrentValue()
        {
            return _currentValue;
        }

        public void SetValue(object newValue)
        {
            Property.SetValue(Instance,newValue);
            ((ISaveable)Instance).SaveToDatabase();
            _currentValue = newValue;
        }

        public void FindAndReplace(string find, string replace)
        {
            SetValue(_currentValue.ToString().Replace(find, replace));
        }
    }
}
