using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ReusableLibraryCode;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace ReusableUIComponents.SqlDialogs
{
    /// <summary>
    /// Shows two pieces of SQL and the differences between them.  This is used by the RDMP for example to show you what the audited extraction SQL for a dataset was and what you 
    /// last extracted it (e.g. before the weekend) and what the active configuration looks like today (e.g. if somebody snuck in a couple of extra columns into a data extraction
    /// after the extract file had already been generated).
    /// </summary>
    public partial class SQLBeforeAndAfterViewer : Form
    {
        private Scintilla QueryEditorBefore;
        private Scintilla QueryEditorAfter;


        public SQLBeforeAndAfterViewer(string sqlBefore, string sqlAfter, string headerTextForBefore, string headerTextForAfter, string caption, MessageBoxButtons buttons, string language = "mssql")
        {
            InitializeComponent();

            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

            if (designMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditorBefore = new ScintillaTextEditorFactory().Create();
            QueryEditorBefore.Text = sqlBefore;
            QueryEditorBefore.ReadOnly = true;

            splitContainer1.Panel1.Controls.Add(QueryEditorBefore);


            QueryEditorAfter = new ScintillaTextEditorFactory().Create();
            QueryEditorAfter.Text = sqlAfter;
            QueryEditorAfter.ReadOnly = true;

            splitContainer1.Panel2.Controls.Add(QueryEditorAfter);

            
            //compute difference
            var highlighter = new ScintillaLineHighlightingHelper();
            highlighter.ClearAll(QueryEditorAfter);
            highlighter.ClearAll(QueryEditorBefore);
            
            if (sqlBefore == null)
                sqlBefore = "";
            if (sqlAfter == null)
                sqlAfter = "";

            Diff diff = new Diff();

            foreach (Diff.Item item in diff.DiffText(sqlBefore, sqlAfter))
            {
                for (int i = item.StartA; i < item.StartA+item.deletedA; i++)
                        highlighter.HighlightLine(QueryEditorBefore,i,Color.Pink);
                    
                for (int i = item.StartB; i < item.StartB+item.insertedB; i++)
                    highlighter.HighlightLine(QueryEditorAfter, i, Color.LawnGreen);
                
            }

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    btnYes.Visible = true;
                    btnYes.Text = "Ok";
                    btnNo.Visible = false;
                    break;
                case MessageBoxButtons.YesNo:
                    btnYes.Visible = true;
                    btnNo.Visible = true;
                    break;
                default:
                    throw new NotSupportedException("buttons");
            }

            lblBefore.Text = headerTextForBefore;
            lblAfter.Text = headerTextForAfter;

            this.Text = caption;
        }
        
        private void btnYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
           this.Close();
        }


    }
}
