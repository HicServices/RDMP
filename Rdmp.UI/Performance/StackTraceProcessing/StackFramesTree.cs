// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Performance;
using Rdmp.UI.SimpleDialogs;

namespace Rdmp.UI.Performance.StackTraceProcessing;

internal class StackFramesTree
{
    public string CurrentFrame { get; private set; }
    public int QueryCount { get; private set; }

    public bool HasSourceCode { get; private set; }

    public string Method { get; private set; }
    public string Filename { get; private set; }
    public int LineNumber { get; private set; }

    public bool IsInDatabaseAccessAssembly { get; private set; }

    public Dictionary<string, StackFramesTree> Children = new();

    public StackFramesTree(string[] stackFrameAndSubframes, QueryPerformed performed,
        bool isInDatabaseAccessAssemblyYet)
    {
        QueryCount = 0;

        PopulateSourceCode(stackFrameAndSubframes[0]);

        CurrentFrame = stackFrameAndSubframes[0];
        AddSubframes(stackFrameAndSubframes, performed);

        IsInDatabaseAccessAssembly = isInDatabaseAccessAssemblyYet || CurrentFrame.Contains("DatabaseCommandHelper");
    }

    private bool PopulateSourceCode(string frame)
    {
        HasSourceCode = ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out var filenameMatch,
            out var lineNumberMatch, out var method);

        Filename = filenameMatch;
        LineNumber = lineNumberMatch;
        Method = method;

        return HasSourceCode;
    }

    public static bool FindSourceCode(string frame) =>
        ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out _, out _, out _);

    public static string GetMethodName(string frame)
    {
        ExceptionViewerStackTraceWithHyperlinks.MatchStackLine(frame, out _, out _, out var method);

        return method;
    }


    public override string ToString()
    {
        if (!HasSourceCode)
            return CurrentFrame;

        return $"{Path.GetFileNameWithoutExtension(Filename)}.{Method}";
    }

    public void AddSubframes(string[] lines, QueryPerformed query)
    {
        if (!lines[0].Equals(CurrentFrame))
            throw new Exception("Current frame did not match expected lines[0]");

        QueryCount += query.TimesSeen;

        //we are the last line
        if (lines.Length == 1)
            return;

        //we know about the child
        if (Children.TryGetValue(lines[1], out var child))
            child.AddSubframes(lines.Skip(1).ToArray(), query); //tell child to audit the relevant subframes
        else
            Children.Add(lines[1], new StackFramesTree(lines.Skip(1).ToArray(), query, IsInDatabaseAccessAssembly));
    }
}