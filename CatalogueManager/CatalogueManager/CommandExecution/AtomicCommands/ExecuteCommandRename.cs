// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Aggregation;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;


namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRename : BasicCommandExecution,IAtomicCommand
    {
        private string _newValue;
        private readonly RefreshBus _refreshBus;
        private readonly INamed _nameable;
        private bool _explicitNewValuePassed;

        public ExecuteCommandRename(RefreshBus refreshBus, INamed nameable)
        {
            _refreshBus = refreshBus;
            _nameable = nameable;
        }

        public ExecuteCommandRename(RefreshBus refreshBus, INamed nameable, string newValue):this(refreshBus,nameable)
        {
            _newValue = newValue;
            _explicitNewValuePassed = true;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return null;
        }

        public override void Execute()
        {
            base.Execute();

            if (!_explicitNewValuePassed)
            {
                var dialog = new TypeTextOrCancelDialog("Rename " + _nameable.GetType().Name, "Name", 2000, _nameable.Name);
                if (dialog.ShowDialog() == DialogResult.OK)
                    _newValue = dialog.ResultText;
                else
                    return;
            }

            _nameable.Name = _newValue;
            EnsureNameIfCohortIdentificationAggregate();

            _nameable.SaveToDatabase();
            _refreshBus.Publish(this, new RefreshObjectEventArgs((DatabaseEntity)_nameable));

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