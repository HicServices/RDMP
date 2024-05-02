// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Helper for unwrapping Exception.InnerExceptions and ReflectionTypeLoadExceptions.LoaderExceptions into a single
///     flat message string of all errors.
/// </summary>
public static class ExceptionHelper
{
    [Pure]
    public static string ExceptionToListOfInnerMessages(Exception e, bool includeStackTrace = false)
    {
        var message = new StringBuilder(e.Message);
        if (includeStackTrace)
            message.Append(Environment.NewLine + e.StackTrace);

        if (e is ReflectionTypeLoadException exception)
            foreach (var loaderException in exception.LoaderExceptions)
                message.Append(Environment.NewLine +
                               ExceptionToListOfInnerMessages(loaderException, includeStackTrace));

        if (e.InnerException != null)
            message.Append(Environment.NewLine + ExceptionToListOfInnerMessages(e.InnerException, includeStackTrace));

        return message.ToString();
    }

    /// <summary>
    ///     Returns the first base Exception in the AggregateException.InnerExceptions list which is of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    [Pure]
    public static T GetExceptionIfExists<T>(this AggregateException e) where T : Exception
    {
        return e.Flatten().InnerExceptions.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    ///     Returns the first InnerException of type T in the Exception or null.
    ///     <para>If e is T then e is returned directly</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="e"></param>
    /// <returns></returns>
    [Pure]
    public static T GetExceptionIfExists<T>(this Exception e) where T : Exception
    {
        while (true)
        {
            if (e is T t) return t;
            if (e.InnerException == null) return null;
            e = e.InnerException;
        }
    }
}