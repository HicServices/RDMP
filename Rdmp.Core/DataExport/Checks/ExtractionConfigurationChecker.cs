// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataExport.Checks
{
    /// <summary>
    /// Checks <see cref="ExtractionConfiguration"/> to ensure they are in a valid state for execution (have a cohort, linkable patient columns etc).  Optionally also checks
    /// each dataset in the configuration with a <see cref="SelectedDataSetsChecker"/>
    /// </summary>
    public class ExtractionConfigurationChecker:ICheckable
    {
        private IExtractionConfiguration _config;

        /// <summary>
        /// True to fetch all <see cref="ISelectedDataSets"/> and check with <see cref="SelectedDataSetsChecker"/>
        /// </summary>
        public bool CheckDatasets { get; set; }

        /// <summary>
        /// True to also check all globals with a <see cref="GlobalExtractionChecker"/>
        /// </summary>
        public bool CheckGlobals { get; set; }

        /// <summary>
        /// Prepares checking of the given <paramref name="config"/>
        /// </summary>
        /// <param name="config"></param>
        public ExtractionConfigurationChecker(IExtractionConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Checks that the configuration is in a valid state.  Supports both released (frozen) <see cref="ExtractionConfiguration"/> and unreleased ones.
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            if (_config.IsReleased)
                CheckReleaseConfiguration(notifier);
            else
                CheckInProgressConfiguration(notifier);
        }

        private void CheckReleaseConfiguration(ICheckNotifier notifier)
        {
            var projectDirectory = new DirectoryInfo(_config.Project.ExtractionDirectory);

            notifier.OnCheckPerformed(new CheckEventArgs("Found Frozen/Released configuration '" + _config + "'", CheckResult.Success));

            foreach (DirectoryInfo directoryInfo in projectDirectory.GetDirectories(ExtractionDirectory.GetExtractionDirectoryPrefix(_config) + "*").ToArray())
            {
                string firstFileFound;

                if (DirectoryIsEmpty(directoryInfo, out firstFileFound))
                {
                    bool deleteIt =
                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                "Found empty folder " + directoryInfo.Name +
                                " which is left over extracted folder after data release", CheckResult.Warning, null,
                                "Delete empty folder"));

                    if (deleteIt)
                        directoryInfo.Delete(true);
                }
                else
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Found non-empty folder " + directoryInfo.Name +
                            " which is left over extracted folder after data release (First file found was '" + firstFileFound + "' but there may be others)", CheckResult.Fail));
            }
        }

        private bool DirectoryIsEmpty(DirectoryInfo d, out string firstFileFound)
        {
            var found = d.GetFiles().FirstOrDefault();
            if (found != null)
            {
                firstFileFound = found.FullName;
                return false;
            }

            foreach (DirectoryInfo directory in d.GetDirectories())
                if (!DirectoryIsEmpty(directory, out firstFileFound))
                    return false;

            firstFileFound = null;
            return true;
        }

        private void CheckInProgressConfiguration(ICheckNotifier notifier)
        {
            var repo = (DataExportRepository)_config.Repository;
            notifier.OnCheckPerformed(new CheckEventArgs("Found configuration '" + _config + "'", CheckResult.Success));

            var datasets = _config.GetAllExtractableDataSets().ToArray();

            foreach (ExtractableDataSet dataSet in datasets)
                if (dataSet.DisableExtraction)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            "Dataset " + dataSet +
                            " is set to DisableExtraction=true, probably someone doesn't want you extracting this dataset at the moment",
                            CheckResult.Fail));

            if (!datasets.Any())
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "There are no datasets selected for open configuration '" + _config + "'",
                        CheckResult.Fail));

            if (_config.Cohort_ID == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Open configuration '" + _config + "' does not have a cohort yet",
                        CheckResult.Fail));
                return;
            }

            //make sure that it's cohort is retrievable
            repo.GetObjectByID<ExtractableCohort>((int)_config.Cohort_ID);

            if (CheckDatasets)
                foreach (ISelectedDataSets s in _config.SelectedDataSets)
                    new SelectedDataSetsChecker(s).Check(notifier);

            //globals
            if (CheckGlobals)
                if (datasets.Any())
                    foreach (SupportingSQLTable table in _config.GetGlobals().OfType<SupportingSQLTable>())
                        new SupportingSQLTableChecker(table).Check(notifier);
        }
    }
}