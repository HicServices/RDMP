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
using System.Text;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSqlEditor : Window
    {
        private readonly IBasicActivateItems _activator;
        private readonly IViewSQLAndResultsCollection _collection;
        private TableView tableView;
        private TextView textView;

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
                Text = collection.GetSql().Replace("\r\n","\n")
            };

            Add(textView);

            var btnRun = new Button("Run"){
                X= 0,
                Y=Pos.Bottom(textView),
                };

            btnRun.Clicked += ()=>RunSql();
            Add(btnRun);

            var btnClose = new Button("Close"){
                X= Pos.Right(btnRun),
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
                    using(var da = db.Server.GetDataAdapter(sql,con))
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
        }
    }
}
