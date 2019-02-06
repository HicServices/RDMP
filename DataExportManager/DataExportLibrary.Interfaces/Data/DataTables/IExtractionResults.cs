// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.Data.Referencing;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    /// <summary>
    /// Record of a single component extracted as part of an <see cref="IExtractionConfiguration"/>.  This could be an anonymised dataset or bundled supporting
    /// documents e.g. Lookups , pdfs etc.  This audit is used to perform release process (where all extracted artifacts are collected and sent somewhere).
    /// </summary>
    public interface IExtractionResults : IReferenceOtherObject,IMapsDirectlyToDatabaseTable, ISaveable
    {
        string DestinationDescription { get; }
        string DestinationType { get; }
        int RecordsExtracted { get; }
        DateTime DateOfExtraction { get; }
        string Exception { get; set; }
        string SQLExecuted { get; }

        Type GetDestinationType();
    }
}