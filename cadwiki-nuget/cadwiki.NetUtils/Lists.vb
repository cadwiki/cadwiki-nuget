Option Strict On
Option Infer Off
Option Explicit On
Public Class Lists
    Public Shared Function StringListToString(lst As List(Of String), delimeter As String) As String
        Dim str As String = ""
        Dim i As Integer = 0
        For Each item As String In lst
            If Not String.IsNullOrEmpty(item) Then
                str += item
            End If
            If i < lst.Count() - 1 And Not String.IsNullOrEmpty(delimeter) Then
                str += delimeter
            End If
            i += 1
        Next
        Return str
    End Function
End Class