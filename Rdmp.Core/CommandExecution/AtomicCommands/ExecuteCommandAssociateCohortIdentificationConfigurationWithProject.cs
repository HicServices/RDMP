// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandAssociateCohortIdentificationConfigurationWithProject : BasicCommandExecution,
    IAtomicCommandWithTarget
{
    private Project _project;
    private CohortIdentificationConfiguration _cic;
    private readonly ProjectCohortIdentificationConfigurationAssociation[] _existingAssociations;

    public ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(IBasicActivateItems activator) :
        base(activator)
    {
        if (!activator.CoreChildProvider.AllCohortIdentificationConfigurations.Any())
            SetImpossible("There are no Cohort Identification Configurations yet");

        _existingAssociations = ((DataExportChildProvider)activator.CoreChildProvider).AllProjectAssociatedCics;
    }

    public override string GetCommandHelp() =>
        "Specifies that the Cohort Identification Configuration (query) is only for use generating cohorts for extractions of the specified project";

    public override void Execute()
    {
        if (_project == null)
        {
            //project is not known so get all projects
            var valid = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>();

            //except if the cic is the launch point
            if (_cic != null)
                valid =
                    valid.Where(v =>

                        //in which case only add projects which are not already associated with the cic launch point
                        !_existingAssociations.Any(
                            a => a.CohortIdentificationConfiguration_ID == _cic.ID && v.ID == a.Project_ID)).ToArray();

            if (SelectOne(valid, out var p))
                SetTarget(p);
            else
                return;
        }

        if (_cic == null)
        {
            //cic is not known (but project is thanks to above block)
            var valid =
                BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>();

            //allow them to select any cic where it does not already belong to the project
            valid =
                valid.Where(v =>
                    !_existingAssociations.Any(
                        a => a.Project_ID == _project.ID && v.ID == a.CohortIdentificationConfiguration_ID)).ToArray();


            if (SelectOne(valid, out var cic))
                SetTarget(cic);
            else
                return;
        }

        //command might be impossible

        base.Execute();

        //create new relationship in database between the cic and project
        _ = new ProjectCohortIdentificationConfigurationAssociation(
            BasicActivator.RepositoryLocator.DataExportRepository, _project, _cic);

        Publish(_project);
        Publish(_cic);
        Emphasise(_cic);
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        //if we know the cic the context is 'pick a project'
        _cic != null
            ? iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Add)
            :
            //if we know the _project the context is 'pick a cic'  (or if we don't know either then just use this icon too)
            iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Link);

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        switch (target)
        {
            case Project project:
                _project = project;
                break;
            case CohortIdentificationConfiguration configuration:
                _cic = configuration;
                break;
        }

        if (_project != null && _cic != null &&
            _project.GetAssociatedCohortIdentificationConfigurations().Contains(_cic))
            SetImpossible("Cohort Identification Configuration is already associated with this Project");

        return this;
    }
}