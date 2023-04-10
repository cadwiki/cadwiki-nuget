Imports System.IO
Public Class TextFileLog

    Private _logFilePath As String = ""

    Public Sub New()
        _logFilePath = System.IO.Path.GetTempFileName()
    End Sub

    Public Sub New(ByVal filePath As String)
        Dim dir As String = Path.GetDirectoryName(filePath)
        Dim fileName As String = Path.GetFileNameWithoutExtension(filePath)
        Dim fileExt As String = Path.GetExtension(filePath)

        'create directory if it does not exist
        If Not Directory.Exists(dir) Then
            Dim di As DirectoryInfo = Directory.CreateDirectory(dir)
        End If

        Dim timeStamp As String = Date.Now.ToString("yyyy-MM-dd-HH-mm-ss")
        _logFilePath = String.Concat(dir, "\", fileName, "-", timeStamp, fileExt)
    End Sub


    Public Function Write(ByVal message As String) As Boolean
        Try
            Dim timeStamp As String = Date.Now.ToString("yyyy-MM-dd HH:mm:s")
            'pad to 19 spaces to keep log file lined up
            timeStamp.PadLeft(19)
            File.AppendAllText(_logFilePath, vbCrLf & timeStamp & " =>" & vbTab & message)
            Return True
        Catch e As Exception
            Return False
        Finally
        End Try
    End Function

    Public Sub Exception(ByVal ex As Exception)
        Write("-----------------------------------------------------------------------------")
        If ex IsNot Nothing Then
            Write(ex.GetType().FullName)
            Write("Message : ".PadLeft(26) & ex.Message)
            Write("StackTrace : ".PadLeft(26) & ex.StackTrace)
            Write("InnerException Message : " & ex.InnerException.Message)
        End If
        Write("-----------------------------------------------------------------------------")
    End Sub

    Public Sub Delete()
        Dim dir As String = Path.GetDirectoryName(_logFilePath)
        If Directory.Exists(dir) Then
            If File.Exists(_logFilePath) Then
                File.Delete(_logFilePath)
            End If
        End If
    End Sub


End Class