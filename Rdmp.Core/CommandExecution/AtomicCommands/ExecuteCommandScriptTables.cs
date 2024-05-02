// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Text;
using FAnsi;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Script multiple tables optionally porting schema to a new server/DBMS type
/// </summary>
public class ExecuteCommandScriptTables : BasicCommandExecution
{
    private readonly TableInfo[] _tableInfos;
    private readonly DatabaseType? _dbType;
    private readonly string _dbName;
    private readonly FileInfo _outFile;

    /// <summary>
    ///     Scripts multiple tables schemas and optionally ports datatypes / constraints etc to alternate DBMS
    /// </summary>
    public ExecuteCommandScriptTables(IBasicActivateItems activator,
        [DemandsInitialization("Tables to script")]
        TableInfo[] tableInfos,
        [DemandsInitialization("Optional alternate DBMS Type to port the schema to")]
        DatabaseType? dbType,
        [DemandsInitialization("Optional alternate database name to use in the script generated.")]
        string dbName,
        [DemandsInitialization("Optional file to write the resulting script out to.  Pass Null to output to console.")]
        FileInfo outFile) : base(activator)
    {
        _tableInfos = tableInfos;
        _dbType = dbType;
        _dbName = dbName;
        _outFile = outFile;
    }

    public override string GetCommandHelp()
    {
        return "Scripts multiple tables structure to Clipboard (without dependencies)";
    }

    public override void Execute()
    {
        var sbScript = new StringBuilder();

        foreach (var tableInfo in _tableInfos)
        {
            var tbl = tableInfo.Discover(DataAccessContext.InternalDataProcessing);

            var hypotheticalServer = new DiscoveredServer("localhost", _dbName ?? "None",
                _dbType ?? tableInfo.DatabaseType, null, null);
            var hypotheticalTable = hypotheticalServer.ExpectDatabase(_dbName ?? tbl.Database.GetRuntimeName())
                .ExpectTable(tbl.GetRuntimeName());

            var result = tbl.ScriptTableCreation(false, false, false, hypotheticalTable);
            sbScript.AppendLine(result);
            sbScript.AppendLine();
        }

        if (_outFile != null)
            File.WriteAllText(_outFile.FullName, sbScript.ToString());
        else
            Show($"Script for {_tableInfos.Length} tables", sbScript.ToString());
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return iconProvider.GetImage(RDMPConcept.SQL);
    }
}