// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Reports.ExtractionTime;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using ReusableLibraryCode.Icons.IconProvision;


namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandGenerateReleaseDocument : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;

        public ExecuteCommandGenerateReleaseDocument(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
            /////////////////Other stuff///////////
            if(!extractionConfiguration.CumulativeExtractionResults.Any())
                SetImpossible("No datasets have been extracted");
        }

        public override string GetCommandHelp()
        {
            return "Generate a document describing what has been extracted so far for each dataset in the extraction configuration including number of rows, distinct patient counts etc";
        }

        public override void Execute()
        {
            base.Execute();

            try
            {
                ReleaseRunner.IdentifyAndRemoveOldExtractionResults(Activator.RepositoryLocator,new PopupChecksUI("Checking Release Logs",true),_extractionConfiguration );

            }
            catch (Exception e)
            {
                ShowException("Error checking for stale extraction logs",e);
            }

            try
            {
                WordDataReleaseFileGenerator generator = new WordDataReleaseFileGenerator(_extractionConfiguration, Activator.RepositoryLocator.DataExportRepository);
                
                //null means leave word file on screen and dont save
                generator.GenerateWordFile(null);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return FamFamFamIcons.page_white_word;
        }
    }
}