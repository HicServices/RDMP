// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using System;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public abstract class ExecuteCommandSetColumnSettingBase : BasicCommandExecution, IAtomicCommand
{
    private ICatalogue _catalogue;
    private ExtractionInformation[] _extractionInformations;
    private ExtractionInformation[] _alreadyMarked;

    private readonly IExtractionConfiguration _inConfiguration;
    private readonly string _commandName;
    private ConcreteColumn[] _selectedDataSetColumns;
    private ConcreteColumn[] _alreadyMarkedInConfiguration;

    /// <summary>
    /// Explicit columns to pick rather than prompting to choose at runtime
    /// </summary>
    private string[] toPick;

    private readonly string _commandProperty;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="catalogue">The dataset you want to change the setting for</param>
    /// <param name="inConfiguration">Optional - If setting should only be applied to a specific extraction or Null for the Catalogue itself (will affect all future extractions)</param>
    /// <param name="column">"Optional - The Column name(s) you want to select as the new selection(s).  Comma separate multiple entries if needed"</param>
    /// <param name="commandName">Describe what is being changed from user perspective e.g. "Set IsExtractionIdentifier"</param>
    /// <param name="commandProperty">Name of property being changed by this command e.g "Extraction Identifier"</param>
    public ExecuteCommandSetColumnSettingBase(
        IBasicActivateItems activator, ICatalogue catalogue, IExtractionConfiguration inConfiguration, string column,
        string commandName, string commandProperty
    ) : base(activator)
    {
        _catalogue = catalogue;
        _inConfiguration = inConfiguration;
        _commandName = commandName;
        _catalogue.ClearAllInjections();
        _commandProperty = commandProperty;


        if (inConfiguration != null)
        {
            SetImpossibleIfReadonly(_inConfiguration);

            var allEds = inConfiguration.GetAllExtractableDataSets();
            var eds = allEds.FirstOrDefault(sds => sds.Catalogue_ID == _catalogue.ID);
            if (eds == null)
            {
                SetImpossible($"Catalogue '{_catalogue}' is not part of ExtractionConfiguration '{inConfiguration}'");
                return;
            }

            _selectedDataSetColumns = inConfiguration.GetAllExtractableColumnsFor(eds);

            if (_selectedDataSetColumns.Length == 0)
            {
                SetImpossible($"Catalogue '{_catalogue}' in '{inConfiguration}' does not have any extractable columns");
                return;
            }

            _alreadyMarkedInConfiguration = _selectedDataSetColumns.Where(Getter).ToArray();
        }
        else
        {
            _extractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            if (_extractionInformations.Length == 0)
            {
                SetImpossible("Catalogue does not have any extractable columns");
                return;
            }

            _alreadyMarked = _extractionInformations.Where(Getter).ToArray();
        }

        if (!string.IsNullOrWhiteSpace(column)) toPick = column.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }


    public override string GetCommandName()
    {
        if (!string.IsNullOrWhiteSpace(OverrideCommandName))
            return OverrideCommandName;

        var cols = _alreadyMarked ?? _alreadyMarkedInConfiguration;

        return cols == null || cols.Length == 0
            ? _commandName
            : $"{_commandName} ({string.Join(",", cols.Select(e => e.GetRuntimeName()))})";
    }

    public override void Execute()
    {
        base.Execute();

        var oldCols = _alreadyMarked ?? _alreadyMarkedInConfiguration;

        var initialSearchText = oldCols.Length == 1 ? oldCols[0].GetRuntimeName() : null;

        if (_inConfiguration != null)
        {
            ChangeFor(initialSearchText, _selectedDataSetColumns);
            Publish(_inConfiguration);
        }
        else
        {
            ChangeFor(initialSearchText, _extractionInformations);
            Publish(_catalogue);
        }
    }

    private void ChangeFor(string initialSearchText, ConcreteColumn[] allColumns)
    {
        ConcreteColumn[] selected = null;

        if (toPick is { Length: > 0 })
        {
            selected = allColumns.Where(a => toPick.Contains(a.GetRuntimeName())).ToArray();

            if (selected.Length != toPick.Length)
                throw new Exception(
                    $"Could not find column(s) {string.Join(',', toPick)} amongst available columns ({string.Join(',', allColumns.Select(c => c.GetRuntimeName()))})");
        }
        else
        {
            if (SelectMany(new DialogArgs
            {
                InitialObjectSelection = _alreadyMarked ?? _alreadyMarkedInConfiguration,
                AllowSelectingNull = true,
                WindowTitle = $"Set {_commandProperty}",
                TaskDescription =
                        $"Choose which columns will make up the new {_commandProperty}.  Or select null to clear"
            }, allColumns, out selected))
            {
                if (selected == null || selected.Length == 0)
                    if (!YesNo($"Do you want to clear the {_commandProperty}?", $"Clear {_commandProperty}?"))
                        return;
                if (!IsValidSelection(selected))
                    return;
            }
            else
            {
                return;
            }
        }

        foreach (var ec in allColumns)
        {
            var newValue = selected != null && selected.Contains(ec);

            if (Getter(ec) != newValue)
            {
                Setter(ec, newValue);
                ec.SaveToDatabase();
            }
        }
    }

    /// <summary>
    /// Value getter to determine if a given <see cref="ConcreteColumn"/> is included in the current selection for your setting
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    protected abstract bool Getter(ConcreteColumn c);

    /// <summary>
    /// Value setter to assign new inclusion/exclusion status for the column (if command is executed and a new selection confirmed)
    /// </summary>
    /// <param name="c"></param>
    /// <param name="newValue">New status, true = include in selection, false = exclude</param>
    protected abstract void Setter(ConcreteColumn c, bool newValue);

    /// <summary>
    /// Show any warnings if applicable and then return false if user changes their mind about <paramref name="selected"/>
    /// </summary>
    /// <param name="selected"></param>
    /// <returns></returns>
    protected abstract bool IsValidSelection(ConcreteColumn[] selected);
}