using System;
using System.Windows.Forms;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Dialogs;

namespace ReusableUIComponents.ChecksUI
{
    public class MakeChangePopup:ICheckNotifier
    {
        private readonly YesNoYesToAllDialog _dialog;

        public MakeChangePopup(YesNoYesToAllDialog dialog)
        {
            _dialog = dialog;
        }

        public static bool ShowYesNoMessageBoxToApplyFix(YesNoYesToAllDialog dialog, string problem, string proposedChange)
        {
            string message = "The following configuration problem was detected:" + Environment.NewLine + "\"" + problem + "\"" + Environment.NewLine;
            message += Environment.NewLine;
            message += " The proposed fix is to:" + Environment.NewLine + "\"" + proposedChange + "\"" + Environment.NewLine; ;
            message += Environment.NewLine;
            message += "Would you like to apply this fix?";

            if (dialog == null)
                return MessageBox.Show(message, "Apply Fix?", MessageBoxButtons.YesNo) == DialogResult.Yes;

            return dialog.ShowDialog(message, "Apply Fix?") == DialogResult.Yes;
        }

        public bool OnCheckPerformed(CheckEventArgs args)
        {
            //if there is a fix suggest it to the user
            if (args.ProposedFix != null)
                return ShowYesNoMessageBoxToApplyFix(_dialog,args.Message, args.ProposedFix);
            
            //else show an Exception
            if(args.Ex != null)
                ExceptionViewer.Show(args.Ex);
            else
            if(args.Result == CheckResult.Fail)
                WideMessageBox.Show(args.Message,"",environmentDotStackTrace: Environment.StackTrace);

            return false;
        }
    }
}
