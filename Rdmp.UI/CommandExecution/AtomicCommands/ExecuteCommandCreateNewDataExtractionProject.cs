// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Wizard;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandCreateNewDataExtractionProject : BasicUICommandExecution
{
    /// <summary>
    /// The folder to put the new <see cref="Project"/> in.  Defaults to <see cref="FolderHelper.Root"/>
    /// </summary>
    public string Folder { get; set; } = FolderHelper.Root;

    public ExecuteCommandCreateNewDataExtractionProject(IActivateItems activator) : base(activator)
    {
        UseTripleDotSuffix = true;
    }

    public override void Execute()
    {
        base.Execute();
        var wizard = new CreateNewDataExtractionProjectUI(Activator);
        if (wizard.ShowDialog() == DialogResult.OK && wizard.ProjectCreatedIfAny != null)
        {
            var p = wizard.ProjectCreatedIfAny;

            p.Folder = Folder;
            p.SaveToDatabase();

            Publish(p);
            Activator.RequestItemEmphasis(this, new EmphasiseRequest(p, int.MaxValue));

            if (wizard.ExtractionConfigurationCreatedIfAny != null)
            {
                //now execute it
                var executeCommand =
                    new ExecuteCommandExecuteExtractionConfiguration(Activator).SetTarget(
                        wizard.ExtractionConfigurationCreatedIfAny);
                if (!executeCommand.IsImpossible)
                    executeCommand.Execute();
            }
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        iconProvider.GetImage(RDMPConcept.Project, OverlayKind.Add);

    public override string GetCommandHelp() =>
        "This will open a window which will guide you in the steps for creating a Data Extraction Project.\r\n" +
        "You will be asked to choose a Cohort, the Catalogues to extract and the destination folder.";
}