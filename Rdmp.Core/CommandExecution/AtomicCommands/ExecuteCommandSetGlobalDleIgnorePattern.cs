// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public sealed class ExecuteCommandSetGlobalDleIgnorePattern : BasicCommandExecution
{
    private readonly string _pattern;
    private readonly bool _explicitPatternProvided;

    [UseWithObjectConstructor]
    public ExecuteCommandSetGlobalDleIgnorePattern(IBasicActivateItems activator, string pattern) : base(activator)
    {
        _pattern = pattern;
        //if pattern is null but this constructor is used then we shouldn't ask them again what they want
        _explicitPatternProvided = true;
    }

    /// <summary>
    ///     Constructor for when we should prompt user in Gui for what the pattern should be if/when command is executed
    /// </summary>
    /// <param name="activator"></param>
    public ExecuteCommandSetGlobalDleIgnorePattern(IBasicActivateItems activator) : base(activator)
    {
    }

    public override void Execute()
    {
        base.Execute();

        var existing =
            HICDatabaseConfiguration.GetGlobalIgnorePatternIfAny(BasicActivator.RepositoryLocator.CatalogueRepository);

        if (existing == null)
        {
            existing = new StandardRegex(BasicActivator.RepositoryLocator.CatalogueRepository)
            {
                ConceptName = StandardRegex.DataLoadEngineGlobalIgnorePattern,
                Description = "Regex that will be applied as an ignore when running the data load engine",
                Regex = "^ignore_.*"
            };
            existing.SaveToDatabase();
        }

        if (_explicitPatternProvided)
        {
            existing.Regex = _pattern;
            existing.SaveToDatabase();
        }
        else
        {
            Publish(existing);
            Activate(existing);
        }
    }
}