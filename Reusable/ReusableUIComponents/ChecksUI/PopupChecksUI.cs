using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode.Checks;


namespace ReusableUIComponents.ChecksUI
{
    /// <summary>
    /// Popup dialog version of ChecksUI, See ChecksUI for description of functionality.
    /// </summary>
    public partial class PopupChecksUI : Form,ICheckNotifier
    {
        public PopupChecksUI(string task,bool showOnlyWhenError)
        {
            InitializeComponent();
            Text = task;

            if (!showOnlyWhenError)
            {
                Show();
                haveDemandedVisibility = true;
            }
            else
                this.CreateHandle(); //let windows get a handle on the situation ;)
        }

        private bool haveDemandedVisibility = false;
        private CheckResult _worstSeen = CheckResult.Success;

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (_worstSeen < args.Result)
                _worstSeen = args.Result;

            if(args.Result == CheckResult.Fail || args.Result == CheckResult.Warning)
                if(!haveDemandedVisibility)
                {
                    haveDemandedVisibility = true;
                    Invoke(new MethodInvoker(Show));
                }

            return checksUI1.OnCheckPerformed(args);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (haveDemandedVisibility)
                checksUI1.TerminateWithExtremePrejudice();

            base.OnClosed(e);
        }

        public void Check(ICheckable checkable)
        {
            try
            {
                checkable.Check(this);
            }
            catch (Exception ex)
            {
                checksUI1.OnCheckPerformed(new CheckEventArgs("Entire checking process failed", CheckResult.Fail, ex));
            }
        }

        public void StartChecking(ICheckable checkable)
        {
            this.Show();
            checksUI1.StartChecking(checkable);
        }

        public CheckResult GetWorst()
        {
            return _worstSeen;
        }
    }
}
