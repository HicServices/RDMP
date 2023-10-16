// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using MongoDB.Driver.Core.Servers;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateHoldoutLookup : BasicCommandExecution
{
    private readonly CohortIdentificationConfiguration _cic;
    IBasicActivateItems _activator;
    private DiscoveredServer _server;
    private DbCommand _cmd;
    private DataTable _dataTable;


    public ExecuteCommandCreateHoldoutLookup(IBasicActivateItems activator,
        CohortIdentificationConfiguration cic, AggregateConfiguration ec) : base(activator)
    {
        _cic = cic;
        _activator = activator;
    }

    public override string GetCommandName() => "Create Holdout";

    /// <summary>
    /// Describes in a user friendly way the activity of picking an <see cref="ExternalCohortTable"/>
    /// </summary>
    /// <returns></returns>
    private static DialogArgs GetChooseCohortDialogArgs() =>
        new()
        {
            WindowTitle = "Choose where to save cohort",
            TaskDescription =
                "Select the Cohort Database in which to store the identifiers.  If you have multiple methods of anonymising cohorts or manage different types of identifiers (e.g. CHI lists, ECHI lists and/or BarcodeIDs) then you must pick the Cohort Database that matches your cohort identifier type/anonymisation protocol.",
            EntryLabel = "Select Cohort Database",
            AllowAutoSelect = true
        };

    private DataTable LoadDataTable(DiscoveredServer server, string sql)
    {

        DataTable dt = new DataTable();

        try
        {
            //then execute the command
            using var con = server.GetConnection();
            con.Open();

            _cmd = server.GetCommand(sql, con);
            _cmd.CommandTimeout = 10000;// _timeoutControls.Timeout;

            var a = server.GetDataAdapter(_cmd);

            a.Fill(dt);

            //MorphBinaryColumns(dt);

            //Invoke(new MethodInvoker(() => { dataGridView1.DataSource = dt; }));
            con.Close();
        }
        catch (Exception)
        {
            //todo something sensible
            //ShowFatal(e);
        }
        finally
        {

        }
        return dt;

    }

    public override void Execute()
    {
        base.Execute();
        ExternalCohortTable ect = null;

        SelectOne(GetChooseCohortDialogArgs(),
                    BasicActivator.RepositoryLocator.DataExportRepository,
                    out ect);
        CohortHoldoutLookupRequest holdoutRequest = BasicActivator.GetCohortHoldoutLookupRequest(ect,null,_cic);
        if (holdoutRequest != null)
        {
            var x = new ViewCohortIdentificationConfigurationSqlCollection(_cic);
            string sql = x.GetSql();
            _server = DataAccessPortal
                   .ExpectServer(x.GetDataAccessPoint(), DataAccessContext.InternalDataProcessing,false);
            _server.TestConnection();
            _dataTable = LoadDataTable(_server, sql);
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = _dataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in _dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("test.csv", sb.ToString());
            FileInfo fi = new FileInfo("test.csv");
            FileInfo[] fil =
            {
                fi
            };
            FileCollectionCombineable fcc = new FileCollectionCombineable(fil);

            var z = new ExecuteCommandCreateNewCatalogueByImportingFile(_activator, fcc);
            z.Execute();
        }


        //todo
        // this flow largely works, but obviously needs tidied up and finessed

    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.FrozenCohortIdentificationConfiguration);
}