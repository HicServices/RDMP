// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.DataExport.DataExtraction.UserPicks;

/// <summary>
///     Bundle containing references to all the globally extractable (supplied with every project extraction regardless of
///     dataset) documents and tables that need
///     to be extracted/copied to the output ExtractionDirectory.
/// </summary>
public class GlobalsBundle : Bundle
{
    public List<SupportingDocument> Documents { get; }
    public List<SupportingSQLTable> SupportingSQL { get; }

    public GlobalsBundle(SupportingDocument[] documents, SupportingSQLTable[] supportingSQL) :
        base(
            Array.Empty<object>().Union(documents).Union(supportingSQL).ToArray()
            //pass all the objects to the base class so it can allocate initial States
        )
    {
        Documents = documents.ToList();
        SupportingSQL = supportingSQL.ToList();
    }

    public bool Any()
    {
        return Documents.Any() || SupportingSQL.Any();
    }


    protected override void OnDropContent(object toDrop)
    {
        switch (toDrop)
        {
            case SupportingDocument item:
                Documents.Remove(item);
                return;
            case SupportingSQLTable drop:
                SupportingSQL.Remove(drop);
                return;
            default:
                throw new NotSupportedException($"Did not know how to drop object of type {toDrop.GetType()}");
        }
    }
}