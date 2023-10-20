using Org.BouncyCastle.Asn1.Crmf;
using Rdmp.UI.ChecksUI;
using Rdmp.UI.SimpleControls;
using ScintillaNET;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Rdmp.UI.CohortUI.CreateHoldoutLookup;

partial class CreateHoldoutLookupUI
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
        gbNewCohort = new GroupBox();
        label7 = new Label();
        tbName = new TextBox();
        gbChooseCohortType = new GroupBox();
        label4 = new Label();
        textBox4 = new TextBox();
        label3 = new Label();
        textBox3 = new TextBox();
        label2 = new Label();
        textBox2 = new TextBox();
        label1 = new Label();
        comboBox1 = new ComboBox();
        numericUpDown1 = new NumericUpDown();
        groupBox3 = new GroupBox();
        label9 = new Label();
        tbDescription = new TextBox();
        btnClearProject = new Button();
        btnOk = new Button();
        ragSmiley1 = new RAGSmiley();
        gbDescription = new GroupBox();
        taskDescriptionLabel1 = new SimpleDialogs.TaskDescriptionLabel();
        panel1 = new Panel();
        panel2 = new Panel();
        gbNewCohort.SuspendLayout();
        gbChooseCohortType.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
        groupBox3.SuspendLayout();
        gbDescription.SuspendLayout();
        panel1.SuspendLayout();
        panel2.SuspendLayout();
        SuspendLayout();
        // 
        // gbNewCohort
        // 
        gbNewCohort.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gbNewCohort.Controls.Add(label7);
        gbNewCohort.Controls.Add(tbName);
        gbNewCohort.Location = new System.Drawing.Point(13, 24);
        gbNewCohort.Margin = new Padding(4, 3, 4, 3);
        gbNewCohort.Name = "gbNewCohort";
        gbNewCohort.Padding = new Padding(4, 3, 4, 3);
        gbNewCohort.Size = new System.Drawing.Size(1005, 54);
        gbNewCohort.TabIndex = 0;
        gbNewCohort.TabStop = false;
        gbNewCohort.Text = "Holdout Cohort Name";
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new System.Drawing.Point(7, 25);
        label7.Margin = new Padding(4, 0, 4, 0);
        label7.Name = "label7";
        label7.Size = new System.Drawing.Size(42, 15);
        label7.TabIndex = 0;
        label7.Text = "Name:";
        // 
        // tbName
        // 
        tbName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tbName.Location = new System.Drawing.Point(58, 22);
        tbName.Margin = new Padding(4, 3, 4, 3);
        tbName.Name = "tbName";
        tbName.Size = new System.Drawing.Size(939, 23);
        tbName.TabIndex = 1;
        tbName.TextChanged += tbName_TextChanged;
        // 
        // gbChooseCohortType
        // 
        gbChooseCohortType.Controls.Add(label4);
        gbChooseCohortType.Controls.Add(textBox4);
        gbChooseCohortType.Controls.Add(label3);
        gbChooseCohortType.Controls.Add(textBox3);
        gbChooseCohortType.Controls.Add(label2);
        gbChooseCohortType.Controls.Add(textBox2);
        gbChooseCohortType.Controls.Add(label1);
        gbChooseCohortType.Controls.Add(comboBox1);
        gbChooseCohortType.Controls.Add(numericUpDown1);
        gbChooseCohortType.Dock = DockStyle.Top;
        gbChooseCohortType.Location = new System.Drawing.Point(0, 0);
        gbChooseCohortType.Margin = new Padding(4, 3, 4, 3);
        gbChooseCohortType.Name = "gbChooseCohortType";
        gbChooseCohortType.Padding = new Padding(4, 3, 4, 3);
        gbChooseCohortType.Size = new System.Drawing.Size(1048, 107);
        gbChooseCohortType.TabIndex = 9;
        gbChooseCohortType.TabStop = false;
        gbChooseCohortType.Text = "1. Define Holdout settings";
        gbChooseCohortType.Enter += gbChooseCohortType_Enter;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(444, 69);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(77, 15);
        label4.TabIndex = 11;
        label4.Text = "Date Column";
        label4.Click += label4_Click;
        // 
        // textBox4
        // 
        textBox4.Location = new System.Drawing.Point(527, 66);
        textBox4.Name = "textBox4";
        textBox4.Size = new System.Drawing.Size(100, 23);
        textBox4.TabIndex = 10;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(229, 61);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(91, 30);
        label3.TabIndex = 9;
        label3.Text = "Max Date\r\n(DD/MM/YYYY)";
        label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        label3.Click += label3_Click;
        // 
        // textBox3
        // 
        textBox3.Location = new System.Drawing.Point(326, 66);
        textBox3.Name = "textBox3";
        textBox3.Size = new System.Drawing.Size(100, 23);
        textBox3.TabIndex = 8;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(16, 61);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(91, 30);
        label2.TabIndex = 7;
        label2.Text = "Min Date\r\n(DD/MM/YYYY)";
        label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        label2.Click += label2_Click;
        // 
        // textBox2
        // 
        textBox2.Location = new System.Drawing.Point(113, 66);
        textBox2.Name = "textBox2";
        textBox2.Size = new System.Drawing.Size(100, 23);
        textBox2.TabIndex = 6;
        textBox2.TextChanged += textBox2_TextChanged;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(171, 20);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(110, 15);
        label1.TabIndex = 5;
        label1.Text = "of people in Cohort";
        label1.Click += label1_Click;
        // 
        // comboBox1
        // 
        comboBox1.AllowDrop = true;
        comboBox1.FormattingEnabled = true;
        comboBox1.Items.AddRange(new object[] { "%", "#" });
        comboBox1.Location = new System.Drawing.Point(122, 17);
        comboBox1.Name = "comboBox1";
        comboBox1.Size = new System.Drawing.Size(43, 23);
        comboBox1.TabIndex = 4;
        // 
        // numericUpDown1
        // 
        numericUpDown1.Location = new System.Drawing.Point(20, 18);
        numericUpDown1.Name = "numericUpDown1";
        numericUpDown1.Size = new System.Drawing.Size(87, 23);
        numericUpDown1.TabIndex = 3;
        // 
        // groupBox3
        // 
        groupBox3.Controls.Add(gbNewCohort);
        groupBox3.Dock = DockStyle.Top;
        groupBox3.Location = new System.Drawing.Point(0, 107);
        groupBox3.Margin = new Padding(4, 3, 4, 3);
        groupBox3.Name = "groupBox3";
        groupBox3.Padding = new Padding(4, 3, 4, 3);
        groupBox3.Size = new System.Drawing.Size(1048, 90);
        groupBox3.TabIndex = 10;
        groupBox3.TabStop = false;
        groupBox3.Text = "2. Configure Cohort (doesn't exist yet, next screen will actually create it)";
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new System.Drawing.Point(7, 24);
        label9.Margin = new Padding(4, 0, 4, 0);
        label9.Name = "label9";
        label9.Size = new System.Drawing.Size(64, 15);
        label9.TabIndex = 2;
        label9.Text = "Comment:";
        // 
        // tbDescription
        // 
        tbDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tbDescription.Location = new System.Drawing.Point(71, 24);
        tbDescription.Margin = new Padding(4, 3, 4, 3);
        tbDescription.Multiline = true;
        tbDescription.Name = "tbDescription";
        tbDescription.Size = new System.Drawing.Size(969, 67);
        tbDescription.TabIndex = 3;
        tbDescription.TextChanged += tbDescription_TextChanged;
        // 
        // btnClearProject
        // 
        btnClearProject.Anchor = AnchorStyles.Top;
        btnClearProject.Location = new System.Drawing.Point(528, 7);
        btnClearProject.Margin = new Padding(4, 3, 4, 3);
        btnClearProject.Name = "btnClearProject";
        btnClearProject.Size = new System.Drawing.Size(141, 27);
        btnClearProject.TabIndex = 14;
        btnClearProject.Text = "Cancel";
        btnClearProject.UseVisualStyleBackColor = true;
        btnClearProject.Click += btnCancel_Click;
        // 
        // btnOk
        // 
        btnOk.Anchor = AnchorStyles.Top;
        btnOk.Location = new System.Drawing.Point(372, 7);
        btnOk.Margin = new Padding(4, 3, 4, 3);
        btnOk.Name = "btnOk";
        btnOk.Size = new System.Drawing.Size(149, 27);
        btnOk.TabIndex = 13;
        btnOk.Text = "Ok";
        btnOk.UseVisualStyleBackColor = true;
        btnOk.Click += btnOk_Click;
        // 
        // ragSmiley1
        // 
        ragSmiley1.AlwaysShowHandCursor = false;
        ragSmiley1.Anchor = AnchorStyles.Top;
        ragSmiley1.BackColor = System.Drawing.Color.Transparent;
        ragSmiley1.Location = new System.Drawing.Point(335, 4);
        ragSmiley1.Margin = new Padding(5, 3, 5, 3);
        ragSmiley1.Name = "ragSmiley1";
        ragSmiley1.Size = new System.Drawing.Size(30, 30);
        ragSmiley1.TabIndex = 12;
        // 
        // gbDescription
        // 
        gbDescription.Controls.Add(label9);
        gbDescription.Controls.Add(tbDescription);
        gbDescription.Dock = DockStyle.Top;
        gbDescription.Location = new System.Drawing.Point(0, 197);
        gbDescription.Margin = new Padding(4, 3, 4, 3);
        gbDescription.Name = "gbDescription";
        gbDescription.Padding = new Padding(4, 3, 4, 3);
        gbDescription.Size = new System.Drawing.Size(1048, 99);
        gbDescription.TabIndex = 11;
        gbDescription.TabStop = false;
        gbDescription.Text = "3. Enter Description Of Holdout";
        // 
        // taskDescriptionLabel1
        // 
        taskDescriptionLabel1.AutoSize = true;
        taskDescriptionLabel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        taskDescriptionLabel1.Dock = DockStyle.Top;
        taskDescriptionLabel1.Location = new System.Drawing.Point(0, 0);
        taskDescriptionLabel1.Name = "taskDescriptionLabel1";
        taskDescriptionLabel1.Size = new System.Drawing.Size(1048, 42);
        taskDescriptionLabel1.TabIndex = 19;
        // 
        // panel1
        // 
        panel1.Controls.Add(panel2);
        panel1.Controls.Add(gbDescription);
        panel1.Controls.Add(groupBox3);
        panel1.Controls.Add(gbChooseCohortType);
        panel1.Dock = DockStyle.Fill;
        panel1.Location = new System.Drawing.Point(0, 42);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(1048, 509);
        panel1.TabIndex = 20;
        // 
        // panel2
        // 
        panel2.Controls.Add(btnOk);
        panel2.Controls.Add(ragSmiley1);
        panel2.Controls.Add(btnClearProject);
        panel2.Dock = DockStyle.Top;
        panel2.Location = new System.Drawing.Point(0, 296);
        panel2.Name = "panel2";
        panel2.Size = new System.Drawing.Size(1048, 55);
        panel2.TabIndex = 20;
        // 
        // CreateHoldoutLookupUI
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1048, 551);
        Controls.Add(panel1);
        Controls.Add(taskDescriptionLabel1);
        Margin = new Padding(4, 3, 4, 3);
        Name = "CreateHoldoutLookupUI";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Create Cohort";
        Load += CohortHoldoutCreationRequestUI_Load;
        gbNewCohort.ResumeLayout(false);
        gbNewCohort.PerformLayout();
        gbChooseCohortType.ResumeLayout(false);
        gbChooseCohortType.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
        groupBox3.ResumeLayout(false);
        gbDescription.ResumeLayout(false);
        gbDescription.PerformLayout();
        panel1.ResumeLayout(false);
        panel2.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    //private System.Windows.Forms.Label label3;
    //private System.Windows.Forms.Label lblExternalCohortTable;
    private GroupBox gbNewCohort;
    private Label label7;
    private TextBox tbName;
    //private System.Windows.Forms.Button btnNewProject;
    private GroupBox gbChooseCohortType;
    private GroupBox groupBox3;
    private Label label9;
    private TextBox tbDescription;
    //private System.Windows.Forms.Label lblProject;
    //private System.Windows.Forms.Label label11;
    private Button btnClearProject;
    private Button btnOk;
    private RAGSmiley ragSmiley1;
    private GroupBox gbDescription;
    //private System.Windows.Forms.PictureBox pbProject;
    //private System.Windows.Forms.PictureBox pbCohortSource;
    //private System.Windows.Forms.Button btnExisting;
    //private System.Windows.Forms.Label lblErrorNoProjectNumber;
    //private System.Windows.Forms.TextBox tbSetProjectNumber;
    //private System.Windows.Forms.Button btnClear;
    private SimpleDialogs.TaskDescriptionLabel taskDescriptionLabel1;
    private Panel panel1;
    private Panel panel2;
    //private System.Windows.Forms.CheckBox checkBox1;
    private Label label1;
    private ComboBox comboBox1;
    private NumericUpDown numericUpDown1;
    private Label label2;
    private TextBox textBox2;
    private Label label3;
    private TextBox textBox3;
    private Label label4;
    private TextBox textBox4;
}
