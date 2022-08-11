Option Strict On
Option Infer Off
Option Explicit On

Public Class Strings
    Public Shared Function NotContainsBackSlashVInSubFolder(s As String) As Boolean
        Return Not s.ToLower().Contains("\_v")
    End Function
End Class
