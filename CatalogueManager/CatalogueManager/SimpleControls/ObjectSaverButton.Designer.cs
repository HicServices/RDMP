namespace CatalogueManager.SimpleControls
{
    partial class ObjectSaverButton
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


            if (_refreshBus != null)
                _refreshBus.Unsubscribe(this);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectSaverButton));
            this.btnSave = new System.Windows.Forms.Button();
            this.btnViewDifferences = new System.Windows.Forms.Button();
            this.btnDiscard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(0, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(27, 27);
            this.btnSave.TabIndex = 0;
            this.btnSave.Tag = "";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnViewDifferences
            // 
            this.btnViewDifferences.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDifferences.Image")));
            this.btnViewDifferences.Location = new System.Drawing.Point(33, 0);
            this.btnViewDifferences.Name = "btnViewDifferences";
            this.btnViewDifferences.Size = new System.Drawing.Size(27, 27);
            this.btnViewDifferences.TabIndex = 0;
            this.btnViewDifferences.Tag = "";
            this.btnViewDifferences.UseVisualStyleBackColor = true;
            this.btnViewDifferences.Click += new System.EventHandler(this.btnViewDifferences_Click);
            // 
            // btnDiscard
            // 
            this.btnDiscard.Image = ((System.Drawing.Image)(resources.GetObject("btnDiscard.Image")));
            this.btnDiscard.Location = new System.Drawing.Point(66, 0);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.Size = new System.Drawing.Size(27, 27);
            this.btnDiscard.TabIndex = 0;
            this.btnDiscard.Tag = "";
            this.btnDiscard.UseVisualStyleBackColor = true;
            this.btnDiscard.Click += new System.EventHandler(this.btnDiscard_Click);
            // 
            // ObjectSaverButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnDiscard);
            this.Controls.Add(this.btnViewDifferences);
            this.Controls.Add(this.btnSave);
            this.Name = "ObjectSaverButton";
            this.Size = new System.Drawing.Size(96, 30);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnViewDifferences;
        private System.Windows.Forms.Button btnDiscard;
    }
}
