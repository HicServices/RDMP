// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.SimpleDialogs;
using ScintillaNET;
using WeCantSpell.Hunspell;

namespace Rdmp.UI.ScintillaHelper;

public enum SyntaxLanguage
{
    None,
    SQL,
    CSharp,
    XML,
    LogFile
}

/// <summary>
/// Factory for creating instances of <see cref="Scintilla"/> with a consistent look and feel and behaviour (e.g. drag and drop).
/// </summary>
public partial class ScintillaTextEditorFactory
{
    private static WordList hUnSpell;
    private static bool DictionaryExceptionShown;

    /// <summary>
    /// Creates a new SQL (default) Scintilla editor with highlighting
    /// </summary>
    /// <param name="commandFactory">Unless your control is going to be 100% ReadOnly then you should supply an <see cref="ICombineableFactory"/> to allow dragging and
    /// dropping components into the window.  The <see cref="ICombineableFactory"/> will decide whether the given object can be translated into an <see cref="ICombineToMakeCommand"/> and hence into a unique bit of SQL
    /// to add to the editor</param>
    /// <param name="language">Determines highlighting, options include mssql,csharp or null</param>
    /// <param name="syntaxHelper"></param>
    /// <param name="spellCheck"></param>
    /// <param name="lineNumbers"></param>
    /// <param name="currentDirectory"></param>
    /// <returns></returns>
    public Scintilla Create(ICombineableFactory commandFactory = null, SyntaxLanguage language = SyntaxLanguage.SQL,
        IQuerySyntaxHelper syntaxHelper = null, bool spellCheck = false, bool lineNumbers = true,
        string currentDirectory = null)
    {
        Scintilla toReturn;

        try
        {
            toReturn = new Scintilla();
        }
        catch (Win32Exception ex)
        {
            throw new Exception(
                "Could not load Scintilla.  Check that the latest Visual C++ 2015 Redistributable is installed", ex);
        }

        toReturn.Dock = DockStyle.Fill;
        toReturn.HScrollBar = true;
        toReturn.VScrollBar = true;

        if (lineNumbers)
        {
            toReturn.Margins[0].Type = MarginType.Text;
            toReturn.Margins[1].Type = MarginType.Number;
        }
        else
        {
            foreach (var margin in toReturn.Margins)
                margin.Width = 0;
        }

        toReturn.ClearCmdKey(Keys.Control | Keys.S); //prevent Ctrl+S displaying ascii code
        toReturn.ClearCmdKey(Keys.Control | Keys.R); //prevent Ctrl+R displaying ascii code
        toReturn.ClearCmdKey(Keys.Control | Keys.W); //prevent Ctrl+W displaying ascii code

        switch (language)
        {
            case SyntaxLanguage.SQL:
                SetSQLHighlighting(toReturn, syntaxHelper);
                break;
            case SyntaxLanguage.CSharp:
                SetCSharpHighlighting(toReturn);
                break;
            case SyntaxLanguage.XML:
                SetLexerEnumHighlighting(toReturn, "xml");
                break;
            case SyntaxLanguage.LogFile:
                SetLexerEnumHighlighting(toReturn, "none");
                break;
        }

        if (commandFactory != null)
        {
            toReturn.AllowDrop = true;
            toReturn.DragEnter += (s, e) => OnDragEnter(s, e, commandFactory);
            toReturn.DragDrop += (s, e) => OnDragDrop(s, e, commandFactory);
        }

        toReturn.WrapMode = (WrapMode)UserSettings.WrapMode;
        var scintillaMenu = new ScintillaMenu(toReturn, spellCheck);
        toReturn.ContextMenuStrip = scintillaMenu;

        try
        {
            if (spellCheck)
            {
                if (hUnSpell == null)
                {
                    using var dic =
                        typeof(ScintillaTextEditorFactory).Assembly.GetManifestResourceStream("Rdmp.UI.en_GB.dic");
                    using var aff =
                        typeof(ScintillaTextEditorFactory).Assembly.GetManifestResourceStream("Rdmp.UI.en_GB.aff");
                    hUnSpell = WordList.CreateFromStreams(
                        dic ?? throw new InvalidOperationException("Dictionary not found"),
                        aff ?? throw new InvalidOperationException("Dictionary not found"));
                }

                var lastCheckedSpelling = DateTime.MinValue;

                toReturn.KeyPress += (s, e) =>
                {
                    if (DateTime.Now.Subtract(lastCheckedSpelling) <= TimeSpan.FromSeconds(10)) return;
                    lastCheckedSpelling = DateTime.Now;
                    CheckSpelling((Scintilla)s, hUnSpell);
                };

                toReturn.Leave += (s, e) => CheckSpelling((Scintilla)s, hUnSpell);
                scintillaMenu.Hunspell = hUnSpell;
            }
        }
        catch (Exception e)
        {
            if (!DictionaryExceptionShown)
            {
                ExceptionViewer.Show("Could not load dictionary", e);
                DictionaryExceptionShown = true;
            }
        }

        return toReturn;
    }

    public static void CheckSpelling(Scintilla scintilla, WordList hunspell)
    {
        if (string.IsNullOrWhiteSpace(scintilla.Text))
            return;

        scintilla.Indicators[8].Style = IndicatorStyle.Squiggle;
        scintilla.Indicators[8].ForeColor = Color.Red;
        scintilla.IndicatorCurrent = 8;
        scintilla.IndicatorClearRange(0, scintilla.TextLength);

        foreach (Match m in Words().Matches(scintilla.Text))
            if (!hunspell.Check(m.Value))
                scintilla.IndicatorFillRange(m.Index, m.Length);
    }

    private static void OnDragEnter(object sender, DragEventArgs dragEventArgs, ICombineableFactory commandFactory)
    {
        var command = commandFactory.Create(dragEventArgs);

        if (command != null)
            dragEventArgs.Effect = DragDropEffects.Copy;
    }

    private static void OnDragDrop(object sender, DragEventArgs dragEventArgs, ICombineableFactory commandFactory)
    {
        //point they are dragged over
        var editor = (Scintilla)sender;

        if (editor.ReadOnly)
            return;

        var clientPoint = editor.PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
        //get where the mouse is hovering over
        var pos = editor.CharPositionFromPoint(clientPoint.X, clientPoint.Y);

        var command = commandFactory.Create(dragEventArgs);

        if (command == null)
            return;

        //if it has a Form give it focus
        var form = editor.FindForm();

        if (form != null)
        {
            form.Activate();
            editor.Focus();
        }

        editor.InsertText(pos, command.GetSqlString());
    }

    private static void SetSQLHighlighting(Scintilla scintilla, IQuerySyntaxHelper syntaxHelper)
    {
        // Reset the styles
        scintilla.StyleResetDefault();
        scintilla.Styles[Style.Default].Font = "Courier New";
        scintilla.Styles[Style.Default].Size = 10;
        scintilla.StyleClearAll();

        // Set the SQL Lexer
        scintilla.LexerName = "sql";

        // Show line numbers
        scintilla.Margins[0].Width = 20;

        // Set the Styles
        scintilla.Styles[Style.LineNumber].ForeColor = Color.FromArgb(255, 128, 128, 128); //Dark Gray
        scintilla.Styles[Style.LineNumber].BackColor = Color.FromArgb(255, 228, 228, 228); //Light Gray
        scintilla.Styles[Style.Sql.Comment].ForeColor = Color.Green;
        scintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.Green;
        scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.Green;
        scintilla.Styles[Style.Sql.Number].ForeColor = Color.Maroon;
        scintilla.Styles[Style.Sql.Word].ForeColor = Color.Blue;
        scintilla.Styles[Style.Sql.Word2].ForeColor = Color.Fuchsia;
        scintilla.Styles[Style.Sql.User1].ForeColor = Color.BlueViolet;
        scintilla.Styles[Style.Sql.User2].ForeColor = Color.FromArgb(255, 00, 128, 192); //Medium Blue-Green
        scintilla.Styles[Style.Sql.String].ForeColor = Color.Red;
        scintilla.Styles[Style.Sql.Character].ForeColor = Color.Red;
        scintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;


        // Set keyword lists
        // Word = 0
        scintilla.SetKeywords(0,
            @"add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch char check checkpoint close clustered column commit compute constraint containstable continue create current current_date cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowcount rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go ");

        var word2 =
            @"ascii cast charindex ceiling coalesce collate contains convert current_time current_timestamp current_user floor isnull max min nullif object_id session_user substring system_user tsequal";

        if (syntaxHelper != null)
            word2 += $" {string.Join(' ', syntaxHelper.GetSQLFunctionsDictionary().Keys)}";

        // Word2 = 1
        scintilla.SetKeywords(1, word2);
        // User1 = 4
        scintilla.SetKeywords(4,
            @"all and any between cross exists in inner is join left like not null or outer pivot right some unpivot");
        // User2 = 5
        scintilla.SetKeywords(5, @"sys objects sysobjects ");
    }

    private CSharpLexer cSharpLexer = new("class const int namespace partial public static string using void");

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

        scintilla.LexerName = null;
        scintilla.StyleNeeded += (s, e) => scintilla_StyleNeeded(scintilla, e);
    }

    private static void SetLexerEnumHighlighting(Scintilla scintilla, string lexer)
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

        scintilla.LexerName = lexer;
    }

    [GeneratedRegex("\\b\\w*\\b")]
    private static partial Regex Words();
}