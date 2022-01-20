// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandOpenExtractionDirectory : BasicUICommandExecution,IAtomicCommand
    {
        private FileInfo _file;
        private DirectoryInfo _dir;

        public ExecuteCommandOpenExtractionDirectory(IActivateItems activator, ISelectedDataSets sds):base(activator)
        {
            var result = sds.GetCumulativeExtractionResultsIfAny();
            try
            {
                if(result == null)
                    SetImpossible("Dataset has not been extracted");
                else
                if(result.DestinationType == null)
                    SetImpossible("This extraction has not been run");
                else if (!result.DestinationType.EndsWith("FlatFileDestination"))
                    SetImpossible($"Extraction destination was '{result.DestinationType}' so cannot be opened");
                else
                {
                    _file = new FileInfo(result.DestinationDescription);

                    if(!_file.Exists)
                        SetImpossible($"File '{_file.FullName}' did not exist on disk");
                }
            }
            catch (Exception)
            {
                SetImpossible("Could not determine file location");
            }

        }

        public ExecuteCommandOpenExtractionDirectory(IActivateItems activator, IExtractionConfiguration configuration) : base(activator)
        {
            var cumulativeExtractionResults = configuration.SelectedDataSets.Select(s=>s.GetCumulativeExtractionResultsIfAny()).Where(c=>c!=null).ToArray();
            try
            {
                if (cumulativeExtractionResults.Length == 0)
                    SetImpossible("No datasets have ever been extracted");
                else
                if (!cumulativeExtractionResults.All(c=>c.DestinationType != null && c.DestinationType.EndsWith("FlatFileDestination")))
                    SetImpossible("Extraction destinations were not to disk");
                else
                {
                    // all datasets have been extracted to disk

                    // but do they have a shared parent dir?
                    var files = cumulativeExtractionResults.Select(c => new FileInfo(c.DestinationDescription)).ToArray();

                    var parents = files.Select(f => f.Directory?.Parent?.FullName).Where(d=>d != null).Distinct().ToArray();

                    if (parents.Length != 1)
                        SetImpossible($"Extracted files do not share a common extraction directory");
                    else
                    {
                        _dir = new DirectoryInfo(parents[0]);
                    }
                        
                }
            }
            catch (Exception)
            {
                SetImpossible("Could not determine file location");
            }

        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionDirectoryNode);
        }

        public override void Execute()
        {
            base.Execute();

            var cmd = _file != null?
                new ExecuteCommandOpenInExplorer(Activator, _file):
                new ExecuteCommandOpenInExplorer(Activator, _dir);
            
            if(!cmd.IsImpossible)
                cmd.Execute();
        }
    }
}