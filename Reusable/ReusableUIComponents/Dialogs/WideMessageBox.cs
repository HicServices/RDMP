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
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace ReusableUIComponents.Dialogs
{
    /// <summary>
    /// Used to display a message to the user including selectable text and resizing.  Basically improves on System.Windows.Forms.MessageBox
    /// </summary>
    [TechnicalUI]
    public partial class WideMessageBox : Form
    {
        /// <summary>
        /// The currently displayed message
        /// </summary>
        public WideMessageBoxArgs Args { get; set; }

        readonly Stack<WideMessageBoxArgs> _navigationStack = new Stack<WideMessageBoxArgs>();

        private static readonly HashSet<string> KeywordBlacklist = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase)
        {
            "date",
            "example",
            "column",
            "error"
        };

        #region Static setup of dictionary of keywords
        public static CommentStore CommentStore;
        #endregion
        
        Regex className = new Regex(@"^\w+$");

        public WideMessageBox(WideMessageBoxArgs args)
        {
            InitializeComponent();
            
            Setup(args);
            
            //can only write to clipboard in STA threads
            btnCopyToClipboard.Visible = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

            //try to resize form to fit bounds
            this.Size = GetPreferredSizeOfTextControl(richTextBox1);
            this.Size = new Size(this.Size.Width + 10, this.Size.Height + 150);//leave a bit of padding

            var theScreen = Screen.FromControl(this);
            
            //enforce sensible max/min sizes
            Width = Math.Min(Math.Max(600, Width),theScreen.Bounds.Width - 400);

            //if the text is too long vertically just maximise the message box
            if (this.Height > theScreen.Bounds.Height)
                this.WindowState = FormWindowState.Maximized;

            richTextBox1.LinkClicked += richTextBox1_LinkClicked;
            btnViewSourceCode.Click += (s, e) => new ViewSourceCodeDialog((string)btnViewSourceCode.Tag).Show();
        }


        private void Setup(WideMessageBoxArgs args)
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

            //todo hack:if theres a long title and no message
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
                lblMainMessage.Text = title;
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
                ViewSourceCodeDialog.SourceCodeIsAvailableFor(title + ".cs"))
            {
                btnViewSourceCode.Enabled = true;
                btnViewSourceCode.Tag = title + ".cs";
            }
            else
                btnViewSourceCode.Enabled = false;
        }

        public static void Show(IHasSummary summary,bool isModalDialog = true)
        {
            summary.GetSummary(out string title,out string body, out string stackTrace,out CheckResult level);
            Show(title,body,stackTrace,isModalDialog,null,GetTheme(level));
        }
        public static void Show(string title, DataGridViewRow row, bool isModalDialog = true, WideMessageBoxTheme theme = WideMessageBoxTheme.Help)
        {
            Show(title, GetText(row), null,isModalDialog,null, theme);
        }

        private static string GetText(DataGridViewRow row)
        {
            const int MAX_LENGTH = 5000;

            StringBuilder sb = new StringBuilder();

            foreach (DataGridViewColumn c in row.DataGridView.Columns)
                if (c.Visible)
                {
                    var v = row.Cells[c.Name].Value;
                    var stringval = v == null || v == DBNull.Value ? "NULL" : v.ToString();

                    if(stringval.Length > MAX_LENGTH)
                        stringval = stringval.Substring(0, MAX_LENGTH) + "...";

                    sb.AppendLine(c.Name + ":" + stringval);
                }
                    

            return sb.ToString();
        }
        public static void Show(string title, string message, string environmentDotStackTrace = null, bool isModalDialog = true, string keywordNotToAdd = null,WideMessageBoxTheme theme = WideMessageBoxTheme.Exception)
        {
            WideMessageBox wmb = new WideMessageBox(new WideMessageBoxArgs(title,message, environmentDotStackTrace, keywordNotToAdd, theme));

            if (isModalDialog)
                wmb.ShowDialog();
            else
                wmb.Show();
            
        }

        public static void Show(string title, string message, WideMessageBoxTheme theme)
        {
            Show(title, message,null,theme:theme);
        }
        private void ApplyTheme(WideMessageBoxTheme theme)
        {
            
            switch (theme)
            {
                case WideMessageBoxTheme.Exception:
                    pbIcon.Image = Images.ErrorIcon;
                    break;
                case WideMessageBoxTheme.Warning:
                    pbIcon.Image = Images.WarningIcon;
                    break;
                case WideMessageBoxTheme.Help:
                    pbIcon.Image =Images.InformationIcon;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("theme");
            }

            var f = new IconFactory();

            Icon = f.GetIcon((Bitmap)pbIcon.Image);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            //gets around formatting of hyperlinks appearing in Ctrl+C
            Clipboard.SetText(Args.Title + Environment.NewLine + Environment.NewLine + Args.Message);
        }

        private void WideMessageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape || (e.KeyCode == Keys.W && e.Control))
                this.Close();

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

        void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if(e.LinkText.Contains("#"))
            {
                var split = e.LinkText.Split('#');
                if(split.Length >=2 && CommentStore.ContainsKey(split[1]))
                    NavigateTo(split[1]);
            }
        }

        private void NavigateTo(string keyword)
        {
            _navigationStack.Push(Args);
            
            Setup(new WideMessageBoxArgs(keyword,CommentStore[keyword],null,keyword,WideMessageBoxTheme.Help){FormatAsParagraphs = true});
        }

        private void SetMessage(string message, string keywordNotToAdd = null)
        {
            if(string.IsNullOrWhiteSpace(message))
                message = "";            

            //unless the text is unreasonably long or we don't have help documentation available
            if (message.Length > 100000 || CommentStore == null)
            {
                richTextBox1.Text = message;
                return;
            }

            bool lastWordWasALink = false;
            richTextBox1.Visible = false;

            foreach (string word in Regex.Split(message, @"(?<=[. ,;)(<>-])"))
            {
                if(string.IsNullOrWhiteSpace(word))
                    continue;

                //Try to match the trimmed keyword or the trimmed keyword without an s
                var keyword = GetDocumentationKeyword(keywordNotToAdd, word.Trim('.', ' ', ',', ';', '(', ')','<','>','-'));

                if (keyword != null)
                {
                    if (lastWordWasALink)
                        richTextBox1.SelectedText = " ";
                    
                    richTextBox1.InsertLink(word, keyword);

                    lastWordWasALink = true;

                }
                else
                {
                    richTextBox1.SelectedText = word;
                    lastWordWasALink = false;
                }
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
        private string GetDocumentationKeyword(string keywordNotToAdd, string word)
        {
            //do not highlight common words like "example"
            if (KeywordBlacklist.Contains(word) || KeywordBlacklist.Contains(word.TrimEnd('s')))
                return null;

            var keyword = CommentStore.GetDocumentationKeywordIfExists(word, true);
            
            if (keyword == keywordNotToAdd)
                return null;

            return keyword;
        }

        private static void ShowHelpSection(HelpSection hs)
        {
            new WideMessageBox(new WideMessageBoxArgs(hs.Keyword, hs.HelpText, Environment.StackTrace, hs.Keyword, WideMessageBoxTheme.Help)
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
            switch (result)
            {
                case CheckResult.Success:
                    return WideMessageBoxTheme.Help;
                case CheckResult.Warning:
                    return WideMessageBoxTheme.Warning;
                case CheckResult.Fail:
                    return WideMessageBoxTheme.Exception;
                default:
                    throw new ArgumentOutOfRangeException("result");
            }
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
            if(!_navigationStack.Any())
                return;

            Setup(_navigationStack.Pop());
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (
       richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) == 0 && e.KeyData == Keys.Up ||
       richTextBox1.GetLineFromCharIndex(richTextBox1.SelectionStart) == richTextBox1.GetLineFromCharIndex(richTextBox1.TextLength) && e.KeyData == Keys.Down ||
       richTextBox1.SelectionStart == richTextBox1.TextLength && e.KeyData == Keys.Right ||
       richTextBox1.SelectionStart == 0 && e.KeyData == Keys.Left
   ) e.Handled = true;

            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Escape || (e.KeyData == Keys.W && e.Control))
                e.Handled = true;
        }

        private Size GetPreferredSizeOfTextControl(Control c)
        {
            Graphics graphics = c.CreateGraphics();
            SizeF measureString = graphics.MeasureString(c.Text, c.Font);

            int minimumWidth = 400;
            int minimumHeight = 150;

            Rectangle maxSize = Screen.GetBounds(c);
            return new Size(
                (int)Math.Min(maxSize.Width, Math.Max(measureString.Width + 50,minimumWidth)),
                (int)Math.Min(maxSize.Height,Math.Max(measureString.Height + 100,minimumHeight)));
        }
    }
}
