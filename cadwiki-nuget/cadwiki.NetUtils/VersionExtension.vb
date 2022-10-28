Imports System.Runtime.CompilerServices

Public Module VersionExtension
    <Extension()>
    Public Function IncrementRevision(ByVal version As Version) As Version
        Return version.AddVersion(0, 0, 0, 1)
    End Function
    <Extension()>
    Public Function IncrementBuild(ByVal version As Version) As Version
        Return version.IncrementBuild(True)
    End Function
    <Extension()>
    Public Function IncrementBuild(ByVal version As Version, ByVal resetLowerNumbers As Boolean) As Version
        Return AddVersion(version, 0, 0, 1, If(resetLowerNumbers, -version.Revision, 0))
    End Function
    <Extension()>
    Public Function IncrementMinor(ByVal version As Version) As Version
        Return version.IncrementMinor(True)
    End Function
    <Extension()>
    Public Function IncrementMinor(ByVal version As Version, ByVal resetLowerNumbers As Boolean) As Version
        Return AddVersion(version, 0, 1, If(resetLowerNumbers, -version.Build, 0), If(resetLowerNumbers, -version.Revision, 0))
    End Function
    <Extension()>
    Public Function IncrementMajor(ByVal version As Version) As Version
        Return version.IncrementMajor(True)
    End Function
    <Extension()>
    Public Function IncrementMajor(ByVal version As Version, ByVal resetLowerNumbers As Boolean) As Version
        Return AddVersion(version, 1, If(resetLowerNumbers, -version.Minor, 0), If(resetLowerNumbers, -version.Build, 0), If(resetLowerNumbers, -version.Revision, 0))
    End Function

    <Extension()>
    Public Function AddVersion(ByVal version As Version, ByVal pAddVersion As String) As Version
        Return AddVersion(version, New Version(pAddVersion))
    End Function
    <Extension()>
    Public Function AddVersion(ByVal version As Version, ByVal pAddVersion As Version) As Version
        Return AddVersion(version, pAddVersion.Major, pAddVersion.Minor, pAddVersion.Build, pAddVersion.Revision)
    End Function
    <Extension()>
    Public Function AddVersion(ByVal version As Version, ByVal major As Integer, ByVal minor As Integer, ByVal build As Integer, ByVal revision As Integer) As Version
        Return SetVersion(version, version.Major + major, version.Minor + minor, version.Build + build, version.Revision + revision)
    End Function
    <Extension()>
    Public Function SetVersion(ByVal version As Version, ByVal major As Integer, ByVal minor As Integer, ByVal build As Integer, ByVal revision As Integer) As Version
        Return New Version(major, minor, build, revision)
    End Function

End Module


