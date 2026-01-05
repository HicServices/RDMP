// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.Checks;

/// <summary>
/// Checks that all the globals (<see cref="SupportingDocument"/> / <see cref="SupportingSQLTable"/>) that would be fetched as part of an
/// <see cref="ExtractionConfiguration"/> are accessible.
/// </summary>
public class GlobalExtractionChecker : ICheckable
{
    private readonly ExtractionConfiguration _configuration;
    private readonly ExtractGlobalsCommand _command;
    private readonly IPipeline _alsoCheckPipeline;
    private readonly IBasicActivateItems _activator;

    /// <summary>
    /// Prepares to check the globals extractable artifacts that should be fetched when extracting the <paramref name="configuration"/>
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="configuration"></param>
    public GlobalExtractionChecker(IBasicActivateItems activator, ExtractionConfiguration configuration) : this(
        activator, configuration, null, null)
    {
    }


    /// <inheritdoc cref="GlobalExtractionChecker(IBasicActivateItems,ExtractionConfiguration)"/>
    public GlobalExtractionChecker(IBasicActivateItems activator, ExtractionConfiguration configuration,
        ExtractGlobalsCommand command, IPipeline alsoCheckPipeline)
    {
        _configuration = configuration;
        _command = command;
        _alsoCheckPipeline = alsoCheckPipeline;
        _activator = activator;
    }

    /// <summary>
    /// Checks that all globals pass their respective checkers (<see cref="SupportingSQLTableChecker"/> and <see cref="SupportingDocumentsFetcher"/>) and that
    /// the <see cref="Pipeline"/> (if any) is capable of extracting the globals.
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        foreach (var table in _configuration.GetGlobals().OfType<SupportingSQLTable>())
            new SupportingSQLTableChecker(table).Check(notifier);

        foreach (var document in _configuration.GetGlobals().OfType<SupportingDocument>())
            new SupportingDocumentsFetcher(document).Check(notifier);

        if (_alsoCheckPipeline != null && _command != null)
        {
            var engine = new ExtractionPipelineUseCase(_activator, _configuration.Project, _command, _alsoCheckPipeline,
                    DataLoadInfo.Empty)
                .GetEngine(_alsoCheckPipeline, new FromCheckNotifierToDataLoadEventListener(notifier));
            engine.Check(notifier);
        }
    }
}