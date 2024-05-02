// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Adds a new SqlParameter to an <see cref="ICollectSqlParameters" />
/// </summary>
public class ExecuteCommandAddParameter : BasicCommandExecution, IAtomicCommand
{
    private readonly ICollectSqlParameters _collector;
    private readonly string _parameterName;
    private readonly string _datatype;
    private readonly string _value;
    private const float DEFAULT_WEIGHT = 2.1f;

    public ExecuteCommandAddParameter(IBasicActivateItems activator, ICollectSqlParameters collector,
        string parameterName, string datatype, string value) : base(activator)
    {
        Weight = DEFAULT_WEIGHT;
        _collector = collector;
        _parameterName = parameterName;
        _datatype = datatype;
        _value = value;
        UseTripleDotSuffix = true;

        if (collector is IMightBeReadOnly r) SetImpossibleIfReadonly(r);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ParametersNode);
    }

    public override void Execute()
    {
        var factory = new ParameterCollectionUIOptionsFactory();
        var options = factory.Create(_collector, BasicActivator.CoreChildProvider);

        var n = _parameterName;
        var d = _datatype;
        var v = _value;

        if (n == null)
        {
            // get user to type the name of the parameter
            if (BasicActivator.TypeText(new DialogArgs
                {
                    EntryLabel = "Name",
                    TaskDescription =
                        "A name is required for the parameter.  It must start with '@' e.g. @myparameter.  Do not add spaces or start the name with a number.",
                    WindowTitle = "Add Parameter"
                }, 99, "@myVariable", out var name, false))
                // user did type a name
                n = name;
            else
                // user cancelled typing the parameter name
                return;
        }


        if (d == null)
        {
            // get user to type the name of the parameter
            if (BasicActivator.TypeText(new DialogArgs
                {
                    EntryLabel = "DataType",
                    TaskDescription = "What data type are you storing in the parameter (e.g. datetime2)",
                    WindowTitle = "Parameter Data Type"
                }, 99, "varchar(10)", out var datatype, false))
                // user did type
                d = datatype;
            else
                // user cancelled typing
                return;
        }

        if (v == null)
        {
            // get user to type the name of the parameter
            if (BasicActivator.TypeText(new DialogArgs
                {
                    EntryLabel = "Value",
                    TaskDescription =
                        "What value should the parameter have.  Ensure if you are using text that it is appropriately quoted",
                    WindowTitle = "Parameter Value"
                }, int.MaxValue, AnyTableSqlParameter.DefaultValue, out var value, false))
                // user did type
                v = value;
            else
                // user cancelled typing
                return;
        }

        var p = options.CreateNewParameter(n);
        p.ParameterSQL = $"DECLARE {p.ParameterName} as {d}";
        p.Value = v;
        p.SaveToDatabase();

        if (_collector is DatabaseEntity de) Publish(de);
    }
}