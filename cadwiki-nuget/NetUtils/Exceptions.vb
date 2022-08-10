Option Strict On
Option Infer Off
Option Explicit On

Public Class Exceptions
    Public Shared Function GetStackTraceLines(ex As Exception) As List(Of String)
        Dim stackTrace As String = ex.StackTrace
        Dim seperator As Char() = CType(Environment.NewLine, Char())
        Return stackTrace.Split(seperator, StringSplitOptions.RemoveEmptyEntries).ToList()
    End Function
End Class

