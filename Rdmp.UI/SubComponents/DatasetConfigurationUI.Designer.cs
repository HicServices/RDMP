namespace Rdmp.UI.SubComponents;

partial class DatasetConfigurationUI
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
        tbDOI = new System.Windows.Forms.TextBox();
        tbSource = new System.Windows.Forms.TextBox();
        label2 = new System.Windows.Forms.Label();
        label3 = new System.Windows.Forms.Label();
        lblDatasetUsage = new System.Windows.Forms.Label();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(24, 40);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(86, 15);
        label1.TabIndex = 0;
        label1.Text = "Dataset Name*";
        label1.Click += label1_Click;
        // 
        // tbName
        // 
        tbName.Location = new System.Drawing.Point(27, 65);
        tbName.Name = "tbName";
        tbName.Size = new System.Drawing.Size(270, 23);
        tbName.TabIndex = 1;
        // 
        // tbDOI
        // 
        tbDOI.Location = new System.Drawing.Point(24, 155);
        tbDOI.Name = "tbDOI";
        tbDOI.Size = new System.Drawing.Size(273, 23);
        tbDOI.TabIndex = 2;
        // 
        // tbSource
        // 
        tbSource.Location = new System.Drawing.Point(24, 235);
        tbSource.Name = "tbSource";
        tbSource.Size = new System.Drawing.Size(273, 23);
        tbSource.TabIndex = 3;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(24, 123);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(69, 15);
        label2.TabIndex = 4;
        label2.Text = "Dataset DOI";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(28, 202);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(85, 15);
        label3.TabIndex = 5;
        label3.Text = "Dataset Source";
        // 
        // lblDatasetUsage
        // 
        lblDatasetUsage.AutoSize = true;
        lblDatasetUsage.Location = new System.Drawing.Point(369, 65);
        lblDatasetUsage.Name = "lblDatasetUsage";
        lblDatasetUsage.Size = new System.Drawing.Size(38, 15);
        lblDatasetUsage.TabIndex = 8;
        lblDatasetUsage.Text = "label5";
        // 
        // DatasetConfigurationUI
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(lblDatasetUsage);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(tbSource);
        Controls.Add(tbDOI);
        Controls.Add(tbName);
        Controls.Add(label1);
        Name = "DatasetConfigurationUI";
        Size = new System.Drawing.Size(800, 450);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox tbName;
    private System.Windows.Forms.TextBox tbDOI;
    private System.Windows.Forms.TextBox tbSource;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblDatasetUsage;
}
