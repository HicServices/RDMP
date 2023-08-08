﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution;

public abstract class CommandFactoryBase
{
    /// <summary>
    /// Returns o is <typeparamref name="T"/> but with auto unpacking of <see cref="IMasqueradeAs"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="o"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    public static bool Is<T>(object o, out T match)
    {
        if (o is T o1)
        {
            match = o1;
            return true;
        }

        if (o is IMasqueradeAs m) return Is<T>(m.MasqueradingAs(), out match);

        match = default;
        return false;
    }
}