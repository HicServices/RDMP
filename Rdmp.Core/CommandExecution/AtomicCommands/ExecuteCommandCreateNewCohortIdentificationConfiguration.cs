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
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Creates a new persistent database query configuration for identifying cohort sets of patients.
/// </summary>
public class ExecuteCommandCreateNewCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommandWithTarget
{
    private Project _associateWithProject;
    private readonly string _name;

    /// <summary>
    ///     True to prompt the user to pick a Project if no explicit Project is configured
    ///     yet on this command.
    /// </summary>
    public bool PromptToPickAProject { get; set; } = false;

    /// <summary>
    ///     The folder to put the new <see cref="CohortIdentificationConfiguration" /> in.  Defaults to
    ///     <see cref="FolderHelper.Root" />
    /// </summary>
    public string Folder { get; init; } = FolderHelper.Root;

    /// <summary>
    ///     Name to give the root component of new cics created by this command (usually an EXCEPT but not always - see Cohort
    ///     Configuration Wizard)
    /// </summary>
    public const string RootContainerName = "Root Container";

    /// <summary>
    ///     Name to give the inclusion component of new cics created by this command
    /// </summary>
    public const string InclusionCriteriaName = "Inclusion Criteria";

    /// <summary>
    ///     Name to give the exclusion component of new cics created by this command
    /// </summary>
    public const string ExclusionCriteriaName = "Exclusion Criteria";

    public ExecuteCommandCreateNewCohortIdentificationConfiguration(IBasicActivateItems activator) : base(activator)
    {
        if (!activator.CoreChildProvider.AllCatalogues.Any())
            SetImpossible("There are no datasets loaded yet into RDMP");

        UseTripleDotSuffix = true;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandCreateNewCohortIdentificationConfiguration(IBasicActivateItems activator, string name) :
        this(activator)
    {
        _name = name;
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Add);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _associateWithProject = target as Project;
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        var proj = _associateWithProject;

        if (proj == null && BasicActivator.IsInteractive && PromptToPickAProject)
        {
            var projects = BasicActivator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>();

            if (projects.Any())
                proj = (Project)BasicActivator.SelectOne(new DialogArgs
                {
                    WindowTitle = "Associate with Project",
                    TaskDescription =
                        "Do you want to associate this new query with a Project? if not select Null or Cancel.",
                    AllowSelectingNull = true
                }, projects);
        }

        CohortIdentificationConfiguration cic;

        //if user wants to see the wizard and isn't using the CLI constructor
        if (UserSettings.ShowCohortWizard && string.IsNullOrWhiteSpace(_name))
        {
            //try showing wizard if we can't
            if (!BasicActivator.ShowCohortWizard(out cic))
                // No wizards are available, just generate a basic one
                cic = GenerateBasicCohortIdentificationConfiguration();
        }
        else
        {
            // user doesn't want to see the wizard
            cic = GenerateBasicCohortIdentificationConfiguration();
        }

        if (cic == null)
            return;

        cic.Folder = Folder;
        cic.SaveToDatabase();

        if (proj != null)
        {
            var assoc = proj.AssociateWithCohortIdentification(cic);
            Publish(assoc);
            Emphasise(assoc, int.MaxValue);
        }
        else
        {
            Publish(cic);
            Emphasise(cic, int.MaxValue);
        }

        Activate(cic);
    }

    private CohortIdentificationConfiguration GenerateBasicCohortIdentificationConfiguration()
    {
        var name = _name;

        if (name == null)
            if (!BasicActivator.TypeText(new DialogArgs
                {
                    WindowTitle = "New Cohort Builder Query",
                    TaskDescription = "Enter a name for the Cohort Builder Query.",
                    EntryLabel = "Name"
                }, 255, null, out name, false))
                return null;

        var cic = new CohortIdentificationConfiguration(BasicActivator.RepositoryLocator.CatalogueRepository, name);
        cic.CreateRootContainerIfNotExists();
        var root = cic.RootCohortAggregateContainer;
        root.Name = RootContainerName;
        root.Operation = SetOperation.EXCEPT;
        root.SaveToDatabase();

        var inclusion =
            new CohortAggregateContainer(BasicActivator.RepositoryLocator.CatalogueRepository, SetOperation.UNION)
            {
                Name = InclusionCriteriaName,
                Order = 0
            };
        inclusion.SaveToDatabase();

        var exclusion =
            new CohortAggregateContainer(BasicActivator.RepositoryLocator.CatalogueRepository, SetOperation.UNION)
            {
                Name = ExclusionCriteriaName,
                Order = 1
            };
        exclusion.SaveToDatabase();

        root.AddChild(inclusion);
        root.AddChild(exclusion);

        return cic;
    }

    public override string GetCommandHelp()
    {
        return
            "Creating a cohort identification configuration which includes/excludes patients based on the data in your database tables ";
    }
}