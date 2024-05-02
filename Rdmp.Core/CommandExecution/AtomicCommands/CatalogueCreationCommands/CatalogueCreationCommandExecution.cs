// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;

public abstract class CatalogueCreationCommandExecution : BasicCommandExecution, IAtomicCommandWithTarget
{
    protected IProject ProjectSpecific;

    public string TargetFolder { get; set; }

    protected const string Desc_ProjectSpecificParameter =
        "Optionally associate the Catalogue created with a specific Project, otherwise Null";

    protected const string Desc_TargetFolder =
        "Optionally create the Catalogue in a virtual subdirectory e.g. /mycatalogues/, otherwise Null";

    /// <summary>
    ///     Create a project specific Catalogue when command is executed by prompting the user to first pick a project
    /// </summary>
    public bool PromptForProject { get; set; }

    protected CatalogueCreationCommandExecution(IBasicActivateItems activator) : this(activator, null, null)
    {
    }

    protected CatalogueCreationCommandExecution(IBasicActivateItems activator, IProject projectSpecific,
        string targetFolder) : base(activator)
    {
        ProjectSpecific = projectSpecific;
        TargetFolder = targetFolder;
    }

    public virtual IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
    {
        if (target is Project project)
            ProjectSpecific = project;

        return this;
    }

    public override void Execute()
    {
        base.Execute();

        if (PromptForProject)
            if (SelectOne(BasicActivator.RepositoryLocator.DataExportRepository, out Project p))
                ProjectSpecific = p;
    }
}