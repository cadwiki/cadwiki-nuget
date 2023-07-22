Imports System
Imports System.IO
Imports System.Runtime.InteropServices

Public MustInherit Class TextFileLog
    Public Overridable Property LogName As String
        Get
            Return _logName
        End Get
        Set(ByVal value As String)
            _logName = value
        End Set
    End Property

    Private Shared _logDir As String = "C:\Temp"

    Public Overridable Property LogDir As String
        Get
            Return _logDir
        End Get
        Set(ByVal value As String)
            _logDir = value
            CreateNewLogFile()
        End Set
    End Property

    Private _logName As String = "TextFileLog"
    Private _logExt As String = ".txt"
    Private _logFilePath As String = ""

    Public Overridable Property LogFilePath As String
        Get
            Return _logFilePath
        End Get
        Set(ByVal value As String)
            _logFilePath = value
        End Set
    End Property

    Private _daysToKeepLogFile As Integer = 30

    Public Sub New()
    End Sub

    Public Sub CreateNewLogFile()
        Try
            Dim dir As String = _logDir
            Dim fileName As String = LogName
            Dim fileExt As String = _logExt

            If Not Directory.Exists(dir) Then
                Dim di As DirectoryInfo = Directory.CreateDirectory(dir)
            End If

            Dim timeStamp As String = DateTime.Now.ToString("yyyy-MM-dd")
            _logFilePath = String.Concat(dir, "\", fileName, "_", timeStamp, fileExt)

            If Not File.Exists(_logFilePath) Then
                Dim stream As FileStream = File.Create(_logFilePath)
                stream.Close()
                Write("Log created.")
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Function Write(ByVal message As String) As Boolean
        Try
            Dim timeStamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            timeStamp.PadRight(19)
            File.AppendAllText(_logFilePath, Environment.NewLine & timeStamp & " =>" & vbTab & message)
            Return True
        Catch e As System.Exception
            Return False
        Finally
        End Try
    End Function

    Public Sub Exception(ByVal ex As Exception)
        Write("-----------------------------------------------------------------------------")

        If ex IsNot Nothing Then
            Write(ex.[GetType]().FullName)

            If ex.Message IsNot Nothing Then
                Write("Message".PadRight(22) & " : " & ex.Message)
            End If

            If ex.StackTrace IsNot Nothing Then
                Write("StackTrace".PadRight(22) & " : " & ex.StackTrace)
            End If

            If ex.InnerException IsNot Nothing AndAlso ex.InnerException.Message IsNot Nothing Then
                Write("InnerException Message" & " : " & ex.InnerException.Message)
            End If
        End If

        Write("-----------------------------------------------------------------------------")
    End Sub

    Public Sub Delete()
        Try
            Dim dir As String = Path.GetDirectoryName(_logFilePath)

            If Directory.Exists(dir) Then
                If File.Exists(_logFilePath) Then File.Delete(_logFilePath)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub DeleteOldLogFiles()
        Try
            Dim thresholdDate As DateTime = DateTime.Now.AddDays(-1 * _daysToKeepLogFile)
            Dim directoryInfo As DirectoryInfo = New DirectoryInfo(_logDir)
            Dim files As FileInfo() = directoryInfo.GetFiles()

            For Each file As FileInfo In files

                If file.Extension.ToLower() = _logExt AndAlso IsLogFileOlderThanThreshold(file.Name, thresholdDate) Then

                    Try
                        file.Delete()
                        Write("Deleted outdated log file: " & file.FullName)
                    Catch ex As Exception
                        Exception(ex)
                    End Try
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub

    Private Function IsLogFileOlderThanThreshold(ByVal fileName As String, ByVal thresholdDate As DateTime) As Boolean
        Try
            Dim dateString As String = fileName.Replace(_logName, "").Replace(_logExt, "").Replace("_", "")
            Dim logDate As DateTime = Nothing

            If DateTime.TryParseExact(dateString, "yyyy-MM-dd", Nothing, System.Globalization.DateTimeStyles.None, logDate) Then
                Return logDate < thresholdDate
            End If

            Return False
        Catch ex As Exception

        End Try

    End Function
End Class
