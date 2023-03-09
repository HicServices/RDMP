// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.UI.SimpleDialogs;
using WideMessageBox = Rdmp.UI.SimpleDialogs.WideMessageBox;

namespace Rdmp.UI.ChecksUI
{
    /// <summary>
    /// Yes/No dialog for handling <see cref="CheckEventArgs.ProposedFix"/>.  Describes the fix and prompts the user for a response.  Includes
    /// support for Yes to All.
    /// </summary>
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
            message += " The proposed fix is to:" + Environment.NewLine + "\"" + proposedChange + "\"" + Environment.NewLine;
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
