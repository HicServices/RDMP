// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using SixLabors.ImageSharp;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI;
using ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.UI.CommandExecution.AtomicCommands;

public class ExecuteCommandExecuteExtractionConfiguration:BasicUICommandExecution,IAtomicCommandWithTarget
{
    private ExtractionConfiguration _extractionConfiguration;
    private SelectedDataSets _selectedDataSet;
    private Project _project;

    [UseWithObjectConstructor]
    public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : this(activator)
    {
        _extractionConfiguration = extractionConfiguration;
    }

    public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, Project project) : this(activator)
    {
        SetTarget(project);
    }

    public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator) : base(activator)
    {
        OverrideCommandName = "Run Extraction...";
    }

    public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, SelectedDataSets selectedDataSet) : this(activator)
    {
        _extractionConfiguration = (ExtractionConfiguration)selectedDataSet.ExtractionConfiguration;
        _selectedDataSet = selectedDataSet;

    }

    public override string GetCommandHelp()
    {
        return "Extract all the datasets in the configuration linking each against the configuration's cohort";
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.ExtractionConfiguration,OverlayKind.Execute);
    }

    public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        _extractionConfiguration = target as ExtractionConfiguration;
        _project = target as Project;

        //if target is ExtractionConfiguration
        if(_extractionConfiguration != null && !_extractionConfiguration.IsExtractable(out string reason))
            SetImpossible(reason);

        if (_project != null && !_project.ExtractionConfigurations.Any(c => c.IsExtractable(out _)))
            SetImpossible("Project has no ExtractionConfigurations in a ready state for extraction");

        return this;
    }
        

    public override void Execute()
    {
        base.Execute();

        if(_project != null && _extractionConfiguration == null)
        {
            var available = _project.ExtractionConfigurations.Where(c=>c.IsExtractable(out _)).Cast<ExtractionConfiguration>().ToArray();
                
            if(available.Any())
                _extractionConfiguration = SelectOne(available);

            if(_extractionConfiguration == null)
                return;
        }

        var ui = Activator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(_extractionConfiguration);

        if (_selectedDataSet != null)
            ui.TickAllFor(_selectedDataSet);
    }
}