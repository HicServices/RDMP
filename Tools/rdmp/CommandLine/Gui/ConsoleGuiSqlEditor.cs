// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Rdmp.Core.Autocomplete;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataViewing;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Terminal.Gui;
using static Terminal.Gui.TabView;
using Attribute = Terminal.Gui.Attribute;
using Rune = System.Rune;

namespace Rdmp.Core.CommandLine.Gui;

internal partial class ConsoleGuiSqlEditor : Window
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

    public ConsoleGuiSqlEditor(IBasicActivateItems activator, IViewSQLAndResultsCollection collection)
    {
        Activator = activator;
        _collection = collection;
        Modal = true;
        ColorScheme = ConsoleMainWindow.ColorScheme;

        // Tabs (query and results)
        TabView = new TabView { Width = Dim.Fill(), Height = Dim.Fill(), Y = 1 };

        textView = new SqlTextView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Text = _orignalSql = collection.GetSql().Replace("\r\n", "\n").Replace("\t", "    "),
            AllowsTab = false
        };

        TabView.AddTab(queryTab = new Tab("Query", textView), true);

        tableView = new TableView
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

        _btnRunOrCancel = new Button("Run")
        {
            X = 0,
            Y = 0
        };

        _btnRunOrCancel.Clicked += RunOrCancel;
        Add(_btnRunOrCancel);

        var resetSql = new Button("Reset Sq_l")
        {
            X = Pos.Right(_btnRunOrCancel) + 1
        };

        resetSql.Clicked += ResetSql;
        Add(resetSql);

        var clearSql = new Button("Clear S_ql")
        {
            X = Pos.Right(resetSql) + 1
        };

        clearSql.Clicked += ClearSql;
        Add(clearSql);

        var lblTimeout = new Label("Timeout:")
        {
            X = Pos.Right(clearSql) + 1
        };
        Add(lblTimeout);

        var tbTimeout = new TextField(_timeout.ToString())
        {
            X = Pos.Right(lblTimeout),
            Width = 5
        };
        tbTimeout.TextChanged += TbTimeout_TextChanged;

        Add(tbTimeout);

        var btnSave = new Button("Save")
        {
            X = Pos.Right(tbTimeout) + 1
        };
        btnSave.Clicked += Save;
        Add(btnSave);

        var btnOpen = new Button("Open")
        {
            X = Pos.Right(btnSave) + 1
        };

        btnOpen.Clicked += OpenFile;

        Add(btnOpen);

        var btnClose = new Button("Clos_e")
        {
            X = Pos.Right(btnOpen) + 1
        };


        btnClose.Clicked += () => { Application.RequestStop(); };

        Add(btnClose);

        var auto = new AutoCompleteProvider(collection.GetQuerySyntaxHelper());
        collection.AdjustAutocomplete(auto);
        var bits = auto.Items.SelectMany(AutoCompleteProvider.GetBits).OrderBy(a => a)
            .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
        textView.Autocomplete.AllSuggestions = bits;
        textView.Autocomplete.MaxWidth = 40;
    }

    private void OpenFile()
    {
        try
        {
            using var open = new OpenDialog("Open Sql File", "Open");
            Application.Run(open, ConsoleMainWindow.ExceptionPopup);

            var file = open.FilePath.ToString();
            if (!open.Canceled && File.Exists(file))
            {
                var sql = File.ReadAllText(file);
                textView.Text = sql;
            }
        }
        catch (Exception ex)
        {
            ConsoleMainWindow.ExceptionPopup(ex);
        }
    }

    private void TableView_CellActivated(TableView.CellActivatedEventArgs obj)
    {
        var val = obj.Table.Rows[obj.Row][obj.Col];
        if (val != null && val != DBNull.Value) Activator.Show(val.ToString());
    }

    private void Save()
    {
        try
        {
            var tbl = tableView.Table;

            if (tbl == null)
            {
                MessageBox.ErrorQuery("Cannot Save", "No Table Loaded", "Ok");
                return;
            }

            var sfd = new SaveDialog("Save", "Pick file location to save");
            Application.Run(sfd, ConsoleMainWindow.ExceptionPopup);

            if (sfd.Canceled)
                return;

            if (sfd.FilePath != null)
            {
                using (var writer = new StreamWriter(File.OpenWrite(sfd.FilePath.ToString())))
                using (var w = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    // write headers
                    foreach (DataColumn c in tbl.Columns)
                        w.WriteField(c.ColumnName);

                    w.NextRecord();

                    // write rows
                    foreach (DataRow r in tbl.Rows)
                    {
                        foreach (var item in r.ItemArray) w.WriteField(item);

                        w.NextRecord();
                    }
                }

                MessageBox.Query("File Saved", "Save completed", "Ok");
            }
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Save Failed", ex.Message, "Ok");
        }
    }

    private void TbTimeout_TextChanged(NStack.ustring value)
    {
        if (int.TryParse(value.ToString(), out var newTimeout))
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
        if (_runSqlTask is { IsCompleted: false })
        {
            // Cancel the sql command and let that naturally end the task
            _runSqlCmd?.Cancel();
        }
        else
        {
            Exception ex = null;
            _runSqlTask = Task.Run(() =>
            {
                try
                {
                    RunSql();
                }
                catch (Exception e)
                {
                    ex = e;
                }
            }).ContinueWith((s, e) =>
            {
                if (ex != null) Activator.ShowException("Failed to run query", ex);
            }, TaskScheduler.FromCurrentSynchronizationContext());

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
            var sql = textView.Text.ToString();

            if (string.IsNullOrWhiteSpace(sql))
            {
                tableView.Table = null;
                return;
            }

            var db = DataAccessPortal.ExpectDatabase(_collection.GetDataAccessPoint(),
                DataAccessContext.InternalDataProcessing);

            using var con = db.Server.GetConnection();
            con.Open();
            _runSqlCmd = db.Server.GetCommand(sql, con);
            _runSqlCmd.CommandTimeout = _timeout;

            using var da = db.Server.GetDataAdapter(_runSqlCmd);
            var dt = new DataTable();
            da.Fill(dt);

            Application.MainLoop.Invoke(() =>
            {
                tableView.Table = dt;

                // if query resulted in some data show it
                if (dt.Columns.Count > 0)
                {
                    TabView.SelectedTab = resultTab;
                    TabView.SetNeedsDisplay();
                }
            });


            OnQueryCompleted(dt);
        }
        finally
        {
            SetReadyToRun();
        }
    }

    protected virtual void OnQueryCompleted(DataTable dt)
    {
    }

    private class SqlAutocomplete : TextViewAutocomplete
    {
        public override bool IsWordChar(Rune rune) => (char)rune == '_' || base.IsWordChar(rune);
    }

    private partial class SqlTextView : TextView
    {
        private readonly HashSet<string> _keywords = new(
            new[]
            {
                "select", "distinct", "top", "from", "create", "CIPHER", "CLASS_ORIGIN", "CLIENT", "CLOSE",
                "COALESCE", "CODE", "COLUMNS", "COLUMN_FORMAT", "COLUMN_NAME", "COMMENT", "COMMIT", "COMPACT",
                "COMPLETION", "COMPRESSED", "COMPRESSION", "CONCURRENT", "CONNECT", "CONNECTION", "CONSISTENT",
                "CONSTRAINT_CATALOG", "CONSTRAINT_SCHEMA", "CONSTRAINT_NAME", "CONTAINS", "CONTEXT", "CONTRIBUTORS",
                "COPY", "CPU", "CURSOR_NAME", "primary", "key", "insert", "alter", "add", "update", "set", "delete",
                "truncate", "as", "order", "by", "asc", "desc", "between", "where", "and", "or", "not", "limit",
                "null", "is", "drop", "database", "table", "having", "in", "join", "on", "union", "exists"
            }, StringComparer.CurrentCultureIgnoreCase);

        private readonly Attribute _blue;
        private readonly Attribute _white;


        public SqlTextView()
        {
            Autocomplete = new SqlAutocomplete
            {
                ColorScheme = new ColorScheme
                {
                    Normal = Driver.MakeAttribute(Color.Black, Color.Blue),
                    Focus = Driver.MakeAttribute(Color.Black, Color.Cyan)
                }
            };

            _blue = Driver.MakeAttribute(Color.Cyan, Color.Black);
            _white = Driver.MakeAttribute(Color.White, Color.Black);
        }

        // The next two are renamed in 1.8.2 of Terminal.Gui.  But we could upgrade because of this issue:
        // https://github.com/HicServices/RDMP/pull/1448 . Do not upgrade until you can test the
        // Sql Editor performs correctly in the version you are updating to.  Everything works great
        // in 1.7.2 so let's stick with that until fixed fully.

        protected override void SetNormalColor()
        {
            Driver.SetAttribute(_white);
        }

        protected override void SetNormalColor(List<Rune> line, int idx)
        {
            Driver.SetAttribute(IsKeyword(line, idx) ? _blue : _white);
        }

        private bool IsKeyword(IEnumerable<Rune> line, int idx)
        {
            var word = IdxToWord(line, idx);

            return !string.IsNullOrWhiteSpace(word) && _keywords.Contains(word);
        }

        private static string IdxToWord(IEnumerable<Rune> line, int idx)
        {
            var words = WordBoundaries().Split(string.Join("", line));

            var count = 0;
            string current = null;

            foreach (var word in words)
            {
                current = word;
                count += word.Length;
                if (count > idx) break;
            }

            return current?.Trim();
        }

        [GeneratedRegex("\\b")]
        private static partial Regex WordBoundaries();
    }
}