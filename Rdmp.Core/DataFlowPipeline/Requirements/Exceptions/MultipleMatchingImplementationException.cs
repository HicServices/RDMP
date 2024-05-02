// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.DataFlowPipeline.Requirements.Exceptions;

/// <summary>
///     Occurs when multiple input objects in a data flow pipeline match a given IPipelineRequirement e.g.
///     IPipelineRequirement of object (never do this!) will match every single input object which will throw this
///     exception
/// </summary>
public sealed class MultipleMatchingImplementationException : Exception
{
    /// <summary>
    ///     Creates a new Exception with the provided message
    /// </summary>
    /// <param name="s"></param>
    public MultipleMatchingImplementationException(string s) : base(s)
    {
    }
}