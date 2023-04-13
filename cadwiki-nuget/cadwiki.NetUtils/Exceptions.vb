Option Strict On
Option Infer Off
Option Explicit On

Public Class Exceptions
    Public Shared Function GetStackTraceLines(ex As Exception) As List(Of String)
        Dim stackTrace As String = ex.StackTrace
        Dim seperator As Char() = CType(Environment.NewLine, Char())
        Return stackTrace.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList()
    End Function

    Public Shared Function GetPrettyStringList(ex As Exception) As List(Of String)
        Dim list As New List(Of String)
        list.Add("-----------------------------------------------------------------------------")

        If (Not String.IsNullOrEmpty(ex.Message)) Then
            list.Add("Message : ".PadLeft(26) & ex.Message)
        End If
        If (Not String.IsNullOrEmpty(ex.StackTrace)) Then
            list.Add("StackTrace : ".PadLeft(26) & ex.StackTrace)
        End If
        If (Not String.IsNullOrEmpty(ex.InnerException.Message)) Then
            list.Add("InnerException Message : " & ex.InnerException.Message)
        End If
        'If (Not String.IsNullOrEmpty(ex.Message)) Then
        'End If

        list.Add("-----------------------------------------------------------------------------")
        Return list
    End Function
End Class

