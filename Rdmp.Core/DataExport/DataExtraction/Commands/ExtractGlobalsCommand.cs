// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.DataExtraction.Commands;

/// <summary>
///     Extraction command for the data export engine which mandates the extraction of all global (not dataset specific)
///     files in an <see cref="ExtractionConfiguration" /> (e.g.
///     <see cref="SupportingSQLTable" />)
/// </summary>
public class ExtractGlobalsCommand : ExtractCommand
{
    private readonly IProject project;

    public GlobalsBundle Globals { get; set; }

    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; private set; }

    public List<IExtractionResults> ExtractionResults { get; private set; }

    public ExtractGlobalsCommand(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IProject project,
        ExtractionConfiguration configuration, GlobalsBundle globals) : base(configuration)
    {
        RepositoryLocator = repositoryLocator;
        this.project = project;
        Globals = globals;

        ExtractionResults = new List<IExtractionResults>();
    }

    public override DirectoryInfo GetExtractionDirectory()
    {
        return new ExtractionDirectory(project.ExtractionDirectory, Configuration).GetGlobalsDirectory();
    }

    public override string DescribeExtractionImplementation()
    {
        return string.Join(";", Globals.Contents);
    }

    public override string ToString()
    {
        return ExtractionDirectory.GLOBALS_DATA_NAME;
    }
}