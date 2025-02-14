// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
/// Input object to Extraction Pipelines.  Typically this is a dataset that needs to be linked with a cohort and extracted into the ExtractionDirectory.
/// Also includes the ongoing ExtractCommandState that the IExtractCommand is in in the Pipeline e.g. WaitingForSQLServer etc.
/// </summary>
public interface IExtractCommand
{
    DirectoryInfo GetExtractionDirectory();
    IExtractionConfiguration Configuration { get; }
    string DescribeExtractionImplementation();

    ExtractCommandState State { get; }
    void ElevateState(ExtractCommandState newState);

    /// <summary>
    /// Flag that can be set by sources that support resume.  This indicates that the request is for the next
    /// set of new data that has not been successfully outputted yet and should be appended to the destination
    /// instead of overwriting.
    /// </summary>
    bool IsBatchResume { get; set; }
}