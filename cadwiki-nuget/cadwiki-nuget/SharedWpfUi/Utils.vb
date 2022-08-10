Option Strict On
Option Infer Off
Option Explicit On
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Public Class Utils


    Public Shared Converter As BrushConverter = New System.Windows.Media.BrushConverter()
    Public Shared ReadOnly Green As Brush = CType(Converter.ConvertFromString("#00FF00"), Brush)
    Public Shared ReadOnly Red As Brush = CType(Converter.ConvertFromString("#FF0000"), Brush)
    Public Shared ReadOnly Normal As Brush = SystemColors.WindowBrush
    Public Shared ReadOnly Yellow As Brush = CType(Converter.ConvertFromString("#FFFF00"), Brush)





    Public Shared Sub SetSuccessStatus(tbs As TextBlock, tbm As TextBlock, message As String)
        tbs.Text = "Success"
        tbs.Background = Green
        tbm.Text = message
    End Sub

    Public Shared Sub SetProcessingStatus(tbs As TextBlock, tbm As TextBlock, message As String)
        tbs.Text = "Processing..."
        tbs.Background = Yellow
        tbm.Text = message
    End Sub

    Public Shared Sub SetErrorStatus(tbs As TextBlock, tbm As TextBlock, message As String)
        tbs.Text = "Error..."
        tbs.Background = Red
        tbm.Text = message
    End Sub
End Class
