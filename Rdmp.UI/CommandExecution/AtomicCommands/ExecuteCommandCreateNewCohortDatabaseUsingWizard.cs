// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.CohortUI.CohortSourceManagement;
using Rdmp.UI.ItemActivation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandCreateNewCohortDatabaseUsingWizard : BasicUICommandExecution
{
    public ExecuteCommandCreateNewCohortDatabaseUsingWizard(IActivateItems activator) : base(activator)
    {
        UseTripleDotSuffix = true;
    }

    public override string GetCommandHelp() =>
        "Create a new empty cohort list storage database with a private identifier that matches your datasets extraction identifier column name\\type (e.g. PatientId varchar(10)";

    public override void Execute()
    {
        base.Execute();

        var wizard = new CreateNewCohortDatabaseWizardUI(Activator);
        wizard.SetItemActivator(Activator);
        var f = Activator.ShowWindow(wizard, true);
        f.FormClosed += (_, _) =>
        {
            if (wizard.ExternalCohortTableCreatedIfAny != null)
                Publish(wizard.ExternalCohortTableCreatedIfAny);
        };
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) => Image.Load<Rgba32>(FamFamFamIcons.wand);
}