using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.Refreshing;
using ReusableUIComponents;

namespace CatalogueManager.ExtractionUIs.FilterUIs
{
    partial class ExtractionFilterUI : ILifetimeSubscriber
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checksUIIconOnly1 = new ReusableUIComponents.ChecksUI.ChecksUIIconOnly();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbxInsertColumnName = new ReusableUIComponents.SuggestComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbIsMandatory = new System.Windows.Forms.CheckBox();
            this.tbFilterName = new System.Windows.Forms.TextBox();
            this.btnPublishToCatalogue = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFilterDescription = new System.Windows.Forms.TextBox();
            this.lblWhere = new System.Windows.Forms.Label();
            this.pQueryEditor = new System.Windows.Forms.Panel();
            this.parameterCollectionUI1 = new CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.ParameterCollectionUI();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnParametersExpand = new System.Windows.Forms.Button();
            this.lblParams = new System.Windows.Forms.Label();
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.checksUIIconOnly1);
            this.groupBox2.Location = new System.Drawing.Point(4, 776);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(60, 50);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Checks";
            // 
            // checksUIIconOnly1
            // 
            this.checksUIIconOnly1.Location = new System.Drawing.Point(12, 15);
            this.checksUIIconOnly1.Name = "checksUIIconOnly1";
            this.checksUIIconOnly1.Size = new System.Drawing.Size(32, 32);
            this.checksUIIconOnly1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbxInsertColumnName);
            this.groupBox1.Location = new System.Drawing.Point(166, 798);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(900, 38);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Insert Column Name";
            this.groupBox1.Visible = false;
            // 
            // cbxInsertColumnName
            // 
            this.cbxInsertColumnName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbxInsertColumnName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbxInsertColumnName.FilterRule = null;
            this.cbxInsertColumnName.FormattingEnabled = true;
            this.cbxInsertColumnName.Location = new System.Drawing.Point(3, 16);
            this.cbxInsertColumnName.Name = "cbxInsertColumnName";
            this.cbxInsertColumnName.PropertySelector = null;
            this.cbxInsertColumnName.Size = new System.Drawing.Size(894, 21);
            this.cbxInsertColumnName.SuggestBoxHeight = 96;
            this.cbxInsertColumnName.SuggestListOrderRule = null;
            this.cbxInsertColumnName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter Name:";
            // 
            // cbIsMandatory
            // 
            this.cbIsMandatory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cbIsMandatory.AutoSize = true;
            this.cbIsMandatory.Location = new System.Drawing.Point(76, 819);
            this.cbIsMandatory.Name = "cbIsMandatory";
            this.cbIsMandatory.Size = new System.Drawing.Size(84, 17);
            this.cbIsMandatory.TabIndex = 9;
            this.cbIsMandatory.Text = "IsMandatory";
            this.cbIsMandatory.UseVisualStyleBackColor = true;
            this.cbIsMandatory.CheckedChanged += new System.EventHandler(this.cbIsMandatory_CheckedChanged);
            // 
            // tbFilterName
            // 
            this.tbFilterName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterName.Location = new System.Drawing.Point(76, 5);
            this.tbFilterName.Name = "tbFilterName";
            this.tbFilterName.Size = new System.Drawing.Size(868, 20);
            this.tbFilterName.TabIndex = 1;
            this.tbFilterName.TextChanged += new System.EventHandler(this.tbFilterName_TextChanged);
            // 
            // btnPublishToCatalogue
            // 
            this.btnPublishToCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPublishToCatalogue.Enabled = false;
            this.btnPublishToCatalogue.Location = new System.Drawing.Point(1072, 819);
            this.btnPublishToCatalogue.Name = "btnPublishToCatalogue";
            this.btnPublishToCatalogue.Size = new System.Drawing.Size(118, 23);
            this.btnPublishToCatalogue.TabIndex = 11;
            this.btnPublishToCatalogue.Text = "Publish To Catalogue";
            this.btnPublishToCatalogue.UseVisualStyleBackColor = true;
            this.btnPublishToCatalogue.Click += new System.EventHandler(this.btnPublishToCatalogue_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Description:";
            // 
            // tbFilterDescription
            // 
            this.tbFilterDescription.AcceptsReturn = true;
            this.tbFilterDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterDescription.Location = new System.Drawing.Point(76, 28);
            this.tbFilterDescription.Multiline = true;
            this.tbFilterDescription.Name = "tbFilterDescription";
            this.tbFilterDescription.Size = new System.Drawing.Size(1195, 96);
            this.tbFilterDescription.TabIndex = 3;
            this.tbFilterDescription.TextChanged += new System.EventHandler(this.tbFilterDescription_TextChanged);
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWhere.ForeColor = System.Drawing.Color.Blue;
            this.lblWhere.Location = new System.Drawing.Point(13, 130);
            this.lblWhere.Name = "lblWhere";
            this.lblWhere.Size = new System.Drawing.Size(53, 13);
            this.lblWhere.TabIndex = 5;
            this.lblWhere.Text = "WHERE";
            // 
            // pQueryEditor
            // 
            this.pQueryEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pQueryEditor.Location = new System.Drawing.Point(0, 0);
            this.pQueryEditor.Name = "pQueryEditor";
            this.pQueryEditor.Size = new System.Drawing.Size(1195, 290);
            this.pQueryEditor.TabIndex = 0;
            // 
            // parameterCollectionUI1
            // 
            this.parameterCollectionUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterCollectionUI1.Location = new System.Drawing.Point(0, 0);
            this.parameterCollectionUI1.Name = "parameterCollectionUI1";
            this.parameterCollectionUI1.Size = new System.Drawing.Size(1195, 368);
            this.parameterCollectionUI1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(79, 130);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pQueryEditor);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.parameterCollectionUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1195, 662);
            this.splitContainer1.SplitterDistance = 290;
            this.splitContainer1.TabIndex = 26;
            // 
            // btnParametersExpand
            // 
            this.btnParametersExpand.Location = new System.Drawing.Point(53, 423);
            this.btnParametersExpand.Name = "btnParametersExpand";
            this.btnParametersExpand.Size = new System.Drawing.Size(17, 23);
            this.btnParametersExpand.TabIndex = 7;
            this.btnParametersExpand.Text = "-";
            this.btnParametersExpand.UseVisualStyleBackColor = true;
            this.btnParametersExpand.Click += new System.EventHandler(this.btnParametersExpand_Click);
            // 
            // lblParams
            // 
            this.lblParams.AutoSize = true;
            this.lblParams.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblParams.ForeColor = System.Drawing.Color.Blue;
            this.lblParams.Location = new System.Drawing.Point(0, 428);
            this.lblParams.Name = "lblParams";
            this.lblParams.Size = new System.Drawing.Size(48, 13);
            this.lblParams.TabIndex = 6;
            this.lblParams.Text = "Params";
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(0, 102);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(75, 23);
            this.objectSaverButton1.TabIndex = 4;
            this.objectSaverButton1.Text = "objectSaverButton1";
            // 
            // ExtractionFilterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.btnParametersExpand);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbIsMandatory);
            this.Controls.Add(this.tbFilterName);
            this.Controls.Add(this.btnPublishToCatalogue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbFilterDescription);
            this.Controls.Add(this.lblParams);
            this.Controls.Add(this.lblWhere);
            this.Name = "ExtractionFilterUI";
            this.Size = new System.Drawing.Size(1274, 849);
            this.SizeChanged += new System.EventHandler(this.ExtractionFilterUI_SizeChanged);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox2;
        private ReusableUIComponents.ChecksUI.ChecksUIIconOnly checksUIIconOnly1;
        private System.Windows.Forms.GroupBox groupBox1;
        private SuggestComboBox cbxInsertColumnName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbIsMandatory;
        private System.Windows.Forms.TextBox tbFilterName;
        private System.Windows.Forms.Button btnPublishToCatalogue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFilterDescription;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.Panel pQueryEditor;
        private ParameterCollectionUI parameterCollectionUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnParametersExpand;
        private System.Windows.Forms.Label lblParams;
        private SimpleControls.ObjectSaverButton objectSaverButton1;

    }
}
