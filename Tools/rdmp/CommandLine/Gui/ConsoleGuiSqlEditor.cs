// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CsvHelper;
using Rdmp.Core.Autocomplete;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataViewing;
using ReusableLibraryCode.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terminal.Gui;
using static Terminal.Gui.TabView;
using Attribute = Terminal.Gui.Attribute;
using Rune = System.Rune;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSqlEditor : Window
    {
        protected readonly IBasicActivateItems Activator;
        private readonly IViewSQLAndResultsCollection _collection;
        private TableView tableView;
        protected TabView TabView;
        private SqlTextView textView;
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
        private Tab queryTab;
        private Tab resultTab;

        /// <summary>
        /// The default number of seconds to allow queries to run for when no value or an invalid value is specified by the user
        /// </summary>
        public const int DefaultTimeout = 300;

        public ConsoleGuiSqlEditor(IBasicActivateItems activator,IViewSQLAndResultsCollection collection)
        {
            this.Activator = activator;
            this._collection = collection;
            Modal = true;
            ColorScheme = ConsoleMainWindow.ColorScheme;

            // Tabs (query and results)
            TabView = new TabView() { Width = Dim.Fill(), Height = Dim.Fill(), Y = 1 };

            textView = new SqlTextView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Text = _orignalSql = collection.GetSql().Replace("\r\n", "\n").Replace("\t", "    ")
            };

            textView.AllowsTab = false;

            // HACK: to avoid https://github.com/migueldeicaza/gui.cs/issues/1438 .  Remove this event handler once Terminal.Gui 1.2.2 (or later) is released 
            textView.KeyPress += (e) =>
            {
                if (e.KeyEvent.Key == Key.Backspace)
                    textView.SetNeedsDisplay();
            };

            TabView.AddTab(queryTab = new Tab("Query", textView),true);

            tableView = new TableView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            tableView.Style.AlwaysShowHeaders = true;
            tableView.CellActivated += TableView_CellActivated;

            TabView.AddTab(resultTab = new Tab("Results", tableView), false);

            Add(TabView);

            // Buttons on top of control

            _btnRunOrCancel = new Button("Run"){
                X= 0,
                Y= 0,
                };

            _btnRunOrCancel.Clicked += ()=>RunOrCancel();
            Add(_btnRunOrCancel);

            var resetSql = new Button("Reset Sq_l"){
                X= Pos.Right(_btnRunOrCancel)+1};

            resetSql.Clicked += ()=>ResetSql();
            Add(resetSql);

            var clearSql = new Button("Clear S_ql"){
                X= Pos.Right(resetSql)+1,
                };

            clearSql.Clicked += ()=>ClearSql();
            Add(clearSql);

            var lblTimeout = new Label("Timeout:")
            {
                X = Pos.Right(clearSql)+1,
            };
            Add(lblTimeout);

            var tbTimeout = new TextField(_timeout.ToString())
            {
                X = Pos.Right(lblTimeout),
                Width = 5
            };
            tbTimeout.TextChanged += TbTimeout_TextChanged;
            
            Add(tbTimeout);

            var btnSave = new Button("Save"){
                X= Pos.Right(tbTimeout)+1,
                };
            btnSave.Clicked += ()=>Save();
            Add(btnSave);

            var btnClose = new Button("Clos_e"){
                X= Pos.Right(btnSave)+1,
                };


            btnClose.Clicked += ()=>{
                Application.RequestStop();
                };
                
            Add(btnClose);

            var auto = new AutoCompleteProvider(collection.GetQuerySyntaxHelper());
            collection.AdjustAutocomplete(auto);
            var bits = auto.Items.SelectMany(auto.GetBits).OrderBy(a => a).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            textView.Autocomplete.AllSuggestions = bits;
            textView.Autocomplete.MaxWidth = 40;
        }

        private void TableView_CellActivated(TableView.CellActivatedEventArgs obj)
        {
            var val = obj.Table.Rows[obj.Row][obj.Col];
            if(val != null && val != DBNull.Value)
            {
                Activator.Show(val.ToString());
            }
        }

        private void Save()
        {
            try
            {
                var tbl = tableView.Table;

                if(tbl == null)
                {
                    MessageBox.ErrorQuery("Cannot Save","No Table Loaded","Ok");
                    return;
                }

                var sfd = new SaveDialog("Save","Pick file location to save");
                Application.Run(sfd);

                if(sfd.Canceled)
                    return;

                if(sfd.FilePath != null)
                {
                    using(var writer = new StreamWriter(File.OpenWrite(sfd.FilePath.ToString())))
                        using(var w = new CsvWriter(writer,CultureInfo.CurrentCulture))
                        {
                            // write headers
                            foreach(DataColumn c in tbl.Columns)
                                w.WriteField(c.ColumnName);

                            w.NextRecord();

                            // write rows
                            foreach (DataRow r in tbl.Rows)
                            {
                                foreach (var item in r.ItemArray)
                                {
                                    w.WriteField(item);
                                }

                                w.NextRecord();
                            }
                        }
                    
                    MessageBox.Query("File Saved","Save completed","Ok");
                }
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Save Failed",ex.Message,"Ok");
            }
            
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

            TabView.SelectedTab = queryTab;
        }

        private void ResetSql()
        {
            textView.Text = _orignalSql;
            textView.SetNeedsDisplay();

            TabView.SelectedTab = queryTab;
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
                Exception ex=null;
                _runSqlTask = Task.Run(()=>
                {
                    try
                    {
                        RunSql();
                    }
                    catch (Exception e)
                    {
                        ex = e;
                    }
                }).ContinueWith((s,e)=> {
                        if(ex != null)
                        {
                            Activator.ShowException("Failed to run query", ex);
                        }
                    },TaskScheduler.FromCurrentSynchronizationContext());

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

                        // if query resulted in some data show it
                        if (dt.Columns.Count > 0)
                        {
                            TabView.SelectedTab = resultTab;
                            TabView.SetNeedsDisplay();
                        }
                            

                        OnQueryCompleted(dt);
                    }   
                }
            }
            finally
            {
                SetReadyToRun();
            }
        }

        protected virtual void OnQueryCompleted(DataTable dt)
        {
            
        }

        private class SqlAutocomplete : Terminal.Gui.Autocomplete
        {
            public override bool IsWordChar(System.Rune rune)
            {
                if ((char)rune == '_')
                    return true;

                return base.IsWordChar(rune);
            }
        }

        private class SqlTextView : TextView
        {

            private readonly HashSet<string> _keywords = new(new[]
            {
                "select", "distinct", "top", "from", "create", "CIPHER", "CLASS_ORIGIN", "CLIENT", "CLOSE", "COALESCE",
                "CODE", "COLUMNS", "COLUMN_FORMAT", "COLUMN_NAME", "COMMENT", "COMMIT", "COMPACT", "COMPLETION",
                "COMPRESSED", "COMPRESSION", "CONCURRENT", "CONNECT", "CONNECTION", "CONSISTENT", "CONSTRAINT_CATALOG",
                "CONSTRAINT_SCHEMA", "CONSTRAINT_NAME", "CONTAINS", "CONTEXT", "CONTRIBUTORS", "COPY", "CPU",
                "CURSOR_NAME", "primary", "key", "insert", "alter", "add", "update", "set", "delete", "truncate", "as",
                "order", "by", "asc", "desc", "between", "where", "and", "or", "not", "limit", "null", "is", "drop",
                "database", "table", "having", "in", "join", "on", "union", "exists",
            }, StringComparer.CurrentCultureIgnoreCase);
            private readonly Attribute _blue;
            private readonly Attribute _white;


            public SqlTextView()
            {
                Autocomplete = new SqlAutocomplete();

                // HACK: to workaround https://github.com/migueldeicaza/gui.cs/pull/1437
                var prev = Colors.Menu;
                Colors.Menu = new ColorScheme()
                {
                    Normal = Driver.MakeAttribute(Color.Black, Color.Blue),
                    Focus = Driver.MakeAttribute(Color.Black, Color.Cyan),
                };

                // this is a hack
                var cs = Autocomplete.ColorScheme;
                Debug.Assert(cs == Colors.Menu);

                // restore menu so not to break all menus in app
                Colors.Menu = prev;
                // ENDHACK

                _blue = Driver.MakeAttribute(Color.Cyan, Color.Black);
                _white = Driver.MakeAttribute(Color.White, Color.Black);
                
            }

            protected override void ColorNormal()
            {
                Driver.SetAttribute(_white);
            }

            protected override void ColorNormal(List<System.Rune> line, int idx)
            {
                Driver.SetAttribute(IsKeyword(line, idx) ? _blue : _white);
            }

            private bool IsKeyword(IEnumerable<Rune> line, int idx)
            {
                var word = IdxToWord(line, idx);

                if (string.IsNullOrWhiteSpace(word))
                {
                    return false;
                }

                return _keywords.Contains(word);
            }

            private string IdxToWord(IEnumerable<Rune> line, int idx)
            {
                var words = Regex.Split(
                    string.Join("", line),
                    "\\b");

                int count = 0;
                string current = null;

                foreach (var word in words)
                {
                    current = word;
                    count += word.Length;
                    if (count > idx)
                    {
                        break;
                    }
                }

                return current?.Trim();
            }
        }
    }
}
