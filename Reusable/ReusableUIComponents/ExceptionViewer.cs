using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ReusableLibraryCode;


namespace ReusableUIComponents
{
    /// <summary>
    /// Used by the RDMP to tell you about something that went wrong.  You can select bits of the message text and copy them with Ctrl+C or select 'Copy to Clipboard' to copy all the
    /// message text in one go.  Clicking ViewException will launch a ExceptionViewerStackTraceWithHyperlinks for viewing the location of the error in the codebase (including viewing
    /// the source code at the point of the error).
    /// </summary>
    public partial class ExceptionViewer : Form
    {
        private readonly Exception _exception;
        
        public ExceptionViewer()
        {
            InitializeComponent();
        }

        private ExceptionViewer(string message, Exception exception)
        {
            _exception = exception;

            var aggregateException = _exception as AggregateException;

            if (aggregateException != null)
            {
                _exception = aggregateException.Flatten();

                if(aggregateException.InnerExceptions.Count == 1)
                    _exception = aggregateException.InnerExceptions[0];
            }

            InitializeComponent();
            
            richTextBox1.Text = message;

            richTextBox1.Font = new Font(FontFamily.GenericMonospace, richTextBox1.Font.Size);
            richTextBox1.Select(0,0);

            keywordHelpTextListbox.Setup(richTextBox1);
            splitContainer1.Panel2Collapsed = !keywordHelpTextListbox.HasEntries;

            //try to resize form to fit bounds
            this.Size = FormsHelper.GetPreferredSizeOfTextControl(richTextBox1);
            this.Size = new Size(this.Size.Width+10,this.Size.Height + 50);

            //put a reasonable minimum size on this messagebox
            this.Size = new Size(Math.Max(Size.Width, 500),Math.Max(this.Height,300));

            if (exception == null)
                btnViewException.Enabled = false;

            //can only write to clipboard in STA threads
            btnCopyToClipboard.Visible = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

            if (this.Height > Screen.FromControl(this).Bounds.Height)
                this.WindowState = FormWindowState.Maximized;
        }
        
        public static void Show(Exception exception, bool isModalDialog = true)
        {

            var message = ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            ExceptionViewer ev = new ExceptionViewer(message, exception);

            if (isModalDialog)
                ev.ShowDialog();
            else
                ev.Show();
        }
        public static void Show(string message, Exception exception, bool isModalDialog = true)
        {
            //if the API user is not being silly and passing a message that is the exception anyway!
            if (!message.Contains(exception.Message))
                message = message + Environment.NewLine + Environment.NewLine + ExceptionHelper.ExceptionToListOfInnerMessages(exception);

            ExceptionViewer ev = new ExceptionViewer(message,exception);

            if(isModalDialog)
                ev.ShowDialog();
            else
                ev.Show();
        }

        private void btnViewException_Click(object sender, EventArgs e)
        {
            
            if (ExceptionViewerStackTraceWithHyperlinks.IsSourceCodeAvailable(_exception))
            {
                ExceptionViewerStackTraceWithHyperlinks.Show(_exception);
                return;
            }
            string exceptionAsString = ExceptionHelper.ExceptionToListOfInnerMessages(_exception,true);
            WideMessageBox.Show(exceptionAsString);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void ExceptionViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
    }
}
