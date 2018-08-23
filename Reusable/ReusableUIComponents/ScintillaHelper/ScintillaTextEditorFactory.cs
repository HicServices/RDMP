using System.Drawing;
using System.Windows.Forms;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;
using ReusableUIComponents.CommandExecution;
using ScintillaNET;

namespace ReusableUIComponents.ScintillaHelper
{
    public class ScintillaTextEditorFactory
    {
        /// <summary>
        /// Creates a new SQL (default) Scintilla editor with highlighting
        /// </summary>
        /// <param name="commandFactory">Unless your control is going to be 100% ReadOnly then you should supply an ICommandFactory e.g. RDMPCommandFactory to allow dragging and  
        /// dropping components into the window.  The ICommandFactory will decide whether the given object can be translated into an ICommand and hence into a unique bit of SQL
        /// to add to the editor</param>
        /// <param name="language"></param>
        /// <returns></returns>
        public Scintilla Create(ICommandFactory commandFactory = null, string language = "mssql", IQuerySyntaxHelper syntaxHelper = null)
        {
            var toReturn =  new Scintilla();
            toReturn.Dock = DockStyle.Fill;
            toReturn.HScrollBar = true;
            toReturn.VScrollBar = true;

            toReturn.Margins[0].Width = 40; //allows display of line numbers
            toReturn.ClearCmdKey(Keys.Control | Keys.S); //prevent Ctrl+S displaying ascii code
            toReturn.ClearCmdKey(Keys.Control | Keys.R); //prevent Ctrl+R displaying ascii code
            toReturn.ClearCmdKey(Keys.Control | Keys.W); //prevent Ctrl+W displaying ascii code
            
            if (language == "mssql")
                SetSQLHighlighting(toReturn,syntaxHelper);

            if (language == "csharp")
                SetCSharpHighlighting(toReturn);

            if(commandFactory != null)
            {
                toReturn.AllowDrop = true;
                toReturn.DragEnter += (s, e) => OnDragEnter(s, e, commandFactory);
                toReturn.DragDrop += (s, e) => OnDragDrop(s, e, commandFactory);
            }
            
            return toReturn;
        }

        private void OnDragEnter(object sender, DragEventArgs dragEventArgs, ICommandFactory commandFactory)
        {
            var command = commandFactory.Create(dragEventArgs);

            if(command != null)
                dragEventArgs.Effect = DragDropEffects.Copy;
        }
        private void OnDragDrop(object sender, DragEventArgs dragEventArgs, ICommandFactory commandFactory)
        {
            //point they are dragged over
            var editor = ((Scintilla) sender);

            if (editor.ReadOnly)
                return;

            var clientPoint = editor.PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
            //get where the mouse is hovering over
            int pos = editor.CharPositionFromPoint(clientPoint.X, clientPoint.Y);

            var command = commandFactory.Create(dragEventArgs);

            if (command == null)
                return;

            //if it has a Form give it focus
            var form = editor.FindForm();

            if(form != null)
            {
                form.Activate();
                editor.Focus();
            }

            editor.InsertText(pos,command.GetSqlString());
        }


        private void SetSQLHighlighting(Scintilla scintilla, IQuerySyntaxHelper syntaxHelper)
        {
            // Reset the styles
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Courier New";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            // Set the SQL Lexer
            scintilla.Lexer = Lexer.Sql;

            // Show line numbers
            scintilla.Margins[0].Width = 20;

            // Set the Styles
            scintilla.Styles[Style.LineNumber].ForeColor = Color.FromArgb(255, 128, 128, 128);  //Dark Gray
            scintilla.Styles[Style.LineNumber].BackColor = Color.FromArgb(255, 228, 228, 228);  //Light Gray
            scintilla.Styles[Style.Sql.Comment].ForeColor = Color.Green;
            scintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.Green;
            scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.Green;
            scintilla.Styles[Style.Sql.Number].ForeColor = Color.Maroon;
            scintilla.Styles[Style.Sql.Word].ForeColor = Color.Blue;
            scintilla.Styles[Style.Sql.Word2].ForeColor = Color.Fuchsia;
            scintilla.Styles[Style.Sql.User1].ForeColor = Color.Gray;
            scintilla.Styles[Style.Sql.User2].ForeColor = Color.FromArgb(255, 00, 128, 192);    //Medium Blue-Green
            scintilla.Styles[Style.Sql.String].ForeColor = Color.Red;
            scintilla.Styles[Style.Sql.Character].ForeColor = Color.Red;
            scintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;

            
            // Set keyword lists
            // Word = 0
            scintilla.SetKeywords(0, @"add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch check checkpoint close clustered column commit compute constraint containstable continue create current cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowcount rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go ");

            string word2 =
                @"ascii cast char charindex ceiling coalesce collate contains convert current_date current_time current_timestamp current_user floor isnull max min nullif object_id session_user substring system_user tsequal";

            if (syntaxHelper != null)
                foreach (var kvp in syntaxHelper.GetSQLFunctionsDictionary())
                word2 += " " + kvp.Key;
            
            // Word2 = 1
            scintilla.SetKeywords(1, word2);
            // User1 = 4
            scintilla.SetKeywords(4, @"all and any between cross exists in inner is join left like not null or outer pivot right some unpivot ( ) * ");
            // User2 = 5
            scintilla.SetKeywords(5, @"sys objects sysobjects ");
        }

        private CSharpLexer cSharpLexer = new CSharpLexer("class const int namespace partial public static string using void");

        private void scintilla_StyleNeeded(Scintilla scintilla, StyleNeededEventArgs e)
        {
            var startPos = scintilla.GetEndStyled();
            var endPos = e.Position;

            cSharpLexer.Style(scintilla, startPos, endPos);
        }

        private void SetCSharpHighlighting(Scintilla scintilla)
        {
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.Styles[CSharpLexer.StyleDefault].ForeColor = Color.Black;
            scintilla.Styles[CSharpLexer.StyleKeyword].ForeColor = Color.Blue;
            scintilla.Styles[CSharpLexer.StyleIdentifier].ForeColor = Color.Teal;
            scintilla.Styles[CSharpLexer.StyleNumber].ForeColor = Color.Purple;
            scintilla.Styles[CSharpLexer.StyleString].ForeColor = Color.Red;

            scintilla.Lexer = Lexer.Container;
            scintilla.StyleNeeded += (s,e)=>scintilla_StyleNeeded(scintilla,e);
        }
    }
}
