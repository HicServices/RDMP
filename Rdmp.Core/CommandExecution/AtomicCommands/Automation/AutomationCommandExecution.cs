// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CommandLine;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Parser = CommandLine.Parser;

namespace Rdmp.Core.CommandExecution.AtomicCommands.Automation;

public abstract class AutomationCommandExecution : BasicCommandExecution
{
    protected readonly Func<RDMPCommandLineOptions> CommandGetter;

    public static readonly string AutomationServiceExecutable =
        Environment.OSVersion.Platform == PlatformID.Win32NT ? "rdmp.exe" : "rdmp";

    private readonly TableRepository _cataTableRepo;
    private readonly TableRepository _dataExportTableRepo;
    private readonly YamlRepository _yamlRepository;


    protected AutomationCommandExecution(IBasicActivateItems activator, Func<RDMPCommandLineOptions> commandGetter) :
        base(activator)
    {
        CommandGetter = commandGetter;

        // repository locator must be one of these types for us to properly assemble
        // CLI args
        _cataTableRepo = activator.RepositoryLocator.CatalogueRepository as TableRepository;
        _yamlRepository = activator.RepositoryLocator.CatalogueRepository as YamlRepository;
        _dataExportTableRepo = activator.RepositoryLocator.DataExportRepository as TableRepository;

        if (_yamlRepository == null && (_cataTableRepo == null || _dataExportTableRepo == null))
            SetImpossible("Current repository is not not TableRepository/YamlRepository");
    }

    /// <summary>
    ///     Generates command line arguments for the current engine
    /// </summary>
    /// <param name="argsOnly"></param>
    /// <returns></returns>
    public string GetCommandText(bool argsOnly = false)
    {
        using var p = new Parser();
        var options = CommandGetter();

        PopulateConnectionStringOptions(options);

        return argsOnly
            ? p.FormatCommandLine(options)
            : $"{AutomationServiceExecutable} {p.FormatCommandLine(options)}";
    }

    private void PopulateConnectionStringOptions(RDMPCommandLineOptions options)
    {
        if (BasicActivator == null)
            return;

        // if backing database uses a directory
        if (_yamlRepository != null)
        {
            // assemble CLI args that also say to use a directory
            options.Dir = _yamlRepository.Directory.FullName;
            return;
        }

        // if backing database uses a specific connection string
        // then use the same connection string for CLI args
        if (string.IsNullOrWhiteSpace(options.CatalogueConnectionString))
            options.CatalogueConnectionString = _cataTableRepo.ConnectionStringBuilder.ConnectionString;

        if (string.IsNullOrWhiteSpace(options.DataExportConnectionString))
            options.DataExportConnectionString = _dataExportTableRepo.ConnectionStringBuilder.ConnectionString;
    }
}