// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.UI.SimpleControls;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Rdmp.UI.SimpleDialogs;

/// <summary>
/// Used to display a message to the user including selectable text and resizing.  Basically improves on System.Windows.Forms.MessageBox
/// </summary>
[TechnicalUI]
public partial class WideMessageBox : Form
{
    /// <summary>
    /// The maximum number of characters displayed in the title
    /// </summary>
    public const int MAX_LENGTH_TITLE = 10000;

    /// <summary>
    /// The maximum number of characters displayed in the body
    /// </summary>
    public const int MAX_LENGTH_BODY = 20000;

    /// <summary>
    /// The currently displayed message
    /// </summary>
    public WideMessageBoxArgs Args { get; set; }

    private readonly Stack<WideMessageBoxArgs> _navigationStack = new();

    private static readonly HashSet<string> KeywordIgnoreList = new(StringComparer.CurrentCultureIgnoreCase)
    {
        "date",
        "example",
        "column",
        "error"
    };

    #region Static setup of dictionary of keywords

    public static CommentStore CommentStore;

    #endregion

    private Regex className = new(@"^\w+$");

    public WideMessageBox(WideMessageBoxArgs args)
    {
        InitializeComponent();

        Setup(args);

        //can only write to clipboard in STA threads
        btnCopyToClipboard.Visible = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
        btnViewStackTrace.Visible = args.Theme == WideMessageBoxTheme.Exception;

        //try to resize form to fit bounds
        Size = GetPreferredSizeOfTextControl(richTextBox1);
        Size = new Size(Size.Width + 10, Size.Height + 150); //leave a bit of padding

        richTextBox1.LinkClicked += richTextBox1_LinkClicked;
        btnViewSourceCode.Click += (s, e) => new ViewSourceCodeDialog((string)btnViewSourceCode.Tag).Show();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        var theScreen = Screen.FromControl(this);

        //enforce sensible max/min sizes
        Width = Math.Min(Math.Max(600, Width), theScreen.Bounds.Width - 400);

        //if the text is too long vertically just maximise the message box
        if (Height > theScreen.Bounds.Height)
        {
            MaximizedBounds = theScreen.WorkingArea;
            WindowState = FormWindowState.Maximized;
        }
    }

    protected void Setup(WideMessageBoxArgs args)
    {
        Args = args;

        btnBack.Enabled = _navigationStack.Any();

        btnViewStackTrace.Visible = args.EnvironmentDotStackTrace != null;

        richTextBox1.Font = new Font(FontFamily.GenericMonospace, richTextBox1.Font.Size);
        richTextBox1.Select(0, 0);
        richTextBox1.WordWrap = true;
        richTextBox1.Text = "";

        var message = args.Message;
        var title = args.Title;

        //todo hack:if there's a long title and no message
        if (!string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(message) && title.Length > 100)
        {
            message = title;
            title = null;
        }

        //Replace single newlines with double new lines
        if (Args.FormatAsParagraphs && CommentStore != null)
            message = CommentStore.FormatAsParagraphs(message);

        //if there is a title
        if (!string.IsNullOrWhiteSpace(title))
        {
            lblMainMessage.Text = title.Length > MAX_LENGTH_TITLE ? title[..MAX_LENGTH_TITLE] : title;
        }
        else
        {
            richTextBox1.Top = lblMainMessage.Top;
            richTextBox1.Height += lblMainMessage.Top;
            lblMainMessage.Visible = false;
        }

        SetMessage(message, args.KeywordNotToAdd);

        ApplyTheme(args.Theme);

        if (args.Theme == WideMessageBoxTheme.Help)
            SetViewSourceCodeButton(args.Title);
    }

    private void SetViewSourceCodeButton(string title)
    {
        //if it's a class name we are showing
        if (className.IsMatch(title) &&
            ViewSourceCodeDialog.SourceCodeIsAvailableFor($"{title}.cs"))
        {
            btnViewSourceCode.Enabled = true;
            btnViewSourceCode.Visible = true;
            btnViewSourceCode.Tag = $"{title}.cs";
        }
        else
        {
            btnViewSourceCode.Enabled = false;
            btnViewSourceCode.Visible = false;
        }
    }

    public static void Show(IHasSummary summary, bool isModalDialog = true)
    {
        summary.GetSummary(out var title, out var body, out var stackTrace, out var level);
        Show(title, body, stackTrace, isModalDialog, null, GetTheme(level));
    }

    public static void Show(string title, DataGridViewRow row, bool isModalDialog = true,
        WideMessageBoxTheme theme = WideMessageBoxTheme.Help)
    {
        Show(title, GetText(row), null, isModalDialog, null, theme);
    }

    private static string GetText(DataGridViewRow row)
    {
        const int MAX_LENGTH_ELEMENT = 10000;
        var sb = new StringBuilder();

        foreach (DataGridViewColumn c in row.DataGridView.Columns)
            if (c.Visible)
            {
                var v = row.Cells[c.Name].Value;
                var stringval = v == null || v == DBNull.Value ? "NULL" : v.ToString();

                if (stringval.Length > MAX_LENGTH_ELEMENT)
                    stringval = $"{stringval[..MAX_LENGTH_ELEMENT]}...";

                sb.AppendLine($"{c.Name}:{stringval}");
            }

        return sb.Length >= MAX_LENGTH_BODY ? sb.ToString(0, MAX_LENGTH_BODY) : sb.ToString();
    }

    public static void Show(string title, string message, string environmentDotStackTrace = null,
        bool isModalDialog = true, string keywordNotToAdd = null,
        WideMessageBoxTheme theme = WideMessageBoxTheme.Exception)
    {
        var wmb = new WideMessageBox(new WideMessageBoxArgs(title, message, environmentDotStackTrace, keywordNotToAdd,
            theme));
        wmb.TopMost = true;
        if (isModalDialog)
            wmb.ShowDialog();
        else
            wmb.Show();
        wmb.Focus();
        wmb.BringToFront();
    }

    public static void Show(string title, string message, WideMessageBoxTheme theme)
    {
        Show(title, message, null, theme: theme);
    }

    private void ApplyTheme(WideMessageBoxTheme theme)
    {
        pbIcon.Image = theme switch
        {
            WideMessageBoxTheme.Exception => (Image)Images.ErrorIcon.ImageToBitmap(),
            WideMessageBoxTheme.Warning => (Image)Images.WarningIcon.ImageToBitmap(),
            WideMessageBoxTheme.Help => (Image)Images.InformationIcon.ImageToBitmap(),
            _ => throw new ArgumentOutOfRangeException(nameof(theme))
        };

        Icon = IconFactory.Instance.GetIcon(pbIcon.Image.LegacyToImage());
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnCopyToClipboard_Click(object sender, EventArgs e)
    {
        //gets around formatting of hyperlinks appearing in Ctrl+C
        Clipboard.SetText(Args.Title + Environment.NewLine + Environment.NewLine + Args.Message);
    }

    private void WideMessageBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.W && e.Control))
            Close();

        if (e.KeyCode == Keys.Back)
            Back();
    }

    private void btnViewStackTrace_Click(object sender, EventArgs e)
    {
        OnViewStackTrace();
    }

    protected virtual void OnViewStackTrace()
    {
        var dialog = new ExceptionViewerStackTraceWithHyperlinks(Args.EnvironmentDotStackTrace);
        dialog.Show();
    }

    private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
    {
        if (e.LinkText.Contains('#'))
        {
            var split = e.LinkText.Split('#');
            if (split.Length >= 2 && CommentStore.ContainsKey(split[1]))
                NavigateTo(split[1]);
        }
        else
        {
            var text = e.LinkText.TrimEnd('.', ')');

            if (CommentStore.ContainsKey(text))
                NavigateTo(text);
        }
    }

    private void NavigateTo(string keyword)
    {
        _navigationStack.Push(Args);

        Setup(new WideMessageBoxArgs(keyword, CommentStore[keyword], null, keyword, WideMessageBoxTheme.Help)
        { FormatAsParagraphs = true });
    }

    private void SetMessage(string message, string keywordNotToAdd = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            message = "";

        if (message.Length > MAX_LENGTH_BODY)
            message = message[..MAX_LENGTH_BODY];

        //if we don't have help documentation available just set the message without looking for hyperlinks
        if (CommentStore == null)
        {
            richTextBox1.Text = message;
            return;
        }

        richTextBox1.Visible = false;

        foreach (var word in Regex.Split(message, @"(?<=[. ,;)(<>\n-])"))
        {
            //Try to match the trimmed keyword or the trimmed keyword without an s
            var keyword = GetDocumentationKeyword(keywordNotToAdd,
                word.Trim('.', ' ', ',', ';', '(', ')', '<', '>', '-', '\r', '\n'));

            if (keyword != null)
                richTextBox1.InsertLink(word, keyword);
            else
            //avoids bong sound
            if (word != "")
                richTextBox1.SelectedText = word; //this appends the text to the text box (confusing I know)
        }

        //scroll back to the top
        richTextBox1.Visible = true;
        richTextBox1.Select(0, 0);
    }

    /// <summary>
    /// Returns <paramref name="word"/> if <see cref="CommentStore"/> contains an entry for it.
    /// </summary>
    /// <param name="keywordNotToAdd"></param>
    /// <param name="word"></param>
    /// <returns></returns>
    private static string GetDocumentationKeyword(string keywordNotToAdd, string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return null;

        //do not highlight common words like "example"
        if (KeywordIgnoreList.Contains(word) || KeywordIgnoreList.Contains(word.TrimEnd('s')))
            return null;

        var keyword = CommentStore.GetDocumentationKeywordIfExists(word.Trim(), true);

        return keyword == keywordNotToAdd ? null : keyword;
    }

    private static void ShowHelpSection(HelpSection hs)
    {
        new WideMessageBox(new WideMessageBoxArgs(hs.Keyword, hs.HelpText, Environment.StackTrace, hs.Keyword,
            WideMessageBoxTheme.Help)
        {
            FormatAsParagraphs = true
        }).Show();
    }

    public static void ShowKeywordHelp(string key, string docs)
    {
        ShowHelpSection(new HelpSection(key, docs));
    }

    public static WideMessageBoxTheme GetTheme(CheckResult result)
    {
        return result switch
        {
            CheckResult.Success => WideMessageBoxTheme.Help,
            CheckResult.Warning => WideMessageBoxTheme.Warning,
            CheckResult.Fail => WideMessageBoxTheme.Exception,
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }

    private void WideMessageBox_Load(object sender, EventArgs e)
    {
        Back();
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        Back();
    }

    private void Back()
    {
        if (!_navigationStack.Any())
            return;

        Setup(_navigationStack.Pop());
    }

    private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
    {
        if (
            (richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) == 0 && e.KeyData == Keys.Up) ||
            (richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) ==
                richTextBox1.GetLineFromCharIndex(richTextBox1.TextLength) && e.KeyData == Keys.Down) ||
            (richTextBox1.SelectionStart == richTextBox1.TextLength && e.KeyData == Keys.Right) ||
            (richTextBox1.SelectionStart == 0 && e.KeyData == Keys.Left)
        ) e.Handled = true;

        if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Escape || (e.KeyData == Keys.W && e.Control))
            e.Handled = true;

        if (e.KeyCode == Keys.C && e.Control)
        {
            e.Handled = true;

            //Ctrl+C with nothing selected copies it all
            if (richTextBox1.SelectedText.Length == 0)
            {
                //gets around formatting of hyperlinks appearing in Ctrl+C
                Clipboard.SetText(Args.Title + Environment.NewLine + Environment.NewLine + Args.Message);
            }
            else
            {
                //the text (which may include 'hyperlinks') e.g. "Bob Project #Project(ExtractionConfiguration #IExtractionConfigurationID=3"
                var text = richTextBox1.SelectedText;

                //
                /*
                 From the rtf text, for example:

                 {\rtf1\ansi\ansicpg1252\deff0\deflang2057{\fonttbl{\f0\fnil\fcharset0 Courier New;}}
\uc1\pard\f0\fs17 Bob Project \v #Project\v0 (ExtractionConfiguration \v #IExtractionConfiguration\v0 ID=3}

                Grab the hyperlinks
*/
                //gets around formatting of hyperlinks appearing in Ctrl+C
                var rtfHyperlinks = new Regex(@"\\v #([^\\]*)\\v");

                foreach (Match m in rtfHyperlinks.Matches(richTextBox1.SelectedRtf))
                    //replace the hyperlink text in the 'unformatted' text
                    text = text.Replace($"#{m.Groups[1].Value}", "");

                Clipboard.SetText(text);
            }
        }
    }

    private static Size GetPreferredSizeOfTextControl(Control c)
    {
        var graphics = c.CreateGraphics();
        var measureString = graphics.MeasureString(c.Text, c.Font);

        var minimumWidth = 400;
        var minimumHeight = 150;

        var maxSize = Screen.GetBounds(c);
        maxSize.Height = Math.Min(maxSize.Height, 800);
        maxSize.Width = Math.Min(maxSize.Width, 1024);

        return new Size(
            (int)Math.Min(maxSize.Width, Math.Max(measureString.Width + 50, minimumWidth)),
            (int)Math.Min(maxSize.Height, Math.Max(measureString.Height + 100, minimumHeight)));
    }
}