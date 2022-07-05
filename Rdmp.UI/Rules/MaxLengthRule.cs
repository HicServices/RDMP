// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Rules
{
    /// <summary>
    /// Checks the database for the column that backs the given property and then
    /// reports an error if it is too long to fit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class MaxLengthRule<T> : BinderRule<T> where T : IMapsDirectlyToDatabaseTable
    {
        private string _problemDescription;
        private int? _maxLength;

        public MaxLengthRule(IActivateItems activator, T toTest, Func<T, object> propertyToCheck, Control control, string propertyToCheckName)
            : base(activator, toTest, propertyToCheck, control, propertyToCheckName)
        {
            _problemDescription = "Value is too long";

            _maxLength = TryGetMaxLengthFrom(activator.RepositoryLocator.CatalogueRepository, toTest) ??
            TryGetMaxLengthFrom(activator.RepositoryLocator.DataExportRepository, toTest);
        }

        private int? TryGetMaxLengthFrom(IRepository repo, T toTest)
        {
            if(repo is not TableRepository tr)
            {
                return null;
            }

            if (!tr.SupportsObjectType(toTest.GetType()))
            {
                return null;
            }

            var table = tr.DiscoveredServer.GetCurrentDatabase().ExpectTable(toTest.GetType().Name);

            if(!table.Exists())
            {
                return null;
            }
            try
            {
                var col = table.DiscoverColumn(PropertyToCheckName);
                
                return col.DataType.GetCSharpDataType() == typeof(string) ? 
                    col.DataType.GetLengthIfString() : 
                    null;
            }
            catch (Exception)
            {

                return null;
            }
        }
        protected override string IsValid(object currentValue, Type typeToTest)
        {
            //never check null/empty values
            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString()))
                return null;

            if (_maxLength.HasValue && currentValue is string s)
            {
                if(s.Length > _maxLength.Value)
                {
                    return _problemDescription;
                }
            }

            return null;
        }
    }
}