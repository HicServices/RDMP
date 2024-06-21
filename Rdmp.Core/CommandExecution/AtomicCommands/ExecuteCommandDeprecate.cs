// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using System.Linq;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandDeprecate : BasicCommandExecution
{
    private readonly IMightBeDeprecated[] _o;
    private readonly bool _desiredState;
    private readonly IBasicActivateItems _activeItems;


    [UseWithObjectConstructor]
    public ExecuteCommandDeprecate(IBasicActivateItems itemActivator,
        [DemandsInitialization("The object you want to deprecate/undeprecate")]
        IMightBeDeprecated[] o,
        [DemandsInitialization("True to deprecate.  False to undeprecate", DefaultValue = true)]
        bool desiredState = true) : base(itemActivator)
    {
        _o = o;
        _desiredState = desiredState;
        _activeItems = itemActivator;
    }

    public override string GetCommandName() => !string.IsNullOrEmpty(OverrideCommandName) ? OverrideCommandName :
        _desiredState ? "Deprecate" : "Undeprecate";

    public override void Execute()
    {
        base.Execute();

        if (_o == null || _o.Length == 0)
            return;

        if (ExecuteWithCommit(ExecuteImpl, GetDescription(), _o)) Publish((DatabaseEntity)_o[0]);
    }

    private void ExecuteImpl()
    {
        foreach (var o in _o)
        {
            o.IsDeprecated = _desiredState;
            o.SaveToDatabase();
            if (!_desiredState && o.GetType() == typeof(Catalogue))//false is not-depricated
            {
                var c = (Catalogue) o;
                var replacedBy = _activeItems.RepositoryLocator.CatalogueRepository.GetExtendedProperties(ExtendedProperty.ReplacedBy);
                var replacement = replacedBy.Where(rb => rb.ReferencedObjectID == c.ID).FirstOrDefault();
                replacement.DeleteInDatabase();
            }
        }

       

        if (!BasicActivator.IsInteractive || _o.Length != 1 || _o[0] is not Catalogue || !_desiredState ||
            !BasicActivator.YesNo("Do you have a replacement Catalogue you want to link?", "Replacement")) return;
        var cmd = new ExecuteCommandReplacedBy(BasicActivator, _o[0], null)
        {
            PromptToPickReplacement = true
        };
        cmd.Execute();
    }

    private string GetDescription()
    {
        var verb = _desiredState ? "Deprecate" : "UnDeprecate";
        var noun = _o.Length == 1 ? _o[0].ToString() : $"{_o.Length} objects";

        return $"{verb} {noun}";
    }
}