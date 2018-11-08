using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Extensions;

namespace CatalogueManager.FindAndReplace
{
    internal class FindAndReplaceNode:IMasqueradeAs
    {
        private object _currentValue;
        public IMapsDirectlyToDatabaseTable Instance { get; set; }
        public PropertyInfo Property { get; set; }
        public string PropertyName { get; private set; }

        public FindAndReplaceNode(IMapsDirectlyToDatabaseTable instance, PropertyInfo property)
        {
            Instance = instance;
            Property = property;
            PropertyName = instance.GetType().Name + "." + property.Name;

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

        public void FindAndReplace(string find, string replace, bool ignoreCase)
        {
            if(_currentValue.ToString().Contains(find,ignoreCase? CompareOptions.IgnoreCase:CompareOptions.None ))
                SetValue(_currentValue.ToString().Replace(find, replace,ignoreCase ? RegexOptions.IgnoreCase: RegexOptions.None));
        }
        #region Equality Members
        protected bool Equals(FindAndReplaceNode other)
        {
            return Instance.Equals(other.Instance) && Property.Equals(other.Property);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FindAndReplaceNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Instance.GetHashCode()*397) ^ Property.GetHashCode();
            }
        }
        #endregion
    }
}
