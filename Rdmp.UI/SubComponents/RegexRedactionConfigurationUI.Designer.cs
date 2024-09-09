namespace Rdmp.UI.SubComponents;

partial class RegexRedactionConfigurationUI
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
        label1 = new System.Windows.Forms.Label();
        tbName = new System.Windows.Forms.TextBox();
        columnButtonRenderer1 = new BrightIdeasSoftware.ColumnButtonRenderer();
        tbRegexPattern = new System.Windows.Forms.TextBox();
        label2 = new System.Windows.Forms.Label();
        tbRedactionString = new System.Windows.Forms.TextBox();
        label3 = new System.Windows.Forms.Label();
        tbDescription = new System.Windows.Forms.TextBox();
        label4 = new System.Windows.Forms.Label();
        label5 = new System.Windows.Forms.Label();
        tbFolder = new System.Windows.Forms.TextBox();
        label6 = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(80, 63);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(47, 15);
        label1.TabIndex = 0;
        label1.Text = "Name *";
        // 
        // tbName
        // 
        tbName.Location = new System.Drawing.Point(80, 81);
        tbName.Name = "tbName";
        tbName.Size = new System.Drawing.Size(320, 23);
        tbName.TabIndex = 1;
        // 
        // columnButtonRenderer1
        // 
        columnButtonRenderer1.ButtonPadding = new System.Drawing.Size(10, 10);
        // 
        // tbRegexPattern
        // 
        tbRegexPattern.Location = new System.Drawing.Point(80, 134);
        tbRegexPattern.Name = "tbRegexPattern";
        tbRegexPattern.Size = new System.Drawing.Size(320, 23);
        tbRegexPattern.TabIndex = 4;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(80, 116);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(88, 15);
        label2.TabIndex = 3;
        label2.Text = "Regex Pattern *";
        // 
        // tbRedactionString
        // 
        tbRedactionString.Location = new System.Drawing.Point(80, 193);
        tbRedactionString.Name = "tbRedactionString";
        tbRedactionString.Size = new System.Drawing.Size(320, 23);
        tbRedactionString.TabIndex = 6;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(80, 175);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(118, 15);
        label3.TabIndex = 5;
        label3.Text = "Replacement String *";
        // 
        // tbDescription
        // 
        tbDescription.Location = new System.Drawing.Point(80, 244);
        tbDescription.Name = "tbDescription";
        tbDescription.Size = new System.Drawing.Size(320, 23);
        tbDescription.TabIndex = 8;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(80, 226);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(67, 15);
        label4.TabIndex = 7;
        label4.Text = "Description";
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new System.Drawing.Point(338, 333);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(62, 15);
        label5.TabIndex = 9;
        label5.Text = "* Required";
        label5.Click += label5_Click;
        // 
        // tbFolder
        // 
        tbFolder.Location = new System.Drawing.Point(80, 298);
        tbFolder.Name = "tbFolder";
        tbFolder.Size = new System.Drawing.Size(320, 23);
        tbFolder.TabIndex = 11;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new System.Drawing.Point(80, 280);
        label6.Name = "label6";
        label6.Size = new System.Drawing.Size(51, 15);
        label6.TabIndex = 10;
        label6.Text = "Folder * ";
        // 
        // RegexRedactionConfigurationUI
        // 
        Controls.Add(tbFolder);
        Controls.Add(label6);
        Controls.Add(label5);
        Controls.Add(tbDescription);
        Controls.Add(label4);
        Controls.Add(tbRedactionString);
        Controls.Add(label3);
        Controls.Add(tbRegexPattern);
        Controls.Add(label2);
        Controls.Add(tbName);
        Controls.Add(label1);
        Name = "RegexRedactionConfigurationUI";
        Size = new System.Drawing.Size(800, 450);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbName;
    private BrightIdeasSoftware.ColumnButtonRenderer columnButtonRenderer1;
    private System.Windows.Forms.TextBox tbRegexPattern;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox tbRedactionString;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox tbDescription;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox tbFolder;
    private System.Windows.Forms.Label label6;
}
