// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Deletes objects out of the RDMP database
/// </summary>
public class ExecuteCommandDelete : BasicCommandExecution
{
    private readonly IList<IDeleteable> _deletables;

    /// <summary>
    /// Flag applies only for deletion where the UI layer is non-interactive.  True to allow
    /// multiple deletes to go ahead without asking.  False to throw exception
    /// </summary>
    private readonly bool _allowDeleteMany;

    public ExecuteCommandDelete(IBasicActivateItems activator,
        IDeleteable deletable) : this(activator, new[] { deletable })
    {
        Weight = 50.4f;
    }


    [UseWithObjectConstructor]
    public ExecuteCommandDelete(IBasicActivateItems activator,
        [DemandsInitialization("The object(s) you want to delete.  If multiple you must set deleteMany to true",
            Mandatory = true)]
        IDeleteable[] deletables,
        [DemandsInitialization(
            "Optional.  Pass \"true\" to allow deleting many objects at once e.g. Catalogue:*bob* (deletes all catalogues with the word bob in)")]
        bool deleteMany = false) : base(activator)
    {
        _deletables = deletables;
        _allowDeleteMany = deleteMany;
        if (_deletables.Any(d => d is CohortAggregateContainer c && c.IsRootContainer()))
            SetImpossible("Cannot delete root containers");
        var reason = "";

        if (_deletables.Any(d => d is IMightBeReadOnly ro && ro.ShouldBeReadOnly(out reason)))
            SetImpossible(reason);

        Weight = 50.4f;
    }

    public override string GetCommandName()
    {
        var verb = GetDeleteVerbIfAny();

        return verb ?? base.GetCommandName();
    }

    private string GetDeleteVerbIfAny()
    {
        // Null unless all objects are IDeletableWithCustomMessage
        if (OverrideCommandName != null || _deletables.Count <= 0 ||
            !_deletables.All(static d => d is IDeletableWithCustomMessage)) return null;

        // Get the verbs (e.g. Remove, Disassociate etc)
        var verbs = _deletables.Cast<IDeletableWithCustomMessage>().Select(static d => d.GetDeleteVerb()).Distinct()
            .ToArray();

        // if they agree on one specific verb
        return verbs.Length == 1 ? verbs[0] : null;
    }

    public override void Execute()
    {
        base.Execute();

        // if the thing we are deleting is important and sensitive then we should use a transaction
        if (_deletables.Count > 1 || ShouldUseTransactionsWhenDeleting(_deletables.FirstOrDefault()))
        {
            ExecuteWithCommit(ExecuteImpl, GetDescription(),
                _deletables.OfType<IMapsDirectlyToDatabaseTable>().ToArray());
            PublishNearest();
        }
        else
        {
            ExecuteImpl();
        }
    }

    private static bool ShouldUseTransactionsWhenDeleting(IDeleteable deleteable) =>
        deleteable is CatalogueItem or ExtractionInformation;

    private string GetDescription() =>
        _deletables.Count == 1
            ? $"Delete '{_deletables.Single()}'"
            : $"Delete {_deletables.Count} objects ({_deletables.ToBeautifulString()})";

    private void ExecuteImpl()
    {
        switch (_deletables.Count)
        {
            case 1:
                BasicActivator.DeleteWithConfirmation(_deletables[0]);
                return;
            case <= 0:
                return;
                // Fall through if deleting multiple:
        }

        // if the command did not ask to delete many and it is not interactive (e.g. CLI) then
        // we shouldn't just blindly delete them all
        if (!BasicActivator.IsInteractive && !_allowDeleteMany)
            throw new Exception(
                $"Allow delete many is false but multiple objects were matched for deletion ({string.Join(",", _deletables)})");

        // if it is interactive, only proceed if the user confirms behaviour
        if (BasicActivator.IsInteractive &&
            !YesNo($"{GetDeleteVerbIfAny() ?? "Delete"} {_deletables.Count} Items?", "Delete Items")) return;

        try
        {
            foreach (var d in _deletables.Where(d => d is not DatabaseEntity exists || exists.Exists()))
                d.DeleteInDatabase();
        }
        finally
        {
            PublishNearest();
        }
    }

    private void PublishNearest()
    {
        try
        {
            BasicActivator.PublishNearest(_deletables.FirstOrDefault());
        }
        catch (Exception ex)
        {
            GlobalError("Failed to publish after delete", ex);
        }
    }
}