// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Checks;

/// <summary>
///     Runs checks on a Project to make sure it has an extraction folder, project number etc.  Also checks
///     ExtractionConfigurations that are part of the project
///     to see if valid queries can be built and rows read.
/// </summary>
public class ProjectChecker : ICheckable
{
    private readonly IProject _project;
    private readonly IBasicActivateItems _activator;
    private IExtractionConfiguration[] _extractionConfigurations;
    private DirectoryInfo _projectDirectory;

    /// <summary>
    ///     True to fetch all <see cref="IExtractionConfiguration" /> and check with
    ///     <see cref="ExtractionConfigurationChecker" />
    /// </summary>
    public bool CheckConfigurations { get; set; }

    /// <summary>
    ///     True to fetch all <see cref="ISelectedDataSets" /> and check with <see cref="SelectedDataSetsChecker" />
    /// </summary>
    public bool CheckDatasets { get; set; }

    /// <summary>
    ///     Sets up the class to check the state of the <paramref name="project" />
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="project"></param>
    public ProjectChecker(IBasicActivateItems activator, IProject project)
    {
        _project = project;
        _activator = activator;
        CheckDatasets = true;
        CheckConfigurations = true;
    }

    /// <summary>
    ///     Checks the <see cref="IProject" /> has a valid <see cref="IProject.ExtractionDirectory" />,
    ///     <see cref="IProject.ProjectNumber" /> etc and runs additional
    ///     checkers (See <see cref="CheckConfigurations" /> and <see cref="CheckDatasets" />).
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        notifier.OnCheckPerformed(new CheckEventArgs($"About to check project {_project.Name} (ID={_project.ID})",
            CheckResult.Success));

        _extractionConfigurations = _project.ExtractionConfigurations;

        if (!_extractionConfigurations.Any())
            notifier.OnCheckPerformed(new CheckEventArgs("Project does not have any ExtractionConfigurations yet",
                CheckResult.Fail));

        if (_project.ProjectNumber == null)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Project does not have a Project Number, this is a number which is meaningful to you (as opposed to ID which is the systems number for the Project) and must be unique.",
                    CheckResult.Fail));

        //check to see if there is an extraction directory
        if (string.IsNullOrWhiteSpace(_project.ExtractionDirectory))
            notifier.OnCheckPerformed(new CheckEventArgs("Project does not have an ExtractionDirectory",
                CheckResult.Fail));
        try
        {
            //see if they punched the keyboard instead of typing a legit folder
            _projectDirectory = new DirectoryInfo(_project.ExtractionDirectory);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Project ExtractionDirectory ('{_project.ExtractionDirectory}') is not a valid directory name ",
                CheckResult.Fail, e));
            return;
        }

        //tell them whether it exists or not
        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Project ExtractionDirectory ('{_project.ExtractionDirectory}') {(_projectDirectory.Exists ? "Exists" : "Does Not Exist")}",
            _projectDirectory.Exists ? CheckResult.Success : CheckResult.Fail));

        if (CheckConfigurations)
            foreach (var extractionConfiguration in _extractionConfigurations)
            {
                var extractionConfigurationChecker =
                    new ExtractionConfigurationChecker(_activator, extractionConfiguration)
                        { CheckDatasets = CheckDatasets };
                extractionConfigurationChecker.Check(notifier);
            }


        foreach (var assoc in _project.ProjectCohortIdentificationConfigurationAssociations)
            if (assoc.CohortIdentificationConfiguration == null)
                if (notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Project contains a reference to a CohortIdentificationConfiguration which does not exist anymore",
                            CheckResult.Fail, null,
                            "Delete orphan ProjectCohortIdentificationConfigurationAssociation?")))
                    assoc.DeleteInDatabase();

        notifier.OnCheckPerformed(new CheckEventArgs("All Project Checks Finished (Not necessarily without errors)",
            CheckResult.Success));
    }
}