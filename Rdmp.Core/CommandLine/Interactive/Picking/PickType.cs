// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

internal partial class PickType : PickObjectBase
{
    private static readonly Regex NonEmptyRegex = MyRegex();

    public PickType(IBasicActivateItems activator) : base(activator, NonEmptyRegex)
    {
    }

    public override string Format { get; }
    public override string Help { get; }
    public override IEnumerable<string> Examples { get; }

    public override bool IsMatch(string arg, int idx)
    {
        return GetType(arg) != null;
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        return new CommandLineObjectPickerArgumentValue(arg, idx, GetType(arg));
    }

    private static Type GetType(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
            return null;

        try
        {
            return
                MEF.GetType(BasicCommandExecution.ExecuteCommandPrefix + arg)
                ??
                MEF.GetType(arg);
        }
        catch (Exception)
        {
            return null;
        }
    }

    [GeneratedRegex(".*")]
    private static partial Regex MyRegex();
}