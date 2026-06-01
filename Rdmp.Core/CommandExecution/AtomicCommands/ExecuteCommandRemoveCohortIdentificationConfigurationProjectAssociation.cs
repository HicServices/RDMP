// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
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

public sealed class ExecuteCommandRemoveCohortIdentificationConfigurationProjectAssociation : BasicCommandExecution,
    IAtomicCommandWithTarget
{
    private Project _project;
    private CohortIdentificationConfiguration _cic;
    private ProjectCohortIdentificationConfigurationAssociation[] _existingAssociations;
    private ProjectCohortIdentificationConfigurationAssociation _associationToDelete;
    private IBasicActivateItems _activator;
    public ExecuteCommandRemoveCohortIdentificationConfigurationProjectAssociation(IBasicActivateItems activator) :
        base(activator)
    {
        _activator = activator;
        if (!activator.CoreChildProvider.AllCohortIdentificationConfigurations.Any())
            SetImpossible("There are no Cohort Identification Configurations yet");

    }

    public override string GetCommandHelp() =>
        "Specifies that the Cohort Identification Configuration (query) is only for use generating cohorts for extractions of the specified project";

    public override void Execute()
    {
        if (_project is null && _cic is null && _associationToDelete is null)
        {
            _cic = (CohortIdentificationConfiguration)_activator.SelectOne("Select a Cohort Identification Configuration", _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>().ToArray());
            if (_cic is null) return;
        }
        if (_cic is not null && _project is null && _associationToDelete is null)
        {

            _existingAssociations = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ProjectCohortIdentificationConfigurationAssociation>("CohortIdentificationConfiguration_ID", _cic.ID);
        }
        if (_cic is null && _project is not null && _associationToDelete is null)
        {

            _existingAssociations = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ProjectCohortIdentificationConfigurationAssociation>("Project_ID", _project.ID);
        }
        if (_cic != null && _project != null && _associationToDelete is null)
        {
            _associationToDelete = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ProjectCohortIdentificationConfigurationAssociation>("Project_ID", _project.ID).Where(pac => pac.CohortIdentificationConfiguration_ID == _cic.ID).First();
        }
        if (_associationToDelete is null)
        {
            if (_cic is not null && _project is null)
            {
                var projects = _existingAssociations.Select(a => a.Project).ToArray();
                var selectedProject = (Project)_activator.SelectOne("Select the project to remove the association with", projects);
                _associationToDelete = _existingAssociations.Where(pac => pac.Project_ID == selectedProject.ID).First();

            }
            else if (_cic is null && _project is not null)
            {
                var cics = _existingAssociations.Select(a => a.CohortIdentificationConfiguration).ToArray();
                var selectedCic = (CohortIdentificationConfiguration)_activator.SelectOne("Select the cohort to remove the association with", cics);
                _associationToDelete = _existingAssociations.Where(pac => pac.CohortIdentificationConfiguration_ID == selectedCic.ID).First();

            }
            else
            {
                _associationToDelete = (ProjectCohortIdentificationConfigurationAssociation)_activator.SelectOne("Select the association to remove", _existingAssociations);
            }
        }
        if (_associationToDelete is null) return;

        _associationToDelete.DeleteInDatabase();
        Publish(_project);
        Publish(_cic);
        base.Execute();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        //if we know the cic the context is 'pick a project'
        _cic != null
            ? iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Delete)
            :
            //if we know the _project the context is 'pick a cic'  (or if we don't know either then just use this icon too)
            iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Delete);

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
            case ProjectCohortIdentificationConfigurationAssociation association:
                _associationToDelete = association;
                break;
        }
        return this;
    }
}