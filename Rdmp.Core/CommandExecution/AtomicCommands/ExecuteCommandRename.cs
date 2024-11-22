// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandRename : BasicCommandExecution
{
    private string _newValue;
    private readonly INamed _nameable;
    private bool _explicitNewValuePassed;

    public ExecuteCommandRename(IBasicActivateItems activator, INamed nameable) : base(activator)
    {
        _nameable = nameable;

        switch (nameable)
        {
            case ITableInfo:
                SetImpossible("TableInfos cannot not be renamed");
                break;
            case IMightBeReadOnly ro when ro.ShouldBeReadOnly(out var reason):
                SetImpossible(reason);
                break;
        }

        Weight = 50.2f;
    }

    [UseWithObjectConstructor]
    public ExecuteCommandRename(IBasicActivateItems activator, INamed nameable, string newValue) : this(activator,
        nameable)
    {
        _newValue = newValue;
        _explicitNewValuePassed = true;
    }

    public override void Execute()
    {
        base.Execute();

        if (!_explicitNewValuePassed)
        {
            if (TypeText($"Rename {_nameable.GetType().Name}", "Name", 500, _nameable.Name, out var text))
            {
                while (UsefulStuff.IsBadName(text))
                {
                    if (YesNo("Name contains illegal characters, do you want to use it anyway?", "Bad Name"))
                        //user wants to use the name anyway
                        break;

                    //user does not want to use the bad name

                    //type a new one then

                    if (!TypeText($"Rename {_nameable.GetType().Name}", "Name", 2000, _nameable.Name, out text))
                        return;
                }

                _newValue = text;
            }
            else
            {
                return;
            }
        }

        _nameable.Name = _newValue;
        EnsureNameIfCohortIdentificationAggregate();

        _nameable.SaveToDatabase();
        Publish((DatabaseEntity)_nameable);
    }

    private void EnsureNameIfCohortIdentificationAggregate()
    {
        //handle Aggregates that are part of cohort identification
        if (_nameable is AggregateConfiguration aggregate)
        {
            var cic = aggregate.GetCohortIdentificationConfigurationIfAny();

            cic?.EnsureNamingConvention(aggregate);
        }
    }
}