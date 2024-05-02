// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;

namespace Rdmp.Core.DataExport.DataExtraction.FileOutputFormats;

public abstract class FileOutputFormat : IFileOutputFormat
{
    public abstract string GetFileExtension();

    public string OutputFilename { get; }

    /// <summary>
    ///     The number of decimal places to round floating point numbers to.  This only applies to data in the pipeline which
    ///     is hard typed Float and not to string values
    /// </summary>
    public int? RoundFloatsTo { get; internal set; }

    public abstract void Open();
    public abstract void Open(bool append);
    public abstract void WriteHeaders(DataTable t);
    public abstract void Append(DataRow r);
    public abstract void Flush();
    public abstract void Close();

    protected FileOutputFormat(string outputFilename)
    {
        OutputFilename = outputFilename;
    }
}