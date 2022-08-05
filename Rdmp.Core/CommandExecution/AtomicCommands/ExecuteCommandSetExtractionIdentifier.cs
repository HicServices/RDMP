// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public abstract class ExecuteCommandSetColumnSettingBase : BasicCommandExecution, IAtomicCommand
    {
        private ICatalogue _catalogue;
        private ExtractionInformation[] _extractionInformations;
        private ExtractionInformation[] _alreadyMarked;

        private readonly IExtractionConfiguration _inConfiguration;
        private readonly string _commandName;
        private ConcreteColumn[] _selectedDataSetColumns;
        private ConcreteColumn[] _alreadyMarkedInConfiguration;
        
        /// <summary>
        /// Explicit columns to pick rather than prompting to choose at runtime
        /// </summary>
        private string[] toPick;
        private readonly string _commandProperty;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="catalogue">The dataset you want to change the setting for</param>
        /// <param name="inConfiguration">Optional - If setting should only be applied to a specific extraction or Null for the Catalogue itself (will affect all future extractions)</param>
        /// <param name="column">"Optional - The Column name(s) you want to select as the new selection(s).  Comma seperate multiple entries if needed"</param>
        /// <param name="commandName">Describe what is being changed from user perspective e.g. "Set IsExtractionIdentifier"</param>
        /// <param name="commandProperty">Name of property being changed by this command e.g "Extraction Identifier"</param>
        public ExecuteCommandSetColumnSettingBase(
            IBasicActivateItems activator,ICatalogue catalogue,IExtractionConfiguration inConfiguration,string column,
            string commandName, string commandProperty
        ) : base(activator)
        {
            _catalogue = catalogue;
            _inConfiguration = inConfiguration;
            _commandName = commandName;
            _catalogue.ClearAllInjections();
            _commandProperty = commandProperty;

            if (inConfiguration != null)
            {
                SetImpossibleIfReadonly(_inConfiguration);

                var allEds = inConfiguration.GetAllExtractableDataSets();
                var eds = allEds.FirstOrDefault(sds => sds.Catalogue_ID == _catalogue.ID);
                if (eds == null)
                {
                    SetImpossible($"Catalogue '{_catalogue}' is not part of ExtractionConfiguration '{inConfiguration}'");
                    return;
                }

                _selectedDataSetColumns = inConfiguration.GetAllExtractableColumnsFor(eds);

                if (_selectedDataSetColumns.Length == 0)
                {
                    SetImpossible($"Catalogue '{_catalogue}' in '{inConfiguration}' does not have any extractable columns");
                    return;
                }

                _alreadyMarkedInConfiguration = _selectedDataSetColumns.Where(c => Getter(c)).ToArray();
            }
            else
            {
                _extractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

                if (_extractionInformations.Length == 0)
                {
                    SetImpossible("Catalogue does not have any extractable columns");
                    return;
                }

                _alreadyMarked = _extractionInformations.Where(c=>Getter(c)).ToArray();
            }

            if (!string.IsNullOrWhiteSpace(column))
            {
                toPick = column.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
        }


        public override string GetCommandName()
        {
            if (!string.IsNullOrWhiteSpace(OverrideCommandName))
                return OverrideCommandName;

            var cols = _alreadyMarked ?? _alreadyMarkedInConfiguration;

            if (cols == null || cols.Length == 0)
                return _commandName;

            return _commandName + " (" + string.Join(",", cols.Select(e => e.GetRuntimeName())) + ")";
        }
        public override void Execute()
        {
            base.Execute();

            var oldCols = _alreadyMarked ?? _alreadyMarkedInConfiguration;

            string initialSearchText = oldCols.Length == 1 ? oldCols[0].GetRuntimeName() : null;

            if (_inConfiguration != null)
            {
                ChangeFor(initialSearchText, _selectedDataSetColumns);
                Publish(_inConfiguration);
            }
            else
            {
                ChangeFor(initialSearchText, _extractionInformations);
                Publish(_catalogue);
            }
        }
        private void ChangeFor(string initialSearchText, ConcreteColumn[] allColumns)
        {
            ConcreteColumn[] selected = null;

            if (toPick != null && toPick.Length > 0)
            {
                selected = allColumns.Where(a => toPick.Contains(a.GetRuntimeName())).ToArray();

                if (selected.Length != toPick.Length)
                {
                    throw new Exception($"Could not find column(s) {string.Join(',', toPick)} amongst available columns ({string.Join(',', allColumns.Select(c => c.GetRuntimeName()))})");
                }
            }
            else
            {
                if (SelectMany(allColumns, out selected, initialSearchText))
                {
                    if (selected.Length == 0)
                        if (!YesNo($"Do you want to clear the {_commandProperty}?", $"Clear {_commandProperty}?"))
                            return;
                    if(!ValidateSelection(selected))
                        return;
                }
                else
                    return;
            }

            foreach (var ec in allColumns)
            {
                bool newValue = selected.Contains(ec);

                if (ec.IsExtractionIdentifier != newValue)
                {
                    Setter(ec,newValue);
                    ec.SaveToDatabase();
                }
            }
        }

        /// <summary>
        /// Value getter to determine if a given <see cref="ConcreteColumn"/> is included in the current selection for your setting
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        protected abstract bool Getter(ConcreteColumn c);

        /// <summary>
        /// Value setter to assign new inclusion/exclusion status for the column (if command is executed and a new selection confirmed)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="newValue">New status, true = include in selection, false = exclude</param>
        protected abstract void Setter(ConcreteColumn c, bool newValue);

        /// <summary>
        /// Show any warnings if applicable and then return false if user changes their mind about <paramref name="selected"/>
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        protected abstract bool ValidateSelection(ConcreteColumn[] selected);
    }

    public class ExecuteCommandSetExtractionPrimaryKeys : ExecuteCommandSetColumnSettingBase, IAtomicCommand
    {
        /// <summary>
        /// Change which column(s) should be marked as primary key on extraction to a destination that supports this feature (e.g. to database).
        /// Operation can be applied either at global <see cref="Catalogue"/> level or for a specific <paramref name="inConfiguration"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="catalogue"></param>
        /// <param name="inConfiguration"></param>
        /// <param name="column"></param>
        public ExecuteCommandSetExtractionPrimaryKeys(IBasicActivateItems activator,
            [DemandsInitialization("The dataset you want to change the extraction primary keys for")]
            ICatalogue catalogue,

            [DemandsInitialization("Optional - The specific extraction you want the change made in or Null for the Catalogue itself (will affect all future extractions)")]
            IExtractionConfiguration inConfiguration,

            [DemandsInitialization("Optional - The Column name(s) you want to select as the new extraction primary keys.  Comma seperate multiple entries if needed")]
            string column)
                // base class args
                : base(activator, catalogue, inConfiguration, column,
                "Set Extraction Primary Key",
                "Extraction Primary Key")
        {


        }
        public override string GetCommandHelp()
        {
            return "Change which column(s) should be marked as primary key if extracting to database";
        }

        protected override bool ValidateSelection(ConcreteColumn[] selected)
        {
            return true;
        }
        protected override bool Getter(ConcreteColumn c)
        {
            return c.IsPrimaryKey;
        }
        protected override void Setter(ConcreteColumn ec, bool newValue)
        {
            ec.IsPrimaryKey = newValue;
        }
    }
    /// <summary>
    /// Change which column is used to perform linkage against a cohort.  This command supports both changing the global setting on a <see cref="Catalogue"/>
    /// or changing it only for a specific <see cref="ExtractionConfiguration"/>
    /// </summary>
    public class ExecuteCommandSetExtractionIdentifier : ExecuteCommandSetColumnSettingBase, IAtomicCommand
    {
        /// <summary>
        /// Change which column is the linkage identifier in a <see cref="Catalogue"/> either at a global level or for a specific <paramref name="inConfiguration"/>
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="catalogue"></param>
        /// <param name="inConfiguration"></param>
        /// <param name="column"></param>
        public ExecuteCommandSetExtractionIdentifier(IBasicActivateItems activator,
            [DemandsInitialization("The dataset you want to change the extraction identifier for")]
            ICatalogue catalogue,

            [DemandsInitialization("Optional - The specific extraction you want the change made in or Null for the Catalogue itself (will affect all future extractions)")]
            IExtractionConfiguration inConfiguration,

            [DemandsInitialization("Optional - The Column name(s) you want to select as the new linkage identifier(s).  Comma seperate multiple entries if needed")]
            string column) 
                // base class args
                :base(activator, catalogue, inConfiguration,column,
                "Set Extraction Identifier",
                "Extraction Identifier")
        {
            
            
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort,OverlayKind.Key);
        }

        public override string GetCommandHelp()
        {
            return "Change which column(s) contain the patient id / linkage column e.g. CHI";
        }

        protected override bool ValidateSelection(ConcreteColumn[] selected)
        {
            // if multiple selected warn user
            if (selected.Length > 1)
                return !YesNo("Are you sure you want multiple linkable extraction identifier columns (most datasets only have 1 person ID column in them)?", "Multiple IsExtractionIdentifier columns?");

            return true;
        }

        protected override bool Getter(ConcreteColumn c)
        {
            return c.IsExtractionIdentifier;
        }
        protected override void Setter(ConcreteColumn c, bool newValue)
        {
            c.IsExtractionIdentifier = newValue;
        }
    }
}