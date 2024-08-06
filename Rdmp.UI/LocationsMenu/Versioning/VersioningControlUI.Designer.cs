using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SubComponents;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Terminal.Gui;
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
            gbTicketing = new GroupBox();
            btnShowTicket = new System.Windows.Forms.Button();
            gbTicketing.SuspendLayout();
            SuspendLayout();
            // 
            // gbTicketing
            // 
            gbTicketing.Controls.Add(btnShowTicket);
            gbTicketing.Location = new System.Drawing.Point(4, 0);
            gbTicketing.Margin = new Padding(4, 3, 4, 3);
            gbTicketing.Name = "gbTicketing";
            gbTicketing.Padding = new Padding(4, 3, 4, 3);
            gbTicketing.Size = new System.Drawing.Size(99, 34);
            gbTicketing.TabIndex = 37;
            gbTicketing.TabStop = false;
            // 
            // btnShowTicket
            // 
            btnShowTicket.Location = new System.Drawing.Point(0, 3);
            btnShowTicket.Margin = new Padding(4, 3, 4, 3);
            btnShowTicket.Name = "btnShowTicket";
            btnShowTicket.Size = new System.Drawing.Size(94, 25);
            btnShowTicket.TabIndex = 32;
            btnShowTicket.Text = "Save Version";
            btnShowTicket.UseVisualStyleBackColor = true;
            btnShowTicket.Click += CommitNewVersion;
            // 
            // VersioningControlUI
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(gbTicketing);
            Margin = new Padding(4, 3, 4, 3);
            Name = "VersioningControlUI";
            Size = new System.Drawing.Size(103, 33);
            gbTicketing.ResumeLayout(false);
            ResumeLayout(false);
        }

        //private void VersionChange(object sender, EventArgs e)
        //{
        //    if (tbTicket.SelectedItem is CohortIdentificationConfiguration ei && ei.ID != _cic.ID)
        //    {
        //        _activator.Activate<CohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(ei);
        //        //reset current dropdown
        //        tbTicket.SelectedIndex = 0;
        //    }
        //}

        private void tbTicket_MouseOver(object sender, EventArgs e)
        {
            ToolTip buttonToolTip = new ToolTip();
            buttonToolTip.ToolTipTitle = "Value";
            buttonToolTip.UseFading = true;
            buttonToolTip.UseAnimation = true;
            buttonToolTip.IsBalloon = true;
            buttonToolTip.ShowAlways = true;
            buttonToolTip.AutoPopDelay = 5000;
            buttonToolTip.InitialDelay = 1000;
            buttonToolTip.ReshowDelay = 0;

            //buttonToolTip.SetToolTip(tbTicket, tbTicket.Text);
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
            versions = _cic.GetVersions();
            versions.Insert(0, _cic);
            //tbTicket.DataSource = versions;
            //tbTicket.Enabled = true;
        }


        public void Setup(CohortIdentificationConfiguration databaseObject, IActivateItems activator)
        {
            _cic = databaseObject;
            _activator = activator;
            //tbTicket.DropDownStyle = ComboBoxStyle.DropDownList;
            //int cbWidth = (int)tbTicket.DropDownWidth;
            var versions = databaseObject.GetVersions();
            if (!versions.Any() || databaseObject.Version is not null)
            {
                //tbTicket.Enabled = false;
                //label6.Enabled = false;
            }
            if (_cic is not null)
                btnShowTicket.Text = _cic.Version is null ? "Save Version" : "Restore";
            versions.Insert(0, databaseObject);
            //tbTicket.DataSource = versions;
            foreach (var version in versions)
            {
                var longestItem = CreateGraphics().MeasureString(version.Name, SystemFonts.MessageBoxFont).Width;
                //if (longestItem > cbWidth)
                //{
                //    cbWidth = (int)longestItem + 1;
                //}

            }
            //tbTicket.DropDownWidth = cbWidth;

        }
        #endregion

        private System.Windows.Forms.GroupBox gbTicketing;
        private System.Windows.Forms.Button btnShowTicket;
    }
}
