// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CohortCommitting;

/// <summary>
///     The datatype of an IsExtractionIdentifier column found in (at least) one of your Catalogues.  This is used to help
///     you make an informed descision about
///     what datatype to store patient identifiers in when creating a new cohort database (See
///     CreateNewCohortDatabaseWizard).  Every IsExtractionIdentifier column
///     datatype will be listed along with a count of the number of columns with that datatype and the user (or system)
///     will be allowed to select one.
///     <para>
///         This helps if you have for example 30 datasets where the patient identifier is varchar(10) and 5 where it is
///         varchar(max) - logical choice is varchar(10).
///     </para>
/// </summary>
public class PrivateIdentifierPrototype
{
    public PrivateIdentifierPrototype(ExtractionInformation extractionInformation)
    {
        RuntimeName = extractionInformation.GetRuntimeName();
        DataType = extractionInformation.ColumnInfo.Data_type;
        MatchingExtractionInformations = new List<ExtractionInformation>(new[] { extractionInformation });
    }

    public PrivateIdentifierPrototype(string runtimeName, string datatype)
    {
        RuntimeName = runtimeName;
        DataType = datatype;
        MatchingExtractionInformations = new List<ExtractionInformation>();
    }

    public string RuntimeName { get; internal set; }
    public string DataType { get; internal set; }
    public List<ExtractionInformation> MatchingExtractionInformations { get; internal set; }

    public bool IsCompatible(ExtractionInformation extractionInformation)
    {
        return extractionInformation.GetRuntimeName() == RuntimeName &&
               extractionInformation.ColumnInfo.Data_type == DataType;
    }

    public int CountOfTimesSeen()
    {
        return MatchingExtractionInformations.Count;
    }
}