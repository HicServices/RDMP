using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents;
using System;
using System.Linq;
using System.Windows.Forms;
using static Azure.Core.HttpHeader;

namespace Rdmp.UI.LocationsMenu.Versioning
{
    partial class VersioningControlUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private CohortIdentificationConfiguration _cic;
        private IActivateItems _activator;
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gbTicketing = new System.Windows.Forms.GroupBox();
            btnShowTicket = new System.Windows.Forms.Button();
            tbTicket = new System.Windows.Forms.ComboBox();
            label6 = new System.Windows.Forms.Label();
            gbTicketing.SuspendLayout();
            SuspendLayout();
            // 
            // gbTicketing
            // 
            gbTicketing.Controls.Add(btnShowTicket);
            gbTicketing.Controls.Add(tbTicket);
            gbTicketing.Controls.Add(label6);
            gbTicketing.Location = new System.Drawing.Point(4, 3);
            gbTicketing.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbTicketing.Name = "gbTicketing";
            gbTicketing.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gbTicketing.Size = new System.Drawing.Size(343, 53);
            gbTicketing.TabIndex = 37;
            gbTicketing.TabStop = false;
            gbTicketing.Text = "Versioning";
            // 
            // btnShowTicket
            // 
            btnShowTicket.Location = new System.Drawing.Point(241, 19);
            btnShowTicket.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnShowTicket.Name = "btnShowTicket";
            btnShowTicket.Size = new System.Drawing.Size(94, 25);
            btnShowTicket.TabIndex = 32;
            btnShowTicket.Text = "Save Current";
            btnShowTicket.UseVisualStyleBackColor = true;
            btnShowTicket.Click += CommitNewVersion;
            // 
            // tbTicket
            // 
            tbTicket.Location = new System.Drawing.Point(50, 20);
            tbTicket.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbTicket.Name = "tbTicket";
            tbTicket.Size = new System.Drawing.Size(187, 23);
            tbTicket.TabIndex = 30;
            //tbTicket.TextChanged += tbTicket_TextChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(6, 23);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(48, 15);
            label6.TabIndex = 31;
            label6.Text = "Version:";
            // 
            // VersioningControlUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(gbTicketing);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "VersioningControlUI";
            Size = new System.Drawing.Size(354, 62);
            gbTicketing.ResumeLayout(false);
            gbTicketing.PerformLayout();
            ResumeLayout(false);
        }

        private void VersionChange(object sender, EventArgs e)
        {
            if (tbTicket.SelectedItem is CohortIdentificationConfiguration ei && ei.ID != _cic.ID)
            {
                _activator.Activate<CohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(ei);
                //reset current dropdown
                tbTicket.SelectedIndex = 0;
            }
        }

        private void CommitNewVersion(object sender, EventArgs e)
        {
            var dialog = new TypeTextOrCancelDialog("Version Name", "Enter a name for your stored version", 450);
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                var cmd = new ExecuteCommandCreateVersionOfCohortConfiguration(_activator, _cic, dialog.ResultText);
                cmd.Execute();
                var versions = _cic.GetVersions();
                versions.Insert(0, _cic);
                tbTicket.DataSource = versions;
                tbTicket.Enabled = true;
                //needs to refresh the UI
            }
        }


        public void Setup(CohortIdentificationConfiguration databaseObject, IActivateItems activator)
        {
            _cic = databaseObject;
            _activator = activator;
            tbTicket.DropDownStyle = ComboBoxStyle.DropDownList;
            var versions = databaseObject.GetVersions();
            if (!versions.Any() || databaseObject.Version is not null)
            {
                tbTicket.Enabled = false;
                label6.Enabled = false;
            }
            if (databaseObject.Version is not null)
            {
                btnShowTicket.Enabled = false;
            }
            versions.Insert(0, databaseObject);
            tbTicket.DataSource = versions;
        }
        #endregion

        private System.Windows.Forms.GroupBox gbTicketing;
        private System.Windows.Forms.Button btnShowTicket;
        private System.Windows.Forms.ComboBox tbTicket;
        private System.Windows.Forms.Label label6;
        
    }
}
