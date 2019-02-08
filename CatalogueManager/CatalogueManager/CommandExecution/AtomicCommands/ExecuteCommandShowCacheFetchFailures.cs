// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandShowCacheFetchFailures : BasicUICommandExecution,IAtomicCommand
    {
        private CacheProgress _cacheProgress;

        public ExecuteCommandShowCacheFetchFailures(IActivateItems activator, CacheProgress cacheProgress):base(activator)
        {
            _cacheProgress = cacheProgress;

            if(_cacheProgress.CacheFetchFailures.All(f => f.ResolvedOn != null))
                SetImpossible("There are no unresolved CacheFetchFailures");
        }

        public override void Execute()
        {
            base.Execute();

            // for now just show a modal dialog with a data grid view of all the failure rows
            var dt = new DataTable("CacheFetchFailure");

            using (var con = Activator.RepositoryLocator.CatalogueRepository.GetConnection())
            {
                var cmd = (SqlCommand)DatabaseCommandHelper.GetCommand("SELECT * FROM CacheFetchFailure WHERE CacheProgress_ID=@CacheProgressID AND ResolvedOn IS NULL", con.Connection);
                cmd.Parameters.AddWithValue("@CacheProgressID", _cacheProgress.ID);
                var reader = cmd.ExecuteReader();
                dt.Load(reader);
            }

            var dgv = new DataGridView { DataSource = dt, Dock = DockStyle.Fill };
            var form = new Form { Text = "Cache Fetch Failures for " + _cacheProgress.LoadProgress.Name };
            form.Controls.Add(dgv);
            form.Show();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(_cacheProgress);
        }
    }
}