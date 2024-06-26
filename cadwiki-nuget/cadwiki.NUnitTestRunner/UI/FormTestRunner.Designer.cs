using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace cadwiki.NUnitTestRunner.UI
{
    public partial class FormTestRunner : System.Windows.Forms.Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
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
            _ButtonOk = new System.Windows.Forms.Button();
            _ButtonOk.Click += new EventHandler(ButtonOk_Click);
            _Cancel = new System.Windows.Forms.Button();
            _Cancel.Click += new EventHandler(Cancel_Click);
            _GroupBox1 = new System.Windows.Forms.GroupBox();
            _TreeViewResults = new System.Windows.Forms.TreeView();
            _GroupBox2 = new System.Windows.Forms.GroupBox();
            _RichTextBoxConsole = new System.Windows.Forms.RichTextBox();
            _ButtonOpenTestEvidenceFolder = new System.Windows.Forms.Button();
            _ButtonOpenTestEvidenceFolder.Click += new EventHandler(ButtonOpenTestEvidenceFolder_Click);
            _GroupBox1.SuspendLayout();
            _GroupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonOk
            // 
            _ButtonOk.Location = new System.Drawing.Point(848, 173);
            _ButtonOk.Name = "_ButtonOk";
            _ButtonOk.Size = new System.Drawing.Size(75, 23);
            _ButtonOk.TabIndex = 0;
            _ButtonOk.Text = "Ok";
            _ButtonOk.UseVisualStyleBackColor = true;
            // 
            // Cancel
            // 
            _Cancel.Location = new System.Drawing.Point(218, 173);
            _Cancel.Name = "_Cancel";
            _Cancel.Size = new System.Drawing.Size(75, 23);
            _Cancel.TabIndex = 1;
            _Cancel.Text = "Cancel";
            _Cancel.UseVisualStyleBackColor = true;
            // 
            // GroupBox1
            // 
            _GroupBox1.Controls.Add(_TreeViewResults);
            _GroupBox1.Location = new System.Drawing.Point(12, 12);
            _GroupBox1.Name = "_GroupBox1";
            _GroupBox1.Size = new System.Drawing.Size(580, 155);
            _GroupBox1.TabIndex = 2;
            _GroupBox1.TabStop = false;
            _GroupBox1.Text = "TestResults";
            // 
            // TreeViewResults
            // 
            _TreeViewResults.Location = new System.Drawing.Point(6, 21);
            _TreeViewResults.Name = "_TreeViewResults";
            _TreeViewResults.Size = new System.Drawing.Size(566, 128);
            _TreeViewResults.TabIndex = 0;
            // 
            // GroupBox2
            // 
            _GroupBox2.Controls.Add(_RichTextBoxConsole);
            _GroupBox2.Location = new System.Drawing.Point(590, 12);
            _GroupBox2.Name = "_GroupBox2";
            _GroupBox2.Size = new System.Drawing.Size(580, 155);
            _GroupBox2.TabIndex = 3;
            _GroupBox2.TabStop = false;
            _GroupBox2.Text = "Log";
            // 
            // RichTextBoxConsole
            // 
            _RichTextBoxConsole.Location = new System.Drawing.Point(8, 22);
            _RichTextBoxConsole.Name = "_RichTextBoxConsole";
            _RichTextBoxConsole.Size = new System.Drawing.Size(566, 127);
            _RichTextBoxConsole.TabIndex = 0;
            _RichTextBoxConsole.Text = "";
            // 
            // ButtonOpenTestEvidenceFolder
            // 
            _ButtonOpenTestEvidenceFolder.Location = new System.Drawing.Point(958, 173);
            _ButtonOpenTestEvidenceFolder.Name = "_ButtonOpenTestEvidenceFolder";
            _ButtonOpenTestEvidenceFolder.Size = new System.Drawing.Size(80, 23);
            _ButtonOpenTestEvidenceFolder.TabIndex = 4;
            _ButtonOpenTestEvidenceFolder.Text = "Evidence";
            _ButtonOpenTestEvidenceFolder.UseVisualStyleBackColor = true;
            // 
            // FormTestRunner
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8.0f, 16.0f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1182, 203);
            Controls.Add(_ButtonOpenTestEvidenceFolder);
            Controls.Add(_GroupBox2);
            Controls.Add(_GroupBox1);
            Controls.Add(_Cancel);
            Controls.Add(_ButtonOk);
            Name = "FormTestRunner";
            Text = "FormTestRunner";
            _GroupBox1.ResumeLayout(false);
            _GroupBox2.ResumeLayout(false);
            ResumeLayout(false);

        }

        private System.Windows.Forms.Button _ButtonOk;

        internal virtual System.Windows.Forms.Button ButtonOk
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonOk;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonOk != null)
                {
                    _ButtonOk.Click -= ButtonOk_Click;
                }

                _ButtonOk = value;
                if (_ButtonOk != null)
                {
                    _ButtonOk.Click += ButtonOk_Click;
                }
            }
        }
        private System.Windows.Forms.Button _Cancel;

        internal virtual System.Windows.Forms.Button Cancel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Cancel;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Cancel != null)
                {
                    _Cancel.Click -= Cancel_Click;
                }

                _Cancel = value;
                if (_Cancel != null)
                {
                    _Cancel.Click += Cancel_Click;
                }
            }
        }
        private System.Windows.Forms.GroupBox _GroupBox1;

        internal virtual System.Windows.Forms.GroupBox GroupBox1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _GroupBox1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _GroupBox1 = value;
            }
        }
        private System.Windows.Forms.GroupBox _GroupBox2;

        internal virtual System.Windows.Forms.GroupBox GroupBox2
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _GroupBox2;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _GroupBox2 = value;
            }
        }
        private System.Windows.Forms.TreeView _TreeViewResults;

        internal virtual System.Windows.Forms.TreeView TreeViewResults
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _TreeViewResults;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _TreeViewResults = value;
            }
        }
        private System.Windows.Forms.RichTextBox _RichTextBoxConsole;

        internal virtual System.Windows.Forms.RichTextBox RichTextBoxConsole
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _RichTextBoxConsole;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _RichTextBoxConsole = value;
            }
        }
        private System.Windows.Forms.Button _ButtonOpenTestEvidenceFolder;

        internal virtual System.Windows.Forms.Button ButtonOpenTestEvidenceFolder
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonOpenTestEvidenceFolder;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonOpenTestEvidenceFolder != null)
                {
                    _ButtonOpenTestEvidenceFolder.Click -= ButtonOpenTestEvidenceFolder_Click;
                }

                _ButtonOpenTestEvidenceFolder = value;
                if (_ButtonOpenTestEvidenceFolder != null)
                {
                    _ButtonOpenTestEvidenceFolder.Click += ButtonOpenTestEvidenceFolder_Click;
                }
            }
        }
    }
}