// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

/// <summary>
///     Loads private identifiers from a file using the given <see cref="Pipeline" /> to create a new
///     <see cref="ExtractableCohort" /> for a given <see cref="Project" /> (which must have a ProjectNumber specified)
/// </summary>
public class ExecuteCommandCreateNewCohortFromFile : CohortCreationCommandExecution
{
    private readonly FileInfo _file;

    public ExecuteCommandCreateNewCohortFromFile(IBasicActivateItems activator, ExternalCohortTable externalCohortTable)
        :
        base(activator, externalCohortTable, null, null, null)
    {
        UseTripleDotSuffix = true;
    }

    public ExecuteCommandCreateNewCohortFromFile(IBasicActivateItems activator, FileInfo file,
        ExternalCohortTable externalCohortTable)
        : this(activator, file, externalCohortTable, null, null, null)
    {
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCohortFromFile(IBasicActivateItems activator,
        [DemandsInitialization("A file containing private cohort identifiers")]
        FileInfo file,
        [DemandsInitialization(Desc_ExternalCohortTableParameter)]
        ExternalCohortTable externalCohortTable,
        [DemandsInitialization(Desc_CohortNameParameter)]
        string cohortName,
        [DemandsInitialization(Desc_ProjectParameter)]
        Project project,
        [DemandsInitialization("Pipeline for reading from the file and allocating release identifiers")]
        IPipeline pipeline)
        : base(activator, externalCohortTable, cohortName, project, pipeline)
    {
        _file = file;
        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp()
    {
        return "Create a cohort containing ALL the patient identifiers in the chosen file";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(CatalogueIcons.ImportFile);
    }

    public override void Execute()
    {
        base.Execute();

        FlatFileToLoad flatFile;

        //if no explicit file has been chosen
        if (_file == null)
        {
            var file = BasicActivator.SelectFile("Cohort file");

            //get user to pick one
            if (file == null)
                return;

            flatFile = new FlatFileToLoad(file);
        }
        else
        {
            flatFile = new FlatFileToLoad(_file);
        }

        var request = GetCohortCreationRequest(ExtractableCohortAuditLogBuilder.GetDescription(flatFile.File));

        //user choose to cancel the cohort creation request dialogue
        if (request == null)
            return;

        request.FileToLoad = flatFile;

        var configureAndExecuteDialog =
            GetConfigureAndExecuteControl(request, $"Uploading File {flatFile.File.Name}", flatFile);

        //add the flat file to the dialog with an appropriate description of what they are trying to achieve
        configureAndExecuteDialog.Run(BasicActivator.RepositoryLocator, null, null, null);
    }
}