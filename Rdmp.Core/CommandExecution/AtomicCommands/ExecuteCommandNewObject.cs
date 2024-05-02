// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a new <see cref="DatabaseEntity" /> in the RDMP Platform database with the provided arguments.
/// </summary>
public class ExecuteCommandNewObject : BasicCommandExecution
{
    /// <summary>
    ///     The type of <see cref="DatabaseEntity" /> the user wants to construct
    /// </summary>
    private readonly Type _type;

    /// <summary>
    ///     if arguments are coming direct from the command line we can pull values from here otherwise we must prompt user for
    ///     those
    ///     values
    /// </summary>
    private readonly CommandLineObjectPicker _picker;

    private readonly Func<IMapsDirectlyToDatabaseTable> _func;

    /// <summary>
    ///     Interactive constructor, user will be prompted for values at execute time
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="type"></param>
    [UseWithObjectConstructor]
    public ExecuteCommandNewObject(IBasicActivateItems activator,
        [DemandsInitialization("Type to create", TypeOf = typeof(DatabaseEntity))]
        Type type) : base(activator)
    {
        if (!typeof(DatabaseEntity).IsAssignableFrom(type))
            SetImpossible("Type must be derived from DatabaseEntity");
        _type = type;
    }

    /// <summary>
    ///     Automatic/Unattended constructor, construction values will come from <paramref name="picker" /> and user will not
    ///     be
    ///     prompted for each constructor argument.
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="picker"></param>
    [UseWithCommandLine(
        ParameterHelpList = "<type> <arg1> <arg2> <etc>",
        ParameterHelpBreakdown = @"type	The object to create e.g. Catalogue
args    Dynamic list of values to satisfy the types constructor")]
    public ExecuteCommandNewObject(IBasicActivateItems activator, CommandLineObjectPicker picker) : base(activator)
    {
        if (!picker.HasArgumentOfType(0, typeof(Type)))
        {
            SetImpossible("First parameter must be a Type of DatabaseEntity");
        }
        else
        {
            _type = picker[0].Type;

            if (!typeof(DatabaseEntity).IsAssignableFrom(_type))
                SetImpossible("Type must be derived from DatabaseEntity");
        }

        _picker = picker;
    }

    /// <summary>
    ///     Create a new instance of an object using the provided func
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="callCtor"></param>
    public ExecuteCommandNewObject(IBasicActivateItems activator, Func<IMapsDirectlyToDatabaseTable> callCtor) :
        base(activator)
    {
        _func = callCtor;
    }

    public override void Execute()
    {
        base.Execute();

        IMapsDirectlyToDatabaseTable instance;
        if (_func != null)
            instance = _func();
        else
            instance = (DatabaseEntity)Construct(_type,
                //use the IRepository constructor of the _type
                () => ObjectConstructor.GetRepositoryConstructor(_type),
                //first argument is the Type, the rest are fed into the constructor of _type
                _picker?.Arguments?.Skip(1));

        if (instance == null)
            throw new Exception("Failed to construct object with provided parameters");

        Publish(instance);
    }
}