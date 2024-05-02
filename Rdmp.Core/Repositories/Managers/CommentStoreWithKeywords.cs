// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.ReusableLibraryCode.Comments;

namespace Rdmp.Core.Repositories.Managers;

/// <summary>
///     Subclass of <see cref="CommentStore" /> which also loads KeywordHelp.txt
/// </summary>
public sealed class CommentStoreWithKeywords : CommentStore
{
    public override void ReadComments(params string[] directoriesToLookInForComments)
    {
        base.ReadComments(directoriesToLookInForComments);

        var assembly = typeof(CommentStoreWithKeywords).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Curation.KeywordHelp.txt");
        using var reader =
            new StreamReader(stream ?? throw new ApplicationException("Unable to read KeywordHelp.txt resource"));
        AddToHelp(reader.ReadToEnd());
    }

    private void AddToHelp(string keywordHelpFileContents)
    {
        var lines = keywordHelpFileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var split = line.Split(':');

            if (split.Length != 2)
                throw new Exception(
                    $"Malformed line in Resources.KeywordHelp, line is:{Environment.NewLine}{line}{Environment.NewLine}We expected it to have exactly one colon in it");

            if (!ContainsKey(split[0]))
                Add(split[0], split[1]);
        }
    }
}