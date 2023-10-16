// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using MongoDB.Driver.Core.Servers;
using NPOI.SS.Formula.Functions;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode.Progress;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TB.ComponentModel;
using static MongoDB.Driver.WriteConcern;

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


    private static DialogArgs GetChooseExtractionIdentifier() =>
    new()
    {
        WindowTitle = "Choose Extraction Identifier",
        TaskDescription =
            "TODO",
        EntryLabel = "Choose Extraction Identifier",
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
        CohortHoldoutLookupRequest holdoutRequest = BasicActivator.GetCohortHoldoutLookupRequest(ect, null, _cic);
        if (holdoutRequest != null)
        {
            var x = new ViewCohortIdentificationConfigurationSqlCollection(_cic);
            string sql = x.GetSql();
            _server = DataAccessPortal
                   .ExpectServer(x.GetDataAccessPoint(), DataAccessContext.InternalDataProcessing, false);
            _server.TestConnection();
            _dataTable = LoadDataTable(_server, sql);
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = _dataTable.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));
            _dataTable.Columns.Add("_Holdoutsuffle");
            Random rnd = new Random();
            foreach (DataRow row in _dataTable.Rows)
            {
                row["_Holdoutsuffle"] = rnd.Next();
            }
            //todo
            string whereCondition = "";
            DateTime beforeDate = holdoutRequest.MaxDate;
            DateTime afterDate = holdoutRequest.MinDate;
            string dateColumn = holdoutRequest.DateColumnName;
            bool hasMinDate = false;
            bool hasMaxDate = false;


            if (!columnNames.Contains(dateColumn))
            {
                //todo throw an error
            }
            else
            {
                if (beforeDate.Date != DateTime.MinValue)
                {
                    //has max date
                    hasMaxDate = true;
                }
                if (afterDate.Date != DateTime.MinValue)
                {
                    //has min date
                    hasMinDate = true;
                }
            }


            if (!string.IsNullOrWhiteSpace(whereCondition))
            {
                DataView dv = new DataView(_dataTable);
                dv.RowFilter = whereCondition;
                DataTable dt2 = dv.ToTable();
                dv.Dispose();
                _dataTable = dt2;
                if (dt2.Rows.Count < 1)
                {
                    //todo warn about filtering out all data
                }
            }
            if (hasMinDate || hasMaxDate)
            {
                foreach(DataRow row in _dataTable.Rows)
                {
                    if (hasMaxDate && DateTime.Parse(row[dateColumn].ToString()) > beforeDate) {
                        row.Delete();
                    }
                    else if (hasMinDate && DateTime.Parse(row[dateColumn].ToString()) < afterDate)
                    {
                        row.Delete();
                    }
                }
            }
            _dataTable.DefaultView.Sort = "_Holdoutsuffle";
            _dataTable = _dataTable.DefaultView.ToTable();
            _dataTable.Columns.Remove("_Holdoutsuffle");
            var rows = _dataTable.Rows.Cast<System.Data.DataRow>().Take(holdoutRequest.Count);
            if (holdoutRequest.IsPercent)
            {
                var rowCount = holdoutRequest.Count;
                if (holdoutRequest.Count > 100)
                {
                    //listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Holdout percentage was >100%. Will use 100%"));
                    holdoutRequest.Count = 100;
                }
                rowCount = (int)Math.Ceiling(((float)_dataTable.Rows.Count / 100 * holdoutRequest.Count));
                rows = _dataTable.Rows.Cast<System.Data.DataRow>().Take(rowCount);
            }
            //todo fail is rows are empty
            foreach (DataRow row in rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText($"{holdoutRequest.Name}.csv", sb.ToString());
            FileInfo fi = new FileInfo($"{holdoutRequest.Name}.csv");
            FileInfo[] fil =
            {
                fi
            };
            FileCollectionCombineable fcc = new FileCollectionCombineable(fil);
            //File = file;
            //_targetDatabase = targetDatabase;
            //_pipeline = pipeline;
            //UseTripleDotSuffix = true;
            //CheckFile();
            //extraction identifier
            List<string> columns = new List<string>();
            foreach (DataColumn column in _dataTable.Columns)
            {
                columns.Add(column.ColumnName.ToString());
            }


            var extractionIdentifier = "";
            BasicActivator.SelectObject("Select", columns.ToArray(), out extractionIdentifier);
            //target db
            DiscoveredDatabase db = SelectDatabase(true, "todo");
            //pipeline
            var pipe = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
            .FirstOrDefault(p => p.Name.Contains("BULK INSERT: CSV Import File (automated column-type detection)"));
            //project specific
            //todo

            var z = new ExecuteCommandCreateNewCatalogueByImportingFile(_activator, fi, extractionIdentifier, db, pipe, null);
            z.Execute();
        }


        //todo
        // this flow largely works, but obviously needs tidied up and finessed

    }

    public override Image<Rgba32> GetImage(IIconProvider iconProvider) =>
        Image.Load<Rgba32>(CatalogueIcons.FrozenCohortIdentificationConfiguration);
}