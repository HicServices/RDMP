namespace CatalogueManager.LocationsMenu
{
    partial class ServerDefaultsUI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnCreateNewANOStore = new System.Windows.Forms.Button();
            this.btnClearANOStore = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.ddDefaultANOStore = new System.Windows.Forms.ComboBox();
            this.btnCreateNewIdentifierDump = new System.Windows.Forms.Button();
            this.btnCreateNewLoggingServer = new System.Windows.Forms.Button();
            this.btnClearIdentifierDump = new System.Windows.Forms.Button();
            this.btnClearLoggingServer = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.ddDefaultIdentifierDump = new System.Windows.Forms.ComboBox();
            this.ddDefaultLoggingServer = new System.Windows.Forms.ComboBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnCreateNewDQEServer = new System.Windows.Forms.Button();
            this.btnClearDQEServer = new System.Windows.Forms.Button();
            this.ddDQEServer = new System.Windows.Forms.ComboBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnCreateNewCohortIdentificationQueryCache = new System.Windows.Forms.Button();
            this.btnClearCohortIdentificationQueryCache = new System.Windows.Forms.Button();
            this.ddCohortIdentificationQueryCacheServer = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btnCreateNewWebServiceQueryCache = new System.Windows.Forms.Button();
            this.btnClearWebServiceQueryCache = new System.Windows.Forms.Button();
            this.ddWebServiceQueryCacheServer = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.btnClearRAWServer = new System.Windows.Forms.Button();
            this.ddOverrideRawServer = new System.Windows.Forms.ComboBox();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnCreateNewANOStore);
            this.groupBox3.Controls.Add(this.btnClearANOStore);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.ddDefaultANOStore);
            this.groupBox3.Controls.Add(this.btnCreateNewIdentifierDump);
            this.groupBox3.Controls.Add(this.btnCreateNewLoggingServer);
            this.groupBox3.Controls.Add(this.btnClearIdentifierDump);
            this.groupBox3.Controls.Add(this.btnClearLoggingServer);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.ddDefaultIdentifierDump);
            this.groupBox3.Controls.Add(this.ddDefaultLoggingServer);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1124, 106);
            this.groupBox3.TabIndex = 13;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Defaults:";
            // 
            // btnCreateNewANOStore
            // 
            this.btnCreateNewANOStore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewANOStore.Location = new System.Drawing.Point(1036, 77);
            this.btnCreateNewANOStore.Name = "btnCreateNewANOStore";
            this.btnCreateNewANOStore.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewANOStore.TabIndex = 11;
            this.btnCreateNewANOStore.Text = "Create New...";
            this.btnCreateNewANOStore.UseVisualStyleBackColor = true;
            this.btnCreateNewANOStore.Click += new System.EventHandler(this.btnCreateNewANOStore_Click);
            // 
            // btnClearANOStore
            // 
            this.btnClearANOStore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearANOStore.Location = new System.Drawing.Point(969, 77);
            this.btnClearANOStore.Name = "btnClearANOStore";
            this.btnClearANOStore.Size = new System.Drawing.Size(65, 23);
            this.btnClearANOStore.TabIndex = 10;
            this.btnClearANOStore.Text = "Clear";
            this.btnClearANOStore.UseVisualStyleBackColor = true;
            this.btnClearANOStore.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 82);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(163, 13);
            this.label13.TabIndex = 9;
            this.label13.Text = "ANOStore (For new ANOTables):";
            // 
            // ddDefaultANOStore
            // 
            this.ddDefaultANOStore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDefaultANOStore.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDefaultANOStore.FormattingEnabled = true;
            this.ddDefaultANOStore.Location = new System.Drawing.Point(220, 79);
            this.ddDefaultANOStore.Name = "ddDefaultANOStore";
            this.ddDefaultANOStore.Size = new System.Drawing.Size(743, 21);
            this.ddDefaultANOStore.TabIndex = 8;
            this.ddDefaultANOStore.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // btnCreateNewIdentifierDump
            // 
            this.btnCreateNewIdentifierDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewIdentifierDump.Location = new System.Drawing.Point(1036, 50);
            this.btnCreateNewIdentifierDump.Name = "btnCreateNewIdentifierDump";
            this.btnCreateNewIdentifierDump.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewIdentifierDump.TabIndex = 7;
            this.btnCreateNewIdentifierDump.Text = "Create New...";
            this.btnCreateNewIdentifierDump.UseVisualStyleBackColor = true;
            this.btnCreateNewIdentifierDump.Click += new System.EventHandler(this.btnCreateNewIdentifierDump_Click);
            // 
            // btnCreateNewLoggingServer
            // 
            this.btnCreateNewLoggingServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewLoggingServer.Location = new System.Drawing.Point(1036, 21);
            this.btnCreateNewLoggingServer.Name = "btnCreateNewLoggingServer";
            this.btnCreateNewLoggingServer.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewLoggingServer.TabIndex = 7;
            this.btnCreateNewLoggingServer.Text = "Create New...";
            this.btnCreateNewLoggingServer.UseVisualStyleBackColor = true;
            this.btnCreateNewLoggingServer.Click += new System.EventHandler(this.btnCreateNewLoggingServer_Click);
            // 
            // btnClearIdentifierDump
            // 
            this.btnClearIdentifierDump.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearIdentifierDump.Location = new System.Drawing.Point(969, 50);
            this.btnClearIdentifierDump.Name = "btnClearIdentifierDump";
            this.btnClearIdentifierDump.Size = new System.Drawing.Size(65, 23);
            this.btnClearIdentifierDump.TabIndex = 3;
            this.btnClearIdentifierDump.Text = "Clear";
            this.btnClearIdentifierDump.UseVisualStyleBackColor = true;
            this.btnClearIdentifierDump.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // btnClearLoggingServer
            // 
            this.btnClearLoggingServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearLoggingServer.Location = new System.Drawing.Point(969, 21);
            this.btnClearLoggingServer.Name = "btnClearLoggingServer";
            this.btnClearLoggingServer.Size = new System.Drawing.Size(65, 23);
            this.btnClearLoggingServer.TabIndex = 3;
            this.btnClearLoggingServer.Text = "Clear";
            this.btnClearLoggingServer.UseVisualStyleBackColor = true;
            this.btnClearLoggingServer.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 55);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(181, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "Identifier Dump (For new TableInfos):";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 26);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(185, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Logging Server (For new Catalogues):";
            // 
            // ddDefaultIdentifierDump
            // 
            this.ddDefaultIdentifierDump.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDefaultIdentifierDump.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDefaultIdentifierDump.FormattingEnabled = true;
            this.ddDefaultIdentifierDump.Location = new System.Drawing.Point(220, 52);
            this.ddDefaultIdentifierDump.Name = "ddDefaultIdentifierDump";
            this.ddDefaultIdentifierDump.Size = new System.Drawing.Size(743, 21);
            this.ddDefaultIdentifierDump.TabIndex = 1;
            this.ddDefaultIdentifierDump.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // ddDefaultLoggingServer
            // 
            this.ddDefaultLoggingServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDefaultLoggingServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDefaultLoggingServer.FormattingEnabled = true;
            this.ddDefaultLoggingServer.Location = new System.Drawing.Point(220, 23);
            this.ddDefaultLoggingServer.Name = "ddDefaultLoggingServer";
            this.ddDefaultLoggingServer.Size = new System.Drawing.Size(743, 21);
            this.ddDefaultLoggingServer.TabIndex = 1;
            this.ddDefaultLoggingServer.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.btnCreateNewDQEServer);
            this.groupBox4.Controls.Add(this.btnClearDQEServer);
            this.groupBox4.Controls.Add(this.ddDQEServer);
            this.groupBox4.Location = new System.Drawing.Point(12, 124);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1126, 62);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Data Quality Engine Reporting Database";
            // 
            // btnCreateNewDQEServer
            // 
            this.btnCreateNewDQEServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewDQEServer.Location = new System.Drawing.Point(1038, 19);
            this.btnCreateNewDQEServer.Name = "btnCreateNewDQEServer";
            this.btnCreateNewDQEServer.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewDQEServer.TabIndex = 6;
            this.btnCreateNewDQEServer.Text = "Create New...";
            this.btnCreateNewDQEServer.UseVisualStyleBackColor = true;
            this.btnCreateNewDQEServer.Click += new System.EventHandler(this.btnCreateNewDQEServer_Click);
            // 
            // btnClearDQEServer
            // 
            this.btnClearDQEServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearDQEServer.Location = new System.Drawing.Point(969, 20);
            this.btnClearDQEServer.Name = "btnClearDQEServer";
            this.btnClearDQEServer.Size = new System.Drawing.Size(65, 23);
            this.btnClearDQEServer.TabIndex = 6;
            this.btnClearDQEServer.Text = "Clear";
            this.btnClearDQEServer.UseVisualStyleBackColor = true;
            this.btnClearDQEServer.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // ddDQEServer
            // 
            this.ddDQEServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDQEServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDQEServer.FormattingEnabled = true;
            this.ddDQEServer.Location = new System.Drawing.Point(10, 22);
            this.ddDQEServer.Name = "ddDQEServer";
            this.ddDQEServer.Size = new System.Drawing.Size(953, 21);
            this.ddDQEServer.TabIndex = 4;
            this.ddDQEServer.SelectedIndexChanged += new System.EventHandler(this.ddDQEServer_SelectedIndexChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.btnCreateNewCohortIdentificationQueryCache);
            this.groupBox5.Controls.Add(this.btnClearCohortIdentificationQueryCache);
            this.groupBox5.Controls.Add(this.ddCohortIdentificationQueryCacheServer);
            this.groupBox5.Controls.Add(this.label16);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.btnCreateNewWebServiceQueryCache);
            this.groupBox5.Controls.Add(this.btnClearWebServiceQueryCache);
            this.groupBox5.Controls.Add(this.ddWebServiceQueryCacheServer);
            this.groupBox5.Location = new System.Drawing.Point(12, 192);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1126, 99);
            this.groupBox5.TabIndex = 15;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Query Result Caching";
            // 
            // btnCreateNewCohortIdentificationQueryCache
            // 
            this.btnCreateNewCohortIdentificationQueryCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewCohortIdentificationQueryCache.Location = new System.Drawing.Point(1038, 52);
            this.btnCreateNewCohortIdentificationQueryCache.Name = "btnCreateNewCohortIdentificationQueryCache";
            this.btnCreateNewCohortIdentificationQueryCache.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewCohortIdentificationQueryCache.TabIndex = 14;
            this.btnCreateNewCohortIdentificationQueryCache.Text = "Create New...";
            this.btnCreateNewCohortIdentificationQueryCache.UseVisualStyleBackColor = true;
            this.btnCreateNewCohortIdentificationQueryCache.Click += new System.EventHandler(this.btnCreateNewCohortIdentificationQueryCache_Click);
            // 
            // btnClearCohortIdentificationQueryCache
            // 
            this.btnClearCohortIdentificationQueryCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearCohortIdentificationQueryCache.Location = new System.Drawing.Point(969, 53);
            this.btnClearCohortIdentificationQueryCache.Name = "btnClearCohortIdentificationQueryCache";
            this.btnClearCohortIdentificationQueryCache.Size = new System.Drawing.Size(65, 23);
            this.btnClearCohortIdentificationQueryCache.TabIndex = 15;
            this.btnClearCohortIdentificationQueryCache.Text = "Clear";
            this.btnClearCohortIdentificationQueryCache.UseVisualStyleBackColor = true;
            this.btnClearCohortIdentificationQueryCache.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // ddCohortIdentificationQueryCacheServer
            // 
            this.ddCohortIdentificationQueryCacheServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddCohortIdentificationQueryCacheServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddCohortIdentificationQueryCacheServer.FormattingEnabled = true;
            this.ddCohortIdentificationQueryCacheServer.Location = new System.Drawing.Point(198, 55);
            this.ddCohortIdentificationQueryCacheServer.Name = "ddCohortIdentificationQueryCacheServer";
            this.ddCohortIdentificationQueryCacheServer.Size = new System.Drawing.Size(765, 21);
            this.ddCohortIdentificationQueryCacheServer.TabIndex = 13;
            this.ddCohortIdentificationQueryCacheServer.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(7, 58);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(169, 13);
            this.label16.TabIndex = 12;
            this.label16.Text = "Cohort Identification Query Cache:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(166, 13);
            this.label9.TabIndex = 12;
            this.label9.Text = "Web Server Query Cache Server:";
            // 
            // btnCreateNewWebServiceQueryCache
            // 
            this.btnCreateNewWebServiceQueryCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateNewWebServiceQueryCache.Location = new System.Drawing.Point(1038, 19);
            this.btnCreateNewWebServiceQueryCache.Name = "btnCreateNewWebServiceQueryCache";
            this.btnCreateNewWebServiceQueryCache.Size = new System.Drawing.Size(80, 23);
            this.btnCreateNewWebServiceQueryCache.TabIndex = 6;
            this.btnCreateNewWebServiceQueryCache.Text = "Create New...";
            this.btnCreateNewWebServiceQueryCache.UseVisualStyleBackColor = true;
            this.btnCreateNewWebServiceQueryCache.Click += new System.EventHandler(this.btnCreateNewWebServiceQueryCache_Click);
            // 
            // btnClearWebServiceQueryCache
            // 
            this.btnClearWebServiceQueryCache.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearWebServiceQueryCache.Location = new System.Drawing.Point(969, 20);
            this.btnClearWebServiceQueryCache.Name = "btnClearWebServiceQueryCache";
            this.btnClearWebServiceQueryCache.Size = new System.Drawing.Size(65, 23);
            this.btnClearWebServiceQueryCache.TabIndex = 6;
            this.btnClearWebServiceQueryCache.Text = "Clear";
            this.btnClearWebServiceQueryCache.UseVisualStyleBackColor = true;
            this.btnClearWebServiceQueryCache.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // ddWebServiceQueryCacheServer
            // 
            this.ddWebServiceQueryCacheServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddWebServiceQueryCacheServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddWebServiceQueryCacheServer.FormattingEnabled = true;
            this.ddWebServiceQueryCacheServer.Location = new System.Drawing.Point(198, 22);
            this.ddWebServiceQueryCacheServer.Name = "ddWebServiceQueryCacheServer";
            this.ddWebServiceQueryCacheServer.Size = new System.Drawing.Size(765, 21);
            this.ddWebServiceQueryCacheServer.TabIndex = 4;
            this.ddWebServiceQueryCacheServer.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.btnClearRAWServer);
            this.groupBox6.Controls.Add(this.ddOverrideRawServer);
            this.groupBox6.Location = new System.Drawing.Point(12, 297);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1126, 62);
            this.groupBox6.TabIndex = 16;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Override Data Loading RAW Server (Defaults to \'localhost\' if no override is confi" +
    "gured - \'Database\' can be left blank as it is ignored, only the Server Name and " +
    "Credentials are used)";
            // 
            // btnClearRAWServer
            // 
            this.btnClearRAWServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearRAWServer.Location = new System.Drawing.Point(969, 20);
            this.btnClearRAWServer.Name = "btnClearRAWServer";
            this.btnClearRAWServer.Size = new System.Drawing.Size(65, 23);
            this.btnClearRAWServer.TabIndex = 6;
            this.btnClearRAWServer.Text = "Clear";
            this.btnClearRAWServer.UseVisualStyleBackColor = true;
            this.btnClearRAWServer.Click += new System.EventHandler(this.btnClearServer_Click);
            // 
            // ddOverrideRawServer
            // 
            this.ddOverrideRawServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddOverrideRawServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOverrideRawServer.FormattingEnabled = true;
            this.ddOverrideRawServer.Location = new System.Drawing.Point(10, 22);
            this.ddOverrideRawServer.Name = "ddOverrideRawServer";
            this.ddOverrideRawServer.Size = new System.Drawing.Size(953, 21);
            this.ddOverrideRawServer.TabIndex = 4;
            this.ddOverrideRawServer.SelectedIndexChanged += new System.EventHandler(this.ddDefault_SelectedIndexChanged);
            // 
            // ServerDefaultsUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1148, 366);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Name = "ServerDefaultsUI";
            this.Text = "ManageExternalServers";
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox ddDefaultIdentifierDump;
        private System.Windows.Forms.ComboBox ddDefaultLoggingServer;
        private System.Windows.Forms.Button btnClearLoggingServer;
        private System.Windows.Forms.Button btnClearIdentifierDump;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnCreateNewDQEServer;
        private System.Windows.Forms.Button btnClearDQEServer;
        private System.Windows.Forms.ComboBox ddDQEServer;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnCreateNewWebServiceQueryCache;
        private System.Windows.Forms.Button btnClearWebServiceQueryCache;
        private System.Windows.Forms.ComboBox ddWebServiceQueryCacheServer;
        private System.Windows.Forms.Button btnCreateNewIdentifierDump;
        private System.Windows.Forms.Button btnCreateNewLoggingServer;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button btnClearRAWServer;
        private System.Windows.Forms.ComboBox ddOverrideRawServer;
        private System.Windows.Forms.Button btnCreateNewANOStore;
        private System.Windows.Forms.Button btnClearANOStore;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox ddDefaultANOStore;
        private System.Windows.Forms.Button btnCreateNewCohortIdentificationQueryCache;
        private System.Windows.Forms.Button btnClearCohortIdentificationQueryCache;
        private System.Windows.Forms.ComboBox ddCohortIdentificationQueryCacheServer;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label9;
    }
}