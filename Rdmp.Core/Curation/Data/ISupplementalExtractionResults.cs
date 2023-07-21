// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Describes the extraction status of a supplemental file/table which was bundled along with the normal datasets being
///     extracted.  This could
///     be lookup tables, pdf documents, etc.
/// </summary>
public interface ISupplementalExtractionResults : IExtractionResults, IReferenceOtherObjectWithPersist
{
    /// <summary>
    ///     <see cref="ISupplementalExtractionResults" /> is an audit class for supplemental artifacts produced in an
    ///     extraction (e.g. Lookup tables).  This
    ///     property points to the main audit record (of the dataset - see <see cref="ICumulativeExtractionResults" />).
    ///     <para>This is null if the artifact is a Global (always extracted)</para>
    /// </summary>
    int? CumulativeExtractionResults_ID { get; }

    /// <summary>
    ///     Only populated if the artifact is a Global (always extracted).  This points to the
    ///     <see cref="IExtractionConfiguration" /> being extracted
    ///     when the global was produced.
    /// </summary>
    int? ExtractionConfiguration_ID { get; }

    /// <summary>
    ///     True if the artifact extracted did not relate to a specific dataset (e.g. a Lookup) but to extract as a while.
    ///     This is determined
    ///     by looking at whether <see cref="CumulativeExtractionResults_ID" /> or <see cref="ExtractionConfiguration_ID" /> is
    ///     populated.
    /// </summary>
    bool IsGlobal { get; }

    /// <summary>
    ///     The Name of the object that was extracted (this is the logical name not the path e.g. "HelpDocs pdf file")
    /// </summary>
    string ExtractedName { get; }
}