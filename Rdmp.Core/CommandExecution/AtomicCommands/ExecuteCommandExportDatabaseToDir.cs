// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

// Dump all compatible objects from the current platform database(s) into a
// YAML/JSON directory for SQL-free operation.
public class ExecuteCommandExportDatabaseToDir : BasicCommandExecution
{
    private readonly IBasicActivateItems _activator;
    private readonly DirectoryInfo _target;

    public ExecuteCommandExportDatabaseToDir(IBasicActivateItems activator, [DemandsInitialization("Where the platform directory should be created")] string target)
    {
        _target = new DirectoryInfo(target);
        _activator = activator;
    }

    private readonly List<string> ignoreList = new() { "Rdmp.Core.DataQualityEngine.Data.DQEGraphAnnotation", "Rdmp.Core.DataQualityEngine.Data.Evaluation" };

    public override void Execute()
    {
        base.Execute();
        var repo = new YamlRepository(_target);
        foreach (var t in repo.GetCompatibleTypes())
        {
            if (ignoreList.Contains(t.FullName)) continue;
            try
            {
                Console.WriteLine(t.FullName);
                foreach (var o in _activator.GetRepositoryFor(t).GetAllObjects(t))
                    repo.SaveToDatabase(o);
            }
            catch(Exception)
            {
                Console.WriteLine($"Unable to find repo for {t.FullName}");
            }

        }
    }
}