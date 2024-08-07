using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ItemActivation;
using System;
using System.Linq;
using System.Windows.Forms;

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
            btnShowTicket = new Button();
            gbTicketing = new GroupBox();
            label1 = new Label();
            gbTicketing.SuspendLayout();
            SuspendLayout();
            // 
            // btnShowTicket
            // 
            btnShowTicket.Location = new System.Drawing.Point(67, 13);
            btnShowTicket.Margin = new Padding(4, 3, 4, 3);
            btnShowTicket.Name = "btnShowTicket";
            btnShowTicket.Size = new System.Drawing.Size(94, 25);
            btnShowTicket.TabIndex = 32;
            btnShowTicket.Text = "Save Version";
            btnShowTicket.UseVisualStyleBackColor = true;
            btnShowTicket.Click += CommitNewVersion;
            // 
            // gbTicketing
            // 
            gbTicketing.Controls.Add(label1);
            gbTicketing.Controls.Add(btnShowTicket);
            gbTicketing.Location = new System.Drawing.Point(0, -8);
            gbTicketing.Margin = new Padding(4, 3, 4, 3);
            gbTicketing.Name = "gbTicketing";
            gbTicketing.Padding = new Padding(4, 3, 4, 3);
            gbTicketing.Size = new System.Drawing.Size(168, 41);
            gbTicketing.TabIndex = 37;
            gbTicketing.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(65, 15);
            label1.TabIndex = 33;
            label1.Text = "Versioning:";
            // 
            // VersioningControlUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(gbTicketing);
            Margin = new Padding(4, 3, 4, 3);
            Name = "VersioningControlUI";
            Size = new System.Drawing.Size(168, 33);
            gbTicketing.ResumeLayout(false);
            gbTicketing.PerformLayout();
            ResumeLayout(false);
        }

        private void CommitNewVersion(object sender, EventArgs e)
        {
            if (_cic.Version != null)
            {
                if (_activator.YesNo("Are you sure you want to revert the cohort to this version?", "Revert Cohort to this Version"))
                {
                    var rootCic = _activator.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("ID", _cic.ClonedFrom_ID).FirstOrDefault();
                    if (rootCic != null)
                    {
                        var revertCmd = new ExecuteCommandRevertToHistoricalCohortVersion(_activator, rootCic, _cic);
                        revertCmd.Execute();
                    }
                }
                return;
            }
            var versions = _cic.GetVersions();
            var addedNewDescription = _activator.TypeText("Add a description of this new version", "Would you like to update the description of this new cohort version?", 250, _cic.Description, out string newDescription, false);
            var cmd = new ExecuteCommandCreateVersionOfCohortConfiguration(_activator, _cic, $"{_cic.Name}-v{versions.Count + 1}-{DateTime.Now.ToString("yyyy-MM-dd")}", addedNewDescription ? newDescription : null);
            cmd.Execute();
        }


        public void Setup(CohortIdentificationConfiguration databaseObject, IActivateItems activator)
        {
            _cic = databaseObject;
            _activator = activator;
            var versions = databaseObject.GetVersions();
            if (_cic is not null)
                btnShowTicket.Text = _cic.Version is null ? "Save Version" : "Restore";
            versions.Insert(0, databaseObject);
        }

        #endregion

        private Button btnShowTicket;
        private GroupBox gbTicketing;
        private Label label1;
    }
}
