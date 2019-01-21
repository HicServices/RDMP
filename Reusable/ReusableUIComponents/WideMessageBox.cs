using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;

namespace ReusableUIComponents
{
    /// <summary>
    /// Used to display a message to the user including selectable text and resizing.  Basically improves on System.Windows.Forms.MessageBox
    /// </summary>
    [TechnicalUI]
    public partial class WideMessageBox : Form
    {
        private readonly string _environmentDotStackTrace;


        #region Static setup of dictionary of keywords
        public static CommentStore CommentStore;
        #endregion

        public WideMessageBox(string title, string message, string environmentDotStackTrace = null, string keywordNotToAdd = null, WideMessageBoxTheme theme = WideMessageBoxTheme.Exception)
        {
            _environmentDotStackTrace = environmentDotStackTrace;
            InitializeComponent();
            
            richTextBox1.Font = new Font(FontFamily.GenericMonospace, richTextBox1.Font.Size);
            richTextBox1.Select(0, 0);
            richTextBox1.WordWrap = true;
            Setup(message,keywordNotToAdd);
            
            //can only write to clipboard in STA threads
            btnCopyToClipboard.Visible = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

            btnViewStackTrace.Visible = _environmentDotStackTrace != null;
            
            if (title != null)
                lblMainMessage.Text = title;
            else
            {
                richTextBox1.Top = lblMainMessage.Top;
                richTextBox1.Height += lblMainMessage.Top;
                lblMainMessage.Visible = false;
            }

            ApplyTheme(theme);
            

            //try to resize form to fit bounds
            this.Size = FormsHelper.GetPreferredSizeOfTextControl(richTextBox1);
            this.Size = new Size(this.Size.Width + 10, this.Size.Height + 150);//leave a bit of padding

            var theScreen = Screen.FromControl(this);
            
            //enforce sensible max/min sizes
            Width = Math.Min(Math.Max(600, Width),theScreen.Bounds.Width - 400);

            //if the text is too long vertically just maximise the message box
            if (this.Height > theScreen.Bounds.Height)
                this.WindowState = FormWindowState.Maximized;

            richTextBox1.LinkClicked += richTextBox1_LinkClicked;
        }
        
        public static void Show(string mainMessage, string message, string environmentDotStackTrace = null, bool isModalDialog = true, string keywordNotToAdd = null,WideMessageBoxTheme theme = WideMessageBoxTheme.Exception)
        {
            WideMessageBox wmb = new WideMessageBox(mainMessage,message, environmentDotStackTrace, keywordNotToAdd, theme);

            if (isModalDialog)
                wmb.ShowDialog();
            else
                wmb.Show();
            
        }

        public static void Show(string mainMessage, string message, WideMessageBoxTheme theme)
        {
            Show(mainMessage, message,null,theme:theme);
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
            Clipboard.SetText(richTextBox1.Text);
        }

        private void WideMessageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
                this.Close();

            if(e.KeyCode == Keys.W && e.Control)
                this.Close();
        }

        private void btnViewStackTrace_Click(object sender, EventArgs e)
        {
            OnViewStackTrace();
        }

        protected virtual void OnViewStackTrace()
        {
            var dialog = new ExceptionViewerStackTraceWithHyperlinks(_environmentDotStackTrace);
            dialog.Show();
        }

        void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if(e.LinkText.Contains("#"))
            {
                var split = e.LinkText.Split('#');
                if(split.Length >=2 && CommentStore.ContainsKey(split[1]))
                    ShowKeywordHelp(split[1], CommentStore[split[1]]);
            }
        }

        public void Setup(string message, string keywordNotToAdd = null)
        {
            //unless the text is unreasonably long or we don't have help documentation available
            if (message.Length > 100000 || CommentStore == null)
            {
                richTextBox1.Text = message;
                return;
            }

            foreach (string word in message.Split(' '))
            {
                if (CommentStore.ContainsKey(word) && !word.Equals(keywordNotToAdd,StringComparison.CurrentCultureIgnoreCase))
                    richTextBox1.InsertLink(word, word);
                else
                    if (CommentStore.ContainsKey(word.Trim('s')) && !word.Trim('s').Equals(keywordNotToAdd,StringComparison.CurrentCultureIgnoreCase))
                        richTextBox1.InsertLink(word, word.Trim('s'));
                else
                    richTextBox1.SelectedText = word;

                richTextBox1.SelectedText = " ";
            }
        }
        
        public static void HighlightText(RichTextBox myRtb, string word, Color color)
        {
            if (word == string.Empty)
                return;
            var reg = new Regex(@"\b" + word + @"(\b|s\b)", RegexOptions.IgnoreCase);

            foreach (Match match in reg.Matches(myRtb.Text))
            {
                myRtb.Select(match.Index, match.Length);
                myRtb.SelectionColor = color;
            }

            myRtb.SelectionLength = 0;
            myRtb.SelectionColor = Color.Black;
        }
        
        private static void ShowHelpSection(HelpSection hs)
        {
            WideMessageBox.Show(hs.Keyword, hs.HelpText, Environment.StackTrace, false, hs.Keyword, WideMessageBoxTheme.Help);
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
    }
}
