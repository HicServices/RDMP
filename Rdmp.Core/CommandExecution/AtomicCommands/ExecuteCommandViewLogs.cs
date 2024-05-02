// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Databases;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandViewLogs : BasicCommandExecution, IAtomicCommand
{
    public ILoggedActivityRootObject RootObject { get; }
    private readonly LogViewerFilter _filter;
    private readonly ExternalDatabaseServer[] _loggingServers;

    [UseWithCommandLine(
        ParameterHelpList = "<root> <table?> <id?>",
        ParameterHelpBreakdown = @"root object to view logs for or logging server
table? Only required if <root> is logging server, specifies the table to view e.g. DataLoadRun
int? Optional, if <root> is logging server this can be a specific audit id to show")]
    public ExecuteCommandViewLogs(IBasicActivateItems activator, CommandLineObjectPicker picker) : base(activator)
    {
        if (picker.Length == 0)
        {
            SetImpossible("Insufficient arguments supplied");
            return;
        }

        if (picker[0].HasValueOfType(typeof(DatabaseEntity)))
        {
            var obj = picker[0].GetValueForParameterOfType(typeof(DatabaseEntity));

            switch (obj)
            {
                case ILoggedActivityRootObject root:
                    RootObject = root;
                    break;
                case ExternalDatabaseServer eds:
                    _loggingServers = new[] { eds };
                    break;
                default:
                    throw new Exception(
                        $"'{obj}' is of type '{obj.GetType().Name}' which is not '{nameof(ILoggedActivityRootObject)}' so cannot be used with this command.");
            }
        }

        var table = LoggingTables.None;

        // Optional second argument: table to filter for
        if (picker.Length >= 1 && Enum.TryParse(picker[1].RawValue, out table)) _filter = new LogViewerFilter(table);

        // Optional third argument: foreign key ID to filter on
        if (picker.Length >= 2 && int.TryParse(picker[2].RawValue, out var id))
            _filter = new LogViewerFilter(table, id);
    }

    [UseWithObjectConstructor]
    public ExecuteCommandViewLogs(IBasicActivateItems activator, ILoggedActivityRootObject rootObject) : base(activator)
    {
        RootObject = rootObject;
    }

    public ExecuteCommandViewLogs(IBasicActivateItems activator) : this(activator,
        new LogViewerFilter(LoggingTables.DataLoadTask))
    {
    }

    public ExecuteCommandViewLogs(IBasicActivateItems activator, ExternalDatabaseServer loggingServer,
        LogViewerFilter filter) : base(activator)
    {
        _filter = filter ?? new LogViewerFilter(LoggingTables.DataLoadTask);
        _loggingServers = new[] { loggingServer };
    }

    public ExecuteCommandViewLogs(IBasicActivateItems activator, LogViewerFilter filter) : base(activator)
    {
        _filter = filter ?? new LogViewerFilter(LoggingTables.DataLoadTask);
        _loggingServers =
            BasicActivator.RepositoryLocator.CatalogueRepository.GetAllDatabases<LoggingDatabasePatcher>();

        if (!_loggingServers.Any())
            SetImpossible("There are no logging servers");
    }

    public override string GetCommandHelp()
    {
        return
            "View the hierarchical audit log of all data flows through RDMP (data load, extraction, dqe runs etc) including progress, errors etc";
    }

    public override void Execute()
    {
        base.Execute();


        if (RootObject != null)
        {
            BasicActivator.ShowLogs(RootObject);
        }
        else
        {
            var server = SelectOne(_loggingServers, null, true);
            BasicActivator.ShowLogs(server, _filter ?? new LogViewerFilter(LoggingTables.DataLoadRun));
        }
    }

    public override string GetCommandName()
    {
        return !string.IsNullOrWhiteSpace(OverrideCommandName)
            ? OverrideCommandName
            : _filter != null
                ? UsefulStuff.PascalCaseStringToHumanReadable(_filter.LoggingTable.ToString())
                : base.GetCommandName();
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.Logging);
    }
}