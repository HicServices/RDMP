// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Reports.ExtractionTime;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

internal class ExecuteCommandGenerateReleaseDocument : BasicCommandExecution, IAtomicCommand
{
    private readonly ExtractionConfiguration _extractionConfiguration;

    public ExecuteCommandGenerateReleaseDocument(IBasicActivateItems activator,
        ExtractionConfiguration extractionConfiguration) : base(activator)
    {
        _extractionConfiguration = extractionConfiguration;
        /////////////////Other stuff///////////
        if (!extractionConfiguration.CumulativeExtractionResults.Any())
            SetImpossible("No datasets have been extracted");

        if (_extractionConfiguration.Cohort_ID == null)
            SetImpossible("ExtractionConfiguration does not have a cohort");
        else
            try
            {
                // try to fetch the cohort (give it 2 seconds maximum).
                // we don't want to freeze waiting for context menu to pop up on this
                var eds = _extractionConfiguration.Cohort.GetExternalData(2);

                if (eds == ExternalCohortDefinitionData.Orphan) SetImpossible("Cohort did not exist");
            }
            catch (Exception)
            {
                SetImpossible("Cohort was unreachable");
            }
    }

    public override string GetCommandHelp()
    {
        return
            "Generate a document describing what has been extracted so far for each dataset in the extraction configuration including number of rows, distinct patient counts etc";
    }

    public override void Execute()
    {
        base.Execute();

        try
        {
            ReleaseRunner.IdentifyAndRemoveOldExtractionResults(BasicActivator.RepositoryLocator,
                new AcceptAllCheckNotifier(), _extractionConfiguration);
        }
        catch (Exception e)
        {
            ShowException("Error checking for stale extraction logs", e);
        }

        try
        {
            var generator = new WordDataReleaseFileGenerator(_extractionConfiguration,
                BasicActivator.RepositoryLocator.DataExportRepository);

            //null means leave word file on screen and don't save
            generator.GenerateWordFile(null);
        }
        catch (Exception e)
        {
            BasicActivator.ShowException("Failed to generate release document", e);
        }
    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider)
    {
        return Image.Load<Rgba32>(FamFamFamIcons.page_white_word);
    }
}