// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRename : BasicUICommandExecution,IAtomicCommand
    {
        private string _newValue;
        private readonly INamed _nameable;
        private bool _explicitNewValuePassed;

        public ExecuteCommandRename(IActivateItems activator, INamed nameable):base(activator)
        {
            _nameable = nameable;

            if(nameable is ITableInfo)
                SetImpossible("TableInfos cannot not be renamed");
        }

        public ExecuteCommandRename(IActivateItems activator, INamed nameable, string newValue):this(activator,nameable)
        {
            _newValue = newValue;
            _explicitNewValuePassed = true;
        }

        public override void Execute()
        {
            base.Execute();

            if (!_explicitNewValuePassed)
            {
                var dialog = new TypeTextOrCancelDialog("Rename " + _nameable.GetType().Name, "Name", 2000, _nameable.Name);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    while(UsefulStuff.IsBadName(dialog.ResultText))
                    {
                        
                        if(YesNo("Name contains illegal characters, do you want to use it anyway?","Bad Name"))
                        {
                            //user wants to use the name anyway
                            break;
                        }
                            
                        //user does not want to use the bad name

                        //type a new one then
                        dialog = new TypeTextOrCancelDialog("Rename " + _nameable.GetType().Name, "Name", 2000, dialog.ResultText);

                        //no? in that case lets just give up altogether
                        if(dialog.ShowDialog() != DialogResult.OK)
                            return;
                    }
                    _newValue = dialog.ResultText;
                }
                else
                    return;
            }

            _nameable.Name = _newValue;
            EnsureNameIfCohortIdentificationAggregate();

            _nameable.SaveToDatabase();
            Publish((DatabaseEntity)_nameable);

        }

        private void EnsureNameIfCohortIdentificationAggregate()
        {
            //handle Aggregates that are part of cohort identification
            var aggregate = _nameable as AggregateConfiguration;
            if (aggregate != null)
            {
                var cic = aggregate.GetCohortIdentificationConfigurationIfAny();

                if (cic != null)
                    cic.EnsureNamingConvention(aggregate);
            }
        }
    }
}