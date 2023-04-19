// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of Array types TableInfo[], Catalogue[] etc</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueArrayUI : UserControl, IArgumentValueUI
{
    private readonly IActivateItems _activator;
    private ArgumentValueUIArgs _args;
    private Array _value;

    public ArgumentValueArrayUI(IActivateItems activator)
    {
        _activator = activator;

        InitializeComponent();
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _args = args;

        var value = ((Array) (args.InitialValue));
        SetUp(value);
    }

    private void SetUp(Array value)
    {
        _value = value;

        if (value == null)
            tbArray.Text = "";
        else
        {
            StringBuilder sb = new StringBuilder();

            var e = value.GetEnumerator();
            while (e.MoveNext())
            {
                sb.Append(e.Current);
                sb.Append(",");
            }

            tbArray.Text = sb.ToString().TrimEnd(',');
        }

        tbArray.ReadOnly = true;
    }

    private void btnPickDatabaseEntities_Click(object sender, EventArgs e)
    {
        var type = _args.Type;
        var elementType = type.GetElementType();

        if(elementType == null)
            throw new NotSupportedException("No array element existed for DemandsInitialization Type " + type);

        if (!_args.CatalogueRepository.SupportsObjectType(elementType))
            throw new NotSupportedException("CatalogueRepository does not support element "+elementType+" for DemandsInitialization Type " + type);

        if(_activator.SelectObjects(new DialogArgs
           {
               TaskDescription = $"Which objects should be selected for Argument '{_args.Required.Name}'",
               InitialObjectSelection = _value is IEnumerable<IMapsDirectlyToDatabaseTable> v ? v.ToArray() : null,
               AllowSelectingNull = true
           }, _args.CatalogueRepository.GetAllObjects(elementType).ToArray(), out var selected))
        {
            _args.Setter(selected);
            SetUp(selected);
        }
    }
}