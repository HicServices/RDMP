// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Records as an ExtendedProperty that a given object is replaced by another.
/// Typically used to forward users of Deprecated items to the new live version
/// </summary>
public class ExecuteCommandReplacedBy : BasicCommandExecution, IAtomicCommand
{
    public IMapsDirectlyToDatabaseTable Deprecated { get; }
    public IMapsDirectlyToDatabaseTable Replacement { get; }


    /// <summary>
    /// True to prompt user to pick and replacement at execute time
    /// </summary>
    public bool PromptToPickReplacement { get; set; }

    [UseWithObjectConstructor]
    public ExecuteCommandReplacedBy(IBasicActivateItems activator,
        [DemandsInitialization(
            "The object that is being retired.  If its Type supports being marked IsDeprecated then it must be true")]
        IMapsDirectlyToDatabaseTable deprecated,
        [DemandsInitialization(
            "The object that replaces the retired one.  Pass null to clear the replacement relationship")]
        IMapsDirectlyToDatabaseTable replacement)
        : base(activator)
    {
        Deprecated = deprecated;
        Replacement = replacement;

        var type = deprecated.GetType();

        if (deprecated is IMightBeDeprecated { IsDeprecated: false })
            SetImpossible($"{deprecated} is not marked IsDeprecated so no replacement can be specified");

        if (replacement != null && replacement.GetType() != type)
            SetImpossible($"'{replacement}' cannot replace '{deprecated}' because it is a different object Type");
    }

    public override void Execute()
    {
        base.Execute();

        var rep = Replacement;

        if (PromptToPickReplacement && rep == null)
            if (!BasicActivator.SelectObject(new DialogArgs
            {
                AllowSelectingNull = true
            }, BasicActivator.CoreChildProvider.AllCatalogues.Value, out rep))
                // user cancelled
                return;

        var cataRepo = BasicActivator.RepositoryLocator.CatalogueRepository;
        foreach (var existing in
                 cataRepo.GetExtendedProperties(ExtendedProperty.ReplacedBy, Deprecated))
            // delete any old references to who we are replaced by
            existing.DeleteInDatabase();

        // null means delete relationship and don't create a new one
        if (rep != null)
            // store the ID of the thing that replaces us
            new ExtendedProperty(cataRepo, Deprecated, ExtendedProperty.ReplacedBy, rep.ID);
    }
}