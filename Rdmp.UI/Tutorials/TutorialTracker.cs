// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TransparentHelpSystem;
using Rdmp.UI.TransparentHelpSystem.ProgressTracking;

namespace Rdmp.UI.Tutorials;

/// <summary>
/// Collection of <see cref="Tutorial"/>.  Manages marking them complete, launching them etc.
/// </summary>
public class TutorialTracker : IHelpWorkflowProgressProvider
{
    private readonly IActivateItems _activator;
        
    public List<Tutorial> TutorialsAvailable { get; private set; }

    public TutorialTracker(IActivateItems activator)
    {
        _activator = activator;
            
        BuildTutorialList();
    }

    private void BuildTutorialList()
    {
        TutorialsAvailable = new List<Tutorial>();
            
        TutorialsAvailable.Add(new Tutorial("1. Generate Test Data", new ExecuteCommandGenerateTestDataUI(_activator), new Guid("8255fb4e-94a4-4bbc-9e8d-edec5ecebab0")));
        TutorialsAvailable.Add(new Tutorial("2. Import a file", new ExecuteCommandCreateNewCatalogueByImportingFile(_activator), new Guid("5d71a169-5c08-4c33-8f88-8ee123222a3b")));

        //var executeExtraction = new Tutorial("4. Execute DataSet Extraction",
        //                                     new ExecuteCommandExecuteExtractionConfiguration(_activator),
        //                                     new Guid("ee8c290e-7905-4241-9b9a-0ba944fd1582"))
        //    {
        //        UserHasSeen = true // this tutorial is only available on demand
        //    };
        //TutorialsAvailable.Add(executeExtraction);
    }

    public bool ShouldShowUserWorkflow(HelpWorkflow workflow)
    {
        //all tutorials disabled
        if (UserSettings.DisableTutorials)
            return false;
            
        return !UserSettings.GetTutorialDone(GetTutorialGuidFromWorkflow(workflow));
    }

    private Guid GetTutorialGuidFromWorkflow(HelpWorkflow workflow)
    {
        //if the workflow has a guid then it isn't associated with a specific command
        if (workflow.WorkflowGuid != Guid.Empty)
            return workflow.WorkflowGuid;

        //workflow is associated with a specific Command, so it should have a Tutorial Available
        var tutorial = TutorialsAvailable.FirstOrDefault(t => t.CommandType == workflow.Command.GetType());

        if (tutorial == null)
            return Guid.Empty;

        return tutorial.Guid;
    }

    public void Completed(HelpWorkflow helpWorkflow)
    {
        UserSettings.SetTutorialDone(GetTutorialGuidFromWorkflow(helpWorkflow),true);
    }

    public void ClearCompleted()
    {
        foreach (var tutorial in TutorialsAvailable)
            UserSettings.SetTutorialDone(tutorial.Guid, false);
    }

    public static void ClearCompleted(Tutorial tutorial)
    {
        UserSettings.SetTutorialDone(tutorial.Guid, false);
        UserSettings.DisableTutorials = false;
    }

    public static void DisableAllTutorials()
    {
        UserSettings.DisableTutorials = true;
    }

    public static bool HasSeen(Tutorial tutorial)
    {
        return UserSettings.GetTutorialDone(tutorial.Guid);
    }

    public static void LaunchTutorial(Tutorial tutorial)
    {
        tutorial.CommandExecution.Execute();
    }

    public bool IsClearable()
    {
        //any that are true
        return TutorialsAvailable.Any(t=>UserSettings.GetTutorialDone(t.Guid));
    }
}