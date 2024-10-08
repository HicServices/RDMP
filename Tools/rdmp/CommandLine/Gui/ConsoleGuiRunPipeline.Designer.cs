//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rdmp.Core.CommandLine.Gui {
    using System;
    using Terminal.Gui;
    
    
    public partial class ConsoleGuiRunPipeline : Terminal.Gui.Window {
        
        private Terminal.Gui.Label label1;
        
        private Terminal.Gui.ComboBox combobox1;
        
        private Terminal.Gui.Button btnRun;
        
        private Terminal.Gui.Button btnCancel;
        
        private Terminal.Gui.Button btnClose;
        
        private Terminal.Gui.TabView tabview1;
        
        private Terminal.Gui.ListView _results;
        
        private Terminal.Gui.TableView _tableView;
        
        private void InitializeComponent() {
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.Text = "";
            this.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
            this.Border.BorderBrush = Terminal.Gui.Color.Blue;
            this.Border.Effect3D = false;
            this.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Title = null;
            this.label1 = new Terminal.Gui.Label();
            this.label1.Width = 8;
            this.label1.Height = 1;
            this.label1.X = 0;
            this.label1.Y = 0;
            this.label1.Data = "label1";
            this.label1.Text = "Pipeline";
            this.label1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.label1);
            this.combobox1 = new Terminal.Gui.ComboBox();
            this.combobox1.Width = Dim.Fill(1);
            this.combobox1.Height = 5;
            this.combobox1.X = 9;
            this.combobox1.Y = 0;
            this.combobox1.Data = "combobox1";
            this.combobox1.Text = "";
            this.combobox1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Add(this.combobox1);
            this.btnRun = new Terminal.Gui.Button();
            this.btnRun.Width = 7;
            this.btnRun.Height = 1;
            this.btnRun.X = 0;
            this.btnRun.Y = 2;
            this.btnRun.Data = "btnRun";
            this.btnRun.Text = "Run";
            this.btnRun.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.btnRun.IsDefault = false;
            this.Add(this.btnRun);
            this.btnCancel = new Terminal.Gui.Button();
            this.btnCancel.Width = 10;
            this.btnCancel.Height = 1;
            this.btnCancel.X = 8;
            this.btnCancel.Y = 2;
            this.btnCancel.Data = "btnCancel";
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.btnCancel.IsDefault = false;
            this.Add(this.btnCancel);
            this.btnClose = new Terminal.Gui.Button();
            this.btnClose.Width = 10;
            this.btnClose.Height = 1;
            this.btnClose.X = 19;
            this.btnClose.Y = 2;
            this.btnClose.Data = "btnClose";
            this.btnClose.Text = "C_lose";
            this.btnClose.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.btnClose.IsDefault = false;
            this.Add(this.btnClose);
            this.tabview1 = new Terminal.Gui.TabView();
            this.tabview1.Width = Dim.Fill(0);
            this.tabview1.Height = Dim.Fill(0);
            this.tabview1.X = 0;
            this.tabview1.Y = 3;
            this.tabview1.Data = "tabview1";
            this.tabview1.Text = "";
            this.tabview1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tabview1.MaxTabTextWidth = 30u;
            this.tabview1.Style.ShowBorder = true;
            this.tabview1.Style.ShowTopLine = true;
            this.tabview1.Style.TabsOnBottom = false;
            Terminal.Gui.TabView.Tab tabview1Messages;
            tabview1Messages = new Terminal.Gui.TabView.Tab("Messages", new View());
            tabview1Messages.View.Width = Dim.Fill();
            tabview1Messages.View.Height = Dim.Fill();
            this._results = new Terminal.Gui.ListView();
            this._results.Width = Dim.Fill(0);
            this._results.Height = Dim.Fill(0);
            this._results.X = 0;
            this._results.Y = 0;
            this._results.Data = "_results";
            this._results.Text = "";
            this._results.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this._results.Source = new Terminal.Gui.ListWrapper(new string[] {
                        "Item1",
                        "Item2",
                        "Item3"});
            tabview1Messages.View.Add(this._results);
            tabview1.AddTab(tabview1Messages, false);
            Terminal.Gui.TabView.Tab tabview1Counts;
            tabview1Counts = new Terminal.Gui.TabView.Tab("Counts", new View());
            tabview1Counts.View.Width = Dim.Fill();
            tabview1Counts.View.Height = Dim.Fill();
            this._tableView = new Terminal.Gui.TableView();
            this._tableView.Width = Dim.Fill(0);
            this._tableView.Height = Dim.Fill(0);
            this._tableView.X = 0;
            this._tableView.Y = 0;
            this._tableView.Data = "_tableView";
            this._tableView.Text = "";
            this._tableView.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this._tableView.Style.AlwaysShowHeaders = false;
            this._tableView.Style.ExpandLastColumn = true;
            this._tableView.Style.InvertSelectedCellFirstCharacter = false;
            this._tableView.Style.ShowHorizontalHeaderOverline = true;
            this._tableView.Style.ShowHorizontalHeaderUnderline = true;
            this._tableView.Style.ShowVerticalCellLines = true;
            this._tableView.Style.ShowVerticalHeaderLines = true;
            System.Data.DataTable _tableViewTable;
            _tableViewTable = new System.Data.DataTable();
            System.Data.DataColumn _tableViewTableCol0;
            _tableViewTableCol0 = new System.Data.DataColumn();
            _tableViewTableCol0.ColumnName = "Name";
            _tableViewTable.Columns.Add(_tableViewTableCol0);
            System.Data.DataColumn _tableViewTableCol1;
            _tableViewTableCol1 = new System.Data.DataColumn();
            _tableViewTableCol1.ColumnName = "Progress";
            _tableViewTable.Columns.Add(_tableViewTableCol1);
            this._tableView.Table = _tableViewTable;
            tabview1Counts.View.Add(this._tableView);
            tabview1.AddTab(tabview1Counts, false);
            this.tabview1.ApplyStyleChanges();
            this.Add(this.tabview1);
        }
    }
}
