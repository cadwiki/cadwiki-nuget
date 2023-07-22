Imports cadwiki.NetUtils

Public Class ReloaderLog
    Inherits TextFileLog

    Public Overrides Property LogName As String
        Get
            Return MyBase.LogName
        End Get
        Set(ByVal value As String)
            MyBase.LogName = value
        End Set
    End Property

    Public Overrides Property LogDir As String
        Get
            Return MyBase.LogDir
        End Get
        Set(ByVal value As String)
            MyBase.LogDir = value
        End Set
    End Property

    Public Sub New()
        LogName = "ReloaderLog"
        CreateNewLogFile()
        DeleteOldLogFiles()
    End Sub
End Class
