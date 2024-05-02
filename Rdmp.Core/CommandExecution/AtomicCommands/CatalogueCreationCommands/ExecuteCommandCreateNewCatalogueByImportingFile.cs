// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Destinations;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;

/// <summary>
///     Import a file (e.g. CSV, excel etc) into a relational database as a new table using a given <see cref="Pipeline" />
///     and create a reference to it in RDMP.
/// </summary>
public class ExecuteCommandCreateNewCatalogueByImportingFile : CatalogueCreationCommandExecution
{
    private readonly DiscoveredDatabase _targetDatabase;
    private IPipeline _pipeline;
    private readonly string _extractionIdentifier;
    private readonly string _initialDescription;

    public FileInfo File { get; private set; }


    private void CheckFile()
    {
        if (File == null)
            return;

        if (!File.Exists)
            SetImpossible("File does not exist");
    }

    public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator, FileInfo file = null) : this(
        activator, file, null, null, null, null)
    {
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator,
        [DemandsInitialization("The file to load into the database")]
        FileInfo file,
        [DemandsInitialization(
            "Name of a column in the file to be the IsExtractionIdentifier column or Null if it doesn't have one")]
        string extractionIdentifier,
        [DemandsInitialization("The database to upload the data into")]
        DiscoveredDatabase targetDatabase,
        [DemandsInitialization(
            "Pipeline for reading the source file, applying any transforms and writing to the database")]
        Pipeline pipeline,
        [DemandsInitialization(Desc_ProjectSpecificParameter)]
        Project projectSpecific,
        string initialDescription = null) : base(activator, projectSpecific, null)

    {
        File = file;
        _targetDatabase = targetDatabase;
        _pipeline = pipeline;
        _extractionIdentifier = extractionIdentifier;
        UseTripleDotSuffix = true;
        CheckFile();
        _initialDescription = initialDescription;
    }


    public ExecuteCommandCreateNewCatalogueByImportingFile(IBasicActivateItems activator,
        FileCollectionCombineable file) : base(activator)
    {
        if (file.Files.Length != 1)
        {
            SetImpossible("Only one file can be imported at once");
            return;
        }

        File = file.Files[0];
        UseTripleDotSuffix = true;
        CheckFile();
    }


    public override void Execute()
    {
        base.Execute();

        if (_pipeline == null)
        {
            var pipelines = BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>();

            var compatible = UploadFileUseCase.DesignTime().FilterCompatiblePipelines(pipelines).ToArray();

            _pipeline = (IPipeline)BasicActivator.SelectOne("File Upload Pipeline", compatible);

            if (_pipeline == null)
                throw new Exception("No pipeline selected for upload");
        }

        var db = _targetDatabase ?? BasicActivator.SelectDatabase(true, "Target database");

        if (db == null)
            return;

        File ??= BasicActivator.SelectFile("File to upload");

        if (File == null)
            return;

        var useCase = new UploadFileUseCase(File, db, BasicActivator);

        var runner = BasicActivator.GetPipelineRunner(
            GetCreateCatalogueFromFileDialogArgs()
            , useCase, _pipeline);
        runner.PipelineExecutionFinishedsuccessfully += (s, e) => OnPipelineCompleted(s, e, db);
        runner.Run(BasicActivator.RepositoryLocator, null, null, null);
    }

    public static DialogArgs GetCreateCatalogueFromFileDialogArgs()
    {
        return new DialogArgs
        {
            WindowTitle = "Create Catalogue from File",
            TaskDescription =
                "Select a Pipeline compatible with the file format you are loading and your intended destination.  If the pipeline completes successfully a new Catalogue will be created referencing the new table created in your database."
        };
    }

    private void OnPipelineCompleted(object _, PipelineEngineEventArgs args, DiscoveredDatabase db)
    {
        var engine = args.PipelineEngine;

        //todo figure out what it created
        if (engine.DestinationObject is not DataTableUploadDestination dest)
            throw new Exception(
                $"Destination of engine was unexpectedly not a DataTableUploadDestination despite use case {nameof(UploadFileUseCase)}");

        if (string.IsNullOrWhiteSpace(dest.TargetTableName))
            throw new Exception($"Destination of engine failed to populate {dest.TargetTableName}");

        var tbl = db.ExpectTable(dest.TargetTableName);

        if (!tbl.Exists())
            throw new Exception(
                $"Destination of engine claimed to have created {tbl.GetFullyQualifiedName()} but it did not exist");

        var importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, tbl);
        importer.DoImport(out var ti, out var _);
        var extractionIdentifiers = _extractionIdentifier is null
            ? null
            : ti.ColumnInfos.Where(t => t.Name == _extractionIdentifier).ToArray();
        var cata = BasicActivator.CreateAndConfigureCatalogue(ti, extractionIdentifiers,
            $"Import of file '{File.FullName}' by {Environment.UserName} on {DateTime.Now}", ProjectSpecific,
            TargetFolder);

        if (cata == null) return;

        if (_initialDescription is not null)
        {
            cata.Description = _initialDescription;
            cata.SaveToDatabase();
        }

        Publish(cata);
        Emphasise(cata);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return ProjectSpecific != null
            ? iconProvider.GetImage(RDMPConcept.ProjectCatalogue, OverlayKind.Add)
            : iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Add);
    }


    public override string GetCommandHelp()
    {
        return GlobalStrings.CreateNewCatalogueByImportingFileHelp;
    }

    public override string GetCommandName()
    {
        return OverrideCommandName ?? GlobalStrings.CreateNewCatalogueByImportingFile;
    }
}