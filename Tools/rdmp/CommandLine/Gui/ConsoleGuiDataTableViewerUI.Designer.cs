
//------------------------------------------------------------------------------

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.18.0
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace Rdmp.Core.CommandLine.Gui {
    using System;
    using Terminal.Gui;
    
    
    public partial class ConsoleGuiDataTableViewerUI : Terminal.Gui.Window {
        
        private Terminal.Gui.TableView tableview1;
        
        private Terminal.Gui.Button button1;
        
        private void InitializeComponent() {
            this.button1 = new Terminal.Gui.Button();
            this.tableview1 = new Terminal.Gui.TableView();
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.Modal = false;
            this.Text = "";
            this.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
            this.Border.Effect3D = false;
            this.Border.DrawMarginFrame = true;
            this.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.Title = "";
            this.tableview1.Width = Dim.Fill(0);
            this.tableview1.Height = Dim.Fill(1);
            this.tableview1.X = 0;
            this.tableview1.Y = 0;
            this.tableview1.Data = "tableview1";
            this.tableview1.Text = "";
            this.tableview1.TextAlignment = Terminal.Gui.TextAlignment.Left;
            this.tableview1.FullRowSelect = false;
            this.tableview1.Style.AlwaysShowHeaders = false;
            this.tableview1.Style.ExpandLastColumn = true;
            this.tableview1.Style.InvertSelectedCellFirstCharacter = false;
            this.tableview1.Style.ShowHorizontalHeaderOverline = true;
            this.tableview1.Style.ShowHorizontalHeaderUnderline = true;
            this.tableview1.Style.ShowVerticalCellLines = true;
            this.tableview1.Style.ShowVerticalHeaderLines = true;
            System.Data.DataTable tableview1Table;
            tableview1Table = new System.Data.DataTable();
            System.Data.DataColumn tableview1TableColumn0;
            tableview1TableColumn0 = new System.Data.DataColumn();
            tableview1TableColumn0.ColumnName = "Column 0";
            tableview1Table.Columns.Add(tableview1TableColumn0);
            this.tableview1.Table = tableview1Table;
            this.Add(this.tableview1);
            this.button1.Width = 8;
            this.button1.X = 0;
            this.button1.Y = Pos.Bottom(tableview1);
            this.button1.Data = "button1";
            this.button1.Text = "Close";
            this.button1.TextAlignment = Terminal.Gui.TextAlignment.Centered;
            this.button1.IsDefault = false;
            this.Add(this.button1);
        }
    }
}
