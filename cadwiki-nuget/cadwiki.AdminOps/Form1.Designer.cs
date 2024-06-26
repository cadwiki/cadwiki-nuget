using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace cadwiki.AdminOps
{
    public partial class Form1 : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            Button1 = new Button();
            Button1.Click += new EventHandler(Button1_Click);
            LabelCurrentVersion = new Label();
            TextBoxNewVersion = new TextBox();
            SuspendLayout();
            // 
            // Button1
            // 
            Button1.Location = new Point(23, 48);
            Button1.Name = "Button1";
            Button1.Size = new Size(154, 23);
            Button1.TabIndex = 0;
            Button1.Text = "Update version";
            Button1.UseVisualStyleBackColor = true;
            // 
            // LabelCurrentVersion
            // 
            LabelCurrentVersion.AutoSize = true;
            LabelCurrentVersion.Location = new Point(228, 52);
            LabelCurrentVersion.Name = "LabelCurrentVersion";
            LabelCurrentVersion.Size = new Size(48, 16);
            LabelCurrentVersion.TabIndex = 1;
            LabelCurrentVersion.Text = "Label1";
            // 
            // TextBoxNewVersion
            // 
            TextBoxNewVersion.Location = new Point(318, 51);
            TextBoxNewVersion.Name = "TextBoxNewVersion";
            TextBoxNewVersion.Size = new Size(118, 22);
            TextBoxNewVersion.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8.0f, 16.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TextBoxNewVersion);
            Controls.Add(LabelCurrentVersion);
            Controls.Add(Button1);
            Name = "Form1";
            Text = "Form1";
            Load += new EventHandler(Form1_Load);
            ResumeLayout(false);
            PerformLayout();

        }

        internal Button Button1;
        internal Label LabelCurrentVersion;
        internal TextBox TextBoxNewVersion;
    }
}