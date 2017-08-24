using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using HIC.Logging.PastEvents;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace Dashboard.CatalogueSummary.LoadEvents
{
    /// <summary>
    /// Fatal errors are crashes of the data load engine when it is attempting to load data.  If you use the RAW / STAGING / LIVE model this should almost never result in contaminating your
    /// live dataset (See 'RAW, STAGING, LIVE' in UserManual.docx).  If however you have lots of post load tasks or other custom functionality with knock on consequences it is possible
    /// that a crash will result unforeseen consequences.  For this reason all failure messages are stored in an 'unresolved' state.  Through LoadEventsTreeView you can launch this dialog.
    /// 
    /// This dialog lets you mark errors as 'resolved' and provide a message as to why the error happened and the steps you have taken to resolve it e.g. "Data load failed because data provider
    /// has started sending 'Male' instead of 'M' for column 'Gender', I have adjusted the column width to varchar(10) to compensate and have created an issue to standardise the column values".
    /// </summary>
    public partial class ResolveFatalErrors : RDMPForm
    {
        private readonly LogManager _logManager;
        private readonly ArchivalFatalError[] _toResolve;
        public Scintilla Explanation { get; set; }

        public ResolveFatalErrors(LogManager logManager, ArchivalFatalError[] toResolve)
        {
            
            _logManager = logManager;
            _toResolve = toResolve;
            InitializeComponent();

            if (VisualStudioDesignMode || logManager == null) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            Explanation = new ScintillaTextEditorFactory().Create(new RDMPCommandFactory());
            Explanation.ReadOnly = false;

            //if there is only 1 explanation already recorded then we should populate the explanation textbox with thsi
            if (toResolve.Select(e => e.Explanation).Distinct().Count() == 1)
                Explanation.Text = toResolve.First().Explanation;

            tbFatalErrorIDs.Text = string.Join(",", toResolve.Select(f => f.ID));
            pbExplanation.Controls.Add(Explanation);
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            FatalErrorLogging.FatalErrorStates newState = FatalErrorLogging.FatalErrorStates.Resolved;
            
            if (string.IsNullOrEmpty(Explanation.Text))
            {

                var answer = MessageBox.Show(
                    @"You have no explanation, are you trying to UNRESOLVE a previously RESOLVED error?
Yes - Delete Explanation in database and mark as unresolved
No - Mark as Resolved but with no Explanation
Cancel - Do not close form and do not make any database changes", "Unresolve", MessageBoxButtons.YesNoCancel);


                if(answer == DialogResult.Cancel)
                    return;

                if(answer == DialogResult.Yes)
                    newState = FatalErrorLogging.FatalErrorStates.Outstanding;
                
                //this is so hacky because ArchivalFatalError has no concept of statusID (resolved/unresolved/blocked).  Originally fatal error recording allowed for agency specific states and even for them to delete these states but that is so not required.  Who needs more than resolved/unresolved? so now we just assume Explanation = resolved, No Explanation = unresolved.  Use CatalogueItemIssues if you want real issue recording or a TicketingSystem
                if (answer == DialogResult.No)
                    Explanation.Text = "No Explanation"; 
            }

            //resolve it in the database
            _logManager.ResolveFatalErrors(_toResolve.Select(f => f.ID).ToArray(), newState, Explanation.Text);
            
            //resolve it in memory
            foreach (ArchivalFatalError error in _toResolve)
                error.Explanation = Explanation.Text;

            this.Close();
        }

        
    }
}
