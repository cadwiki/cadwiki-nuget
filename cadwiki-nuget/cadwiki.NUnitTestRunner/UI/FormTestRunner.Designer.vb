Namespace UI
    Partial Class FormTestRunner
        Inherits System.Windows.Forms.Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.ButtonOk = New System.Windows.Forms.Button()
            Me.Cancel = New System.Windows.Forms.Button()
            Me.GroupBox1 = New System.Windows.Forms.GroupBox()
            Me.TreeViewResults = New System.Windows.Forms.TreeView()
            Me.GroupBox2 = New System.Windows.Forms.GroupBox()
            Me.RichTextBoxConsole = New System.Windows.Forms.RichTextBox()
            Me.ButtonOpenTestEvidenceFolder = New System.Windows.Forms.Button()
            Me.GroupBox1.SuspendLayout()
            Me.GroupBox2.SuspendLayout()
            Me.SuspendLayout()
            '
            'ButtonOk
            '
            Me.ButtonOk.Location = New System.Drawing.Point(848, 173)
            Me.ButtonOk.Name = "ButtonOk"
            Me.ButtonOk.Size = New System.Drawing.Size(75, 23)
            Me.ButtonOk.TabIndex = 0
            Me.ButtonOk.Text = "Ok"
            Me.ButtonOk.UseVisualStyleBackColor = True
            '
            'Cancel
            '
            Me.Cancel.Location = New System.Drawing.Point(218, 173)
            Me.Cancel.Name = "Cancel"
            Me.Cancel.Size = New System.Drawing.Size(75, 23)
            Me.Cancel.TabIndex = 1
            Me.Cancel.Text = "Cancel"
            Me.Cancel.UseVisualStyleBackColor = True
            '
            'GroupBox1
            '
            Me.GroupBox1.Controls.Add(Me.TreeViewResults)
            Me.GroupBox1.Location = New System.Drawing.Point(12, 12)
            Me.GroupBox1.Name = "GroupBox1"
            Me.GroupBox1.Size = New System.Drawing.Size(580, 155)
            Me.GroupBox1.TabIndex = 2
            Me.GroupBox1.TabStop = False
            Me.GroupBox1.Text = "TestResults"
            '
            'TreeViewResults
            '
            Me.TreeViewResults.Location = New System.Drawing.Point(6, 21)
            Me.TreeViewResults.Name = "TreeViewResults"
            Me.TreeViewResults.Size = New System.Drawing.Size(566, 128)
            Me.TreeViewResults.TabIndex = 0
            '
            'GroupBox2
            '
            Me.GroupBox2.Controls.Add(Me.RichTextBoxConsole)
            Me.GroupBox2.Location = New System.Drawing.Point(590, 12)
            Me.GroupBox2.Name = "GroupBox2"
            Me.GroupBox2.Size = New System.Drawing.Size(580, 155)
            Me.GroupBox2.TabIndex = 3
            Me.GroupBox2.TabStop = False
            Me.GroupBox2.Text = "Log"
            '
            'RichTextBoxConsole
            '
            Me.RichTextBoxConsole.Location = New System.Drawing.Point(8, 22)
            Me.RichTextBoxConsole.Name = "RichTextBoxConsole"
            Me.RichTextBoxConsole.Size = New System.Drawing.Size(566, 127)
            Me.RichTextBoxConsole.TabIndex = 0
            Me.RichTextBoxConsole.Text = ""
            '
            'ButtonOpenTestEvidenceFolder
            '
            Me.ButtonOpenTestEvidenceFolder.Location = New System.Drawing.Point(958, 173)
            Me.ButtonOpenTestEvidenceFolder.Name = "ButtonOpenTestEvidenceFolder"
            Me.ButtonOpenTestEvidenceFolder.Size = New System.Drawing.Size(80, 23)
            Me.ButtonOpenTestEvidenceFolder.TabIndex = 4
            Me.ButtonOpenTestEvidenceFolder.Text = "Evidence"
            Me.ButtonOpenTestEvidenceFolder.UseVisualStyleBackColor = True
            '
            'FormTestRunner
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(1182, 203)
            Me.Controls.Add(Me.ButtonOpenTestEvidenceFolder)
            Me.Controls.Add(Me.GroupBox2)
            Me.Controls.Add(Me.GroupBox1)
            Me.Controls.Add(Me.Cancel)
            Me.Controls.Add(Me.ButtonOk)
            Me.Name = "FormTestRunner"
            Me.Text = "FormTestRunner"
            Me.GroupBox1.ResumeLayout(False)
            Me.GroupBox2.ResumeLayout(False)
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents ButtonOk As Windows.Forms.Button
        Friend WithEvents Cancel As Windows.Forms.Button
        Friend WithEvents GroupBox1 As Windows.Forms.GroupBox
        Friend WithEvents GroupBox2 As Windows.Forms.GroupBox
        Friend WithEvents TreeViewResults As Windows.Forms.TreeView
        Friend WithEvents RichTextBoxConsole As Windows.Forms.RichTextBox
        Friend WithEvents ButtonOpenTestEvidenceFolder As Windows.Forms.Button
    End Class
End Namespace



