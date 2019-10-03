using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.CohortUI.CohortSourceManagement
{
    partial class CreateNewCohortDatabaseWizardUI
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateNewCohortDatabaseWizardUI));
            this.listView1 = new BrightIdeasSoftware.ObjectListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvDataType = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNumberOfTimesSeen = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btnDiscoverExtractionIdentifiers = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.serverDatabaseTableSelector1 = new ReusableUIComponents.ServerDatabaseTableSelector();
            this.label2 = new System.Windows.Forms.Label();
            this.cbAllowNullReleaseIdentifiers = new System.Windows.Forms.CheckBox();
            this.helpIcon1 = new HelpIcon();
            this.helpIcon2 = new HelpIcon();
            ((System.ComponentModel.ISupportInitialize)(this.listView1)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.AllColumns.Add(this.olvName);
            this.listView1.AllColumns.Add(this.olvDataType);
            this.listView1.AllColumns.Add(this.olvNumberOfTimesSeen);
            this.listView1.CellEditUseWholeCell = false;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName,
            this.olvDataType,
            this.olvNumberOfTimesSeen});
            this.listView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 32);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(718, 256);
            this.listView1.TabIndex = 4;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // olvName
            // 
            this.olvName.AspectName = "RuntimeName";
            this.olvName.Text = "Column Name";
            this.olvName.Width = 117;
            // 
            // olvDataType
            // 
            this.olvDataType.AspectName = "DataType";
            this.olvDataType.Text = "Data Type";
            this.olvDataType.Width = 112;
            // 
            // olvNumberOfTimesSeen
            // 
            this.olvNumberOfTimesSeen.AspectName = "CountOfTimesSeen";
            this.olvNumberOfTimesSeen.Text = "Number of Times Seen";
            this.olvNumberOfTimesSeen.Width = 130;
            // 
            // btnDiscoverExtractionIdentifiers
            // 
            this.btnDiscoverExtractionIdentifiers.Location = new System.Drawing.Point(0, 3);
            this.btnDiscoverExtractionIdentifiers.Name = "btnDiscoverExtractionIdentifiers";
            this.btnDiscoverExtractionIdentifiers.Size = new System.Drawing.Size(379, 23);
            this.btnDiscoverExtractionIdentifiers.TabIndex = 3;
            this.btnDiscoverExtractionIdentifiers.Text = "1. Attempt to figure out what name/datatype my patient identifiers are";
            this.btnDiscoverExtractionIdentifiers.UseVisualStyleBackColor = true;
            this.btnDiscoverExtractionIdentifiers.Click += new System.EventHandler(this.btnDiscoverExtractionIdentifiers_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(6, 451);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(297, 23);
            this.btnNext.TabIndex = 9;
            this.btnNext.Text = "That\'s it! That\'s what I want, Create it for me now please";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // serverDatabaseTableSelector1
            // 
            this.serverDatabaseTableSelector1.AllowTableValuedFunctionSelection = false;
            this.serverDatabaseTableSelector1.AutoSize = true;
            this.serverDatabaseTableSelector1.Database = "";
            this.serverDatabaseTableSelector1.DatabaseType = FAnsi.DatabaseType.MicrosoftSQLServer;
            this.serverDatabaseTableSelector1.Location = new System.Drawing.Point(3, 307);
            this.serverDatabaseTableSelector1.Name = "serverDatabaseTableSelector1";
            this.serverDatabaseTableSelector1.Password = "";
            this.serverDatabaseTableSelector1.Server = "";
            this.serverDatabaseTableSelector1.Size = new System.Drawing.Size(623, 181);
            this.serverDatabaseTableSelector1.TabIndex = 11;
            this.serverDatabaseTableSelector1.Username = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 291);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(145, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "2. Pick Destination Database";
            // 
            // cbAllowNullReleaseIdentifiers
            // 
            this.cbAllowNullReleaseIdentifiers.AutoSize = true;
            this.cbAllowNullReleaseIdentifiers.Location = new System.Drawing.Point(6, 428);
            this.cbAllowNullReleaseIdentifiers.Name = "cbAllowNullReleaseIdentifiers";
            this.cbAllowNullReleaseIdentifiers.Size = new System.Drawing.Size(162, 17);
            this.cbAllowNullReleaseIdentifiers.TabIndex = 13;
            this.cbAllowNullReleaseIdentifiers.Text = "Allow Null Release Identifiers";
            this.cbAllowNullReleaseIdentifiers.UseVisualStyleBackColor = true;
            // 
            // helpIcon1
            // 
            this.helpIcon1.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon1.Location = new System.Drawing.Point(175, 428);
            this.helpIcon1.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIcon1.Name = "helpIcon1";
            this.helpIcon1.Size = new System.Drawing.Size(19, 19);
            this.helpIcon1.TabIndex = 14;
            // 
            // helpIcon2
            // 
            this.helpIcon2.BackColor = System.Drawing.Color.Transparent;
            this.helpIcon2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.helpIcon2.Location = new System.Drawing.Point(385, 7);
            this.helpIcon2.MaximumSize = new System.Drawing.Size(19, 19);
            this.helpIcon2.MinimumSize = new System.Drawing.Size(19, 19);
            this.helpIcon2.Name = "helpIcon2";
            this.helpIcon2.Size = new System.Drawing.Size(19, 19);
            this.helpIcon2.TabIndex = 15;
            this.helpIcon2.Click += new System.EventHandler(this.HelpIcon2_Click);
            // 
            // CreateNewCohortDatabaseWizardUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.helpIcon2);
            this.Controls.Add(this.helpIcon1);
            this.Controls.Add(this.cbAllowNullReleaseIdentifiers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.btnDiscoverExtractionIdentifiers);
            this.Controls.Add(this.serverDatabaseTableSelector1);
            this.Name = "CreateNewCohortDatabaseWizardUI";
            this.Size = new System.Drawing.Size(752, 543);
            ((System.ComponentModel.ISupportInitialize)(this.listView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.ObjectListView listView1;
        private BrightIdeasSoftware.OLVColumn olvName;
        private BrightIdeasSoftware.OLVColumn olvDataType;
        private BrightIdeasSoftware.OLVColumn olvNumberOfTimesSeen;
        private System.Windows.Forms.Button btnDiscoverExtractionIdentifiers;
        public System.Windows.Forms.Button btnNext;
        private ReusableUIComponents.ServerDatabaseTableSelector serverDatabaseTableSelector1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbAllowNullReleaseIdentifiers;
        private HelpIcon helpIcon1;
        private HelpIcon helpIcon2;
    }
}
