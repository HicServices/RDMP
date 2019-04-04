// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CommandLine;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.Rules
{
    abstract class BinderRule<T> : IBinderRule where T : IMapsDirectlyToDatabaseTable
    {
        protected readonly IActivateItems Activator;
        protected readonly T ToTest;
        public ErrorProvider ErrorProvider { get; private set; }
        protected readonly Func<T, object> PropertyToCheck;
        protected readonly Control Control;

        protected BinderRule(IActivateItems activator, T toTest, Func<T, object> propertyToCheck, Control control)
        {
            ErrorProvider = new ErrorProvider();
            Activator = activator;
            ToTest = toTest;
            PropertyToCheck = propertyToCheck;
            Control = control;

            activator.OnRuleRegistered(this);

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