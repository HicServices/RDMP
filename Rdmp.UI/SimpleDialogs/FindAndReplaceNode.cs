// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using FAnsi.Extensions;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.SimpleDialogs;

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
        PropertyName = $"{instance.GetType().Name}.{property.Name}";

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
        var current =_currentValue.ToString();
        if(current?.Contains(find,ignoreCase?StringComparison.CurrentCultureIgnoreCase:StringComparison.CurrentCulture)==true)
            SetValue(current.Replace(find, replace,ignoreCase ? StringComparison.CurrentCultureIgnoreCase:StringComparison.CurrentCulture));
    }
    #region Equality Members
    protected bool Equals(FindAndReplaceNode other)
    {
        return Instance.Equals(other.Instance) && Property.Equals(other.Property);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
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