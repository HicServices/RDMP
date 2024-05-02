// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;

public class ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration : CatalogueCreationCommandExecution
{
    private AggregateConfiguration _aggregateConfiguration;
    private ExtractableCohort _cohort;
    private DiscoveredTable _table;

    public ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(IBasicActivateItems activator,
        AggregateConfiguration ac) : base(activator)
    {
        _aggregateConfiguration = ac;
    }

    public override string GetCommandHelp()
    {
        return
            "Executes an existing cohort set, patient index table or graph and stores the results in a new table (which is imported as a new dataset)";
    }

    public override void Execute()
    {
        base.Execute();

        _aggregateConfiguration ??=
            SelectOne<AggregateConfiguration>(BasicActivator.RepositoryLocator.CatalogueRepository);

        if (_aggregateConfiguration == null)
            return;

        if (_aggregateConfiguration.IsJoinablePatientIndexTable())
        {
            if (!BasicActivator.YesNo("Would you like to constrain the records to only those in a committed cohort?",
                    "Cohort Records Only", out var chosen))
                return;

            if (chosen)
            {
                _cohort = SelectOne<ExtractableCohort>(BasicActivator.RepositoryLocator.DataExportRepository);

                if (_cohort == null)
                    return;
            }

            var externalData = _cohort?.GetExternalData();
            if (externalData != null)
            {
                var projNumber = externalData.ExternalProjectNumber;
                var projs = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>()
                    .Where(p => p.ProjectNumber == projNumber).ToArray();
                if (projs.Length == 1)
                    ProjectSpecific = projs[0];
            }
        }

        _table = SelectTable(true, "Choose destination table name");

        if (_table == null)
            return;

        var useCase = new CreateTableFromAggregateUseCase(_aggregateConfiguration, _cohort, _table);

        var runner = BasicActivator.GetPipelineRunner(new DialogArgs
            {
                WindowTitle = "Create Table from AggregateConfiguration",
                TaskDescription =
                    "Select a Pipeline compatible with reading data from an AggregateConfiguration.  If the pipeline completes successfully a new Catalogue will be created referencing the new table created in your database."
            }
            , useCase, null /*TODO inject Pipeline in CLI constructor*/);

        runner.PipelineExecutionFinishedsuccessfully += ui_PipelineExecutionFinishedsuccessfully;

        runner.Run(BasicActivator.RepositoryLocator, null, null, null);
    }

    private void ui_PipelineExecutionFinishedsuccessfully(object sender, PipelineEngineEventArgs args)
    {
        if (!_table.Exists())
            throw new Exception($"Pipeline execute successfully but the expected table '{_table}' did not exist");

        var importer = new TableInfoImporter(BasicActivator.RepositoryLocator.CatalogueRepository, _table);
        importer.DoImport(out var ti, out _);

        BasicActivator.CreateAndConfigureCatalogue(ti, null,
            $"Execution of '{_aggregateConfiguration}' (AggregateConfiguration ID ={_aggregateConfiguration.ID})",
            ProjectSpecific, TargetFolder);
    }


    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Execute);
    }

    public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        base.SetTarget(target);

        if (target is AggregateConfiguration configuration)
            _aggregateConfiguration = configuration;

        if (target is ExtractableCohort cohort)
            _cohort = cohort;

        return this;
    }
}