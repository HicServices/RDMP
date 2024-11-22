// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Threading.Tasks;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Progress;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandRefreshExtractionConfigurationsCohort : BasicUICommandExecution
{
    private readonly ExtractionConfiguration _extractionConfiguration;

    public ExecuteCommandRefreshExtractionConfigurationsCohort(IActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;
        var project = (Project)_extractionConfiguration.Project;

        if (extractionConfiguration.Cohort_ID == null)
            SetImpossible("No Cohort Set");

        if (extractionConfiguration.CohortRefreshPipeline_ID == null)
            SetImpossible("No Refresh Pipeline Set");

        if (!project.ProjectNumber.HasValue)
            SetImpossible($"Project '{project}' does not have a Project Number");
    }

    public override string GetCommandHelp() =>
        "Update the cohort to a new version by rerunning the associated Cohort Identification Configuration (query). " +
        "This is useful if you have to do yearly\\monthly releases and update the cohort based on new data";

    public override void Execute()
    {
        base.Execute();

        //show the ui
        var progressUi = new ProgressUI();
        progressUi.ApplyTheme(Activator.Theme);

        progressUi.Text = $"Refreshing Cohort ({_extractionConfiguration})";
        Activator.ShowWindow(progressUi, true);

        var engine = new CohortRefreshEngine(progressUi, _extractionConfiguration);
        Task.Run(
            //run the pipeline in a Thread
            () =>
            {
                progressUi.ShowRunning(true);
                engine.Execute();
            }
        ).ContinueWith(s =>
        {
            progressUi.ShowRunning(false);

            //then on the UI thread
            if (s.IsFaulted)
                return;

            //issue save and refresh
            if (engine.Request.CohortCreatedIfAny != null)
                Publish(_extractionConfiguration);
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
}