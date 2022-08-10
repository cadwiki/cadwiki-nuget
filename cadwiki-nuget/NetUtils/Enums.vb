Option Strict On
Option Infer Off
Option Explicit On

Public Class Enums
    Public Shared Function GetStringsFromEnum(type As Type) As List(Of String)
        Dim strings As New List(Of String)
        Dim items As Array = System.Enum.GetNames(type)
        For Each item As String In items
            strings.Add(item)
        Next
        Return strings
    End Function
End Class
