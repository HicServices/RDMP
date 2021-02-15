// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataViewing;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSqlEditor : Window
    {
        private readonly IBasicActivateItems _activator;
        private readonly IViewSQLAndResultsCollection _collection;
        private TableView tableView;
        private TextView textView;
        private Button _btnRunOrCancel;
        private Task _runSqlTask;
        private DbCommand _runSqlCmd;

        /// <summary>
        /// The original SQL this control was launched with
        /// </summary>
        private string _orignalSql;

        /// <summary>
        /// The number of seconds to allow queries to run for, can be changed by user
        /// </summary>
        private int _timeout = DefaultTimeout;

        /// <summary>
        /// The default number of seconds to allow queries to run for when no value or an invalid value is specified by the user
        /// </summary>
        public const int DefaultTimeout = 300;

        public ConsoleGuiSqlEditor(IBasicActivateItems activator,IViewSQLAndResultsCollection collection)
        {
            this._activator = activator;
            this._collection = collection;
            Modal = true;

            textView = new TextView()
            {
                X= 0,
                Y=0,
                Width = Dim.Fill(),
                Height = Dim.Percent(30),
                Text = _orignalSql = collection.GetSql().Replace("\r\n","\n")
            };

            Add(textView);

            _btnRunOrCancel = new Button("Run"){
                X= 0,
                Y=Pos.Bottom(textView),
                };

            _btnRunOrCancel.Clicked += ()=>RunOrCancel();
            Add(_btnRunOrCancel);

            var resetSql = new Button("Reset Sql"){
                X= Pos.Right(_btnRunOrCancel)+1,
                Y= Pos.Bottom(textView),
                };

            resetSql.Clicked += ()=>ResetSql();
            Add(resetSql);

            var clearSql = new Button("Clear Sql"){
                X= Pos.Right(resetSql)+1,
                Y= Pos.Bottom(textView),
                };

            clearSql.Clicked += ()=>ClearSql();
            Add(clearSql);

            var lblTimeout = new Label("Timeout:")
            {
                X = Pos.Right(clearSql)+1,
                Y = Pos.Bottom(textView)
            };
            Add(lblTimeout);

            var tbTimeout = new TextField(_timeout.ToString())
            {
                X = Pos.Right(lblTimeout),
                Y = Pos.Bottom(textView),
                Width = 5
            };
            tbTimeout.TextChanged += TbTimeout_TextChanged;
            
            Add(tbTimeout);

            var btnClose = new Button("Close"){
                X= Pos.Right(tbTimeout)+1,
                Y= Pos.Bottom(textView),
                };
            btnClose.Clicked += ()=>Application.RequestStop();
            Add(btnClose);

            
            tableView = new TableView(){
            X = 0,
            Y = Pos.Bottom(btnClose),
            Width = Dim.Fill(),
            Height = Dim.Fill()
                };

            Add(tableView);
        }

        private void TbTimeout_TextChanged(NStack.ustring value)
        {
            if(int.TryParse(value.ToString(),out int newTimeout))
                _timeout = newTimeout < 0 ? DefaultTimeout : newTimeout;
            else
                _timeout = DefaultTimeout;
        }

        private void ClearSql()
        {
            textView.Text = "";
            textView.SetNeedsDisplay();
        }

        private void ResetSql()
        {
            textView.Text = _orignalSql;
            textView.SetNeedsDisplay();
        }

        private void RunOrCancel()
        {
            // if task is still running we should cancel
            if(_runSqlTask  != null && !_runSqlTask.IsCompleted)
            {
                // Cancel the sql command and let that naturally end the task
                _runSqlCmd?.Cancel();
            }
            else
            {
                _runSqlTask = Task.Run(()=>RunSql());
                _btnRunOrCancel.Text = "Cancel";
                _btnRunOrCancel.SetNeedsDisplay();
            }
        }
        
        private void SetReadyToRun()
        {
            _btnRunOrCancel.Text = "Run";
            _btnRunOrCancel.SetNeedsDisplay();
        }
        private void RunSql()
        {
            try
            {
                string sql = textView.Text.ToString();

                if(string.IsNullOrWhiteSpace(sql))
                {
                    tableView.Table = null;
                    return;
                }

                var db = DataAccessPortal.GetInstance().ExpectDatabase(_collection.GetDataAccessPoint(),DataAccessContext.InternalDataProcessing);

                using(var con = db.Server.GetConnection())
                {
                    con.Open();
                    _runSqlCmd = db.Server.GetCommand(sql,con);
                    _runSqlCmd.CommandTimeout = _timeout;

                    using(var da = db.Server.GetDataAdapter(_runSqlCmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        tableView.Table = dt;
                    }   
                }
            }
            catch (Exception ex)
            {
                _activator.ShowException("Failed to run query",ex);
            }
            finally
            {
                SetReadyToRun();
            }
        }
    }
}
