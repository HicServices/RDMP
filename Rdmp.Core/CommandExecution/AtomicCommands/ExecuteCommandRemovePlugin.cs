// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using NLog;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Deletes a plugin from RDMP.
/// </summary>
/// 
[Alias("dp")]
[Alias("DeletePlugin")]
public class ExecuteCommandRemovePlugin : BasicCommandExecution, IAtomicCommand
{
    private LoadModuleAssembly _assembly;


    [UseWithObjectConstructor]
    public ExecuteCommandRemovePlugin(LoadModuleAssembly assembly)
    {
        _assembly = assembly;
    }

    /// <summary>
    /// Interactive constructor
    /// </summary>
    /// <param name="activator"></param>
    public ExecuteCommandRemovePlugin(IBasicActivateItems activator) : base(activator)
    {
        //todo
        Console.WriteLine("yeehaw");
    }


    public override void Execute()
    {
        base.Execute();
        _assembly.Delete();
    }

}