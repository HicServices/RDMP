// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCloneCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommandWithTarget
{
    private CohortIdentificationConfiguration _cic;
    private Project _project;
    private string _name;
    private int? _version;
    private bool _autoConfirm;

    /// <summary>
    /// The clone that was created this command or null if it has not been executed/failed
    /// </summary>
    public CohortIdentificationConfiguration CloneCreatedIfAny { get; private set; }

    [UseWithObjectConstructor]
    public ExecuteCommandCloneCohortIdentificationConfiguration(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic, string name = null, int? version = null, bool autoConfirm=false)
        : base(activator)
    {
        _cic = cic;
        _name = name;
        _version = version;
        _autoConfirm = autoConfirm;
    }

    public override string GetCommandHelp() =>
        "Creates an exact copy of the Cohort Identification Configuration (query) including all cohort sets, patient index tables, parameters, filter containers, filters etc";

    public ExecuteCommandCloneCohortIdentificationConfiguration(IBasicActivateItems activator) : base(activator)
    {
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        switch (target)
        {
            case CohortIdentificationConfiguration configuration:
                _cic = configuration;
                break;
            case Project project:
                _project = project;
                break;
        }

        return this;
    }

    public override void Execute()
    {
        base.Execute();

        _cic ??= SelectOne<CohortIdentificationConfiguration>(BasicActivator.RepositoryLocator.CatalogueRepository);

        if (_cic == null)
            return;

        // Confirm creating yes/no (assuming activator is interactive)
        if (!_autoConfirm && BasicActivator.IsInteractive && !YesNo(
                "This will create a 100% copy of the entire CohortIdentificationConfiguration including all datasets, filters, parameters and set operations. Are you sure this is what you want?",
                "Confirm Cloning")) return;
        CloneCreatedIfAny = _cic.CreateClone(ThrowImmediatelyCheckNotifier.Quiet);
        if (CloneCreatedIfAny != null)
        {
            CloneCreatedIfAny.Version = _version;
            CloneCreatedIfAny.Name = _name ?? $"{CloneCreatedIfAny.Name[..^8]}:{CloneCreatedIfAny.Version}";
            CloneCreatedIfAny.SaveToDatabase();
        }

        if (_project != null) // clone the association
            _ = new ProjectCohortIdentificationConfigurationAssociation(
                BasicActivator.RepositoryLocator.DataExportRepository,
                _project,
                CloneCreatedIfAny);

        //Load the clone up
        Publish(CloneCreatedIfAny);
        if (_project != null)
            Emphasise(_project);
        else
            Emphasise(CloneCreatedIfAny);

        Activate(CloneCreatedIfAny);
    }
}