// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
///     Abstract base class for all 'view data' style CLI compatible commands
/// </summary>
public abstract class ExecuteCommandViewDataBase : BasicCommandExecution
{
    public const string ToFileDescription = "Optional file to output the results of the query to.  Or null";

    public FileInfo ToFile { get; }

    /// <summary>
    ///     Set to true to prompt user to pick a <see cref="ToFile" /> at command execution time.
    /// </summary>
    public bool AskForFile { get; set; }

    public ExecuteCommandViewDataBase(IBasicActivateItems activator, FileInfo toFile) : base(activator)
    {
        ToFile = toFile;
    }

    /// <summary>
    ///     Get the SQL query to run and where to run it.  Return null to cancel
    ///     data fetching.
    /// </summary>
    /// <returns></returns>
    protected abstract IViewSQLAndResultsCollection GetCollection();


    public override void Execute()
    {
        base.Execute();

        var collection = GetCollection();

        if (collection == null)
            return;


        var toFile = ToFile;

        if (AskForFile)
        {
            toFile = BasicActivator.SelectFile("Save as", "Comma Separated Values", "*.csv");
            if (toFile == null)
                // user cancelled selecting a file
                return;
        }

        if (toFile != null)
            ExtractTableVerbatim.ExtractDataToFile(collection, toFile);
        else
            BasicActivator.ShowData(collection);
    }
}