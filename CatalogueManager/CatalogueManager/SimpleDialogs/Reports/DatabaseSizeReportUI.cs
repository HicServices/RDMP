// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;

namespace CatalogueManager.SimpleDialogs.Reports
{
    /// <summary>
    /// Allows you to generate a report of how big in MB and records each database in your live data repository is.  The tool will evaluate every database on each server for which you
    /// have a TableInfo (See TableInfoTab).  The report will include sizes/row counts of all databases/tables on these servers (not just those managed by the RDMP).
    /// </summary>
    public partial class DatabaseSizeReportUI : RDMPForm
    {
        public DatabaseSizeReportUI(IActivateItems activator):base(activator)
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            var repo = Activator.RepositoryLocator.CatalogueRepository;

            checksUI1.StartChecking(new DatabaseSizeReport(repo.GetAllObjects<TableInfo>().ToArray(),repo));
        }
    }
}
