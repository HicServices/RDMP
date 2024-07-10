// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Rules;

/// <summary>
/// Factory for generating <see cref="Binding"/>s for a <see cref="Control"/> and automatically configuring error providers based on
/// whether the bound property has relevant attributes (e.g. <see cref="UniqueAttribute"/>, <see cref="NotNullAttribute"/>).
/// </summary>
public class BinderWithErrorProviderFactory
{
    private readonly IActivateItems _activator;

    public BinderWithErrorProviderFactory(IActivateItems activator)
    {
        _activator = activator;
    }

    public void Bind<T>(Control c, string propertyName, T databaseObject, string dataMember, bool formattingEnabled,
        DataSourceUpdateMode updateMode, Func<T, object> getter) where T : IMapsDirectlyToDatabaseTable
    {
        c.DataBindings.Clear();
        var dataBinding = c.DataBindings.Add(propertyName, databaseObject, dataMember, formattingEnabled, updateMode);
        dataBinding.BindingComplete += new BindingCompleteEventHandler(BindingHandler);
        var property = databaseObject.GetType().GetProperty(dataMember);

        if (property.GetCustomAttributes(typeof(UniqueAttribute), true).Any())
            new UniqueRule<T>(_activator, databaseObject, getter, c, dataMember);

        if (property.GetCustomAttributes(typeof(NotNullAttribute), true).Any())
            new NotNullRule<T>(_activator, databaseObject, getter, c, dataMember);

        if (property.PropertyType == typeof(string))
            new MaxLengthRule<T>(_activator, databaseObject, getter, c, dataMember);

        if (dataMember.Equals("Name") && databaseObject is INamed)
            new NoBadNamesRule<T>(_activator, databaseObject, getter, c, dataMember);
    }

    private void BindingHandler(object sender, BindingCompleteEventArgs e)
    {
        if (e.BindingCompleteState != BindingCompleteState.Success)
            MessageBox.Show("Error: " + e.ErrorText);
    }
}