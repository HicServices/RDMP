using Rdmp.Core.CommandExecution;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rdmp.UI.SimpleDialogs
{
    public partial class TaskDescriptionLabel : UserControl
    {
        public TaskDescriptionLabel()
        {
            InitializeComponent();
        }

        public void SetupFor(DialogArgs args)
        {
            var task = args.TaskDescription;
            var entryLabel = args.EntryLabel;

            lblTaskDescription.Visible = !string.IsNullOrWhiteSpace(task);
            lblTaskDescription.Text = task;

            lblEntryLabel.Visible = !string.IsNullOrWhiteSpace(entryLabel);

            if (entryLabel != null && entryLabel.Length > WideMessageBox.MAX_LENGTH_BODY)
                entryLabel = entryLabel.Substring(0, WideMessageBox.MAX_LENGTH_BODY);

            // set prompt text. If theres a TaskDescription too then leave a bit of extra space
            this.lblEntryLabel.Text = !string.IsNullOrWhiteSpace(task) ? Environment.NewLine + entryLabel : entryLabel;

            this.Height = (!string.IsNullOrWhiteSpace(entryLabel) ? lblEntryLabel.Height : 0) + 
                          (!string.IsNullOrWhiteSpace(task) ? lblTaskDescription.Height : 0);
        }

        /// <summary>
        /// Returns the width this control would ideally like to take up
        /// </summary>
        public int PreferredWidth => Math.Max(lblEntryLabel.PreferredWidth, lblTaskDescription.PreferredWidth);
    }
}
