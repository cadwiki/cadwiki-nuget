Public Class PathUtils
    Public Shared Function GetAutoCADAppDomainDllReloaderTempFolder() As String
        Return System.IO.Path.GetTempPath() + "cadwiki.AutoCADAppDomainDllReloader"
    End Function

    Public Shared Function GetAutoCADAppDomainDllReloaderTempFolderLogFilePath() As String
        Return GetAutoCADAppDomainDllReloaderTempFolder() + "\" + "AutoCADAppDomainDllReloader.txt"
    End Function

End Class
