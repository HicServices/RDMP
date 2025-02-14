// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// <para>Describes whether a Catalogue can be extracted in data export projects and if so, whether it is only permitted in a single Project.</para>
/// 
/// <para>See <see cref="Catalogue.GetExtractabilityStatus"/></para>
/// </summary>
public class CatalogueExtractabilityStatus
{
    /// <summary>
    /// The <see cref="Catalogue"/> is extractable as an ExtractableDataSet in data export database
    /// </summary>
    public bool IsExtractable { get; private set; }

    /// <summary>
    /// The <see cref="Catalogue"/> is extractable as an ExtractableDataSet in data export database but only for use in a single
    /// Project.
    /// </summary>
    public bool IsProjectSpecific { get; private set; }

    /// <summary>
    /// Creates a new confirmed extractability knowledge for a <see cref="Catalogue"/>
    /// </summary>
    /// <param name="isExtractable"></param>
    /// <param name="isProjectSpecific"></param>
    public CatalogueExtractabilityStatus(bool isExtractable, bool isProjectSpecific)
    {
        IsExtractable = isExtractable;
        IsProjectSpecific = isProjectSpecific;
    }
}