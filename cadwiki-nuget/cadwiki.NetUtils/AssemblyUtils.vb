Option Strict On
Option Infer Off
Option Explicit On
Imports System.Reflection

Public Class AssemblyUtils
    Public Shared Function GetCurrentlyExecutingAssembly() As Assembly
        Return System.Reflection.Assembly.GetExecutingAssembly()
    End Function

    Public Shared Function GetTypesSafely(assembly As Assembly) As Type()
        If assembly IsNot Nothing Then
            Try
                Return assembly.GetTypes()
            Catch ex As ReflectionTypeLoadException
                Dim foundTypes As Type() = ex.Types
                Dim validTypes As New List(Of Type)
                For Each foundType As Type In foundTypes
                    If (foundType IsNot Nothing) Then
                        validTypes.Add(foundType)
                    End If
                Next
                Dim validTypeArray As Type() = validTypes.ToArray
                Return validTypeArray
            End Try
        Else
            Return Nothing
        End If

    End Function

    Public Shared Function GetVersion(assembly As Assembly) As Version
        Dim assemblyVersion As Version = assembly.GetName().Version
        Return assemblyVersion
    End Function


    Public Shared Function GetFolderLocationFromCodeBase(assembly As Assembly) As String
        Dim fileLocation As String = GetFileLocationFromCodeBase(assembly)
        If String.IsNullOrEmpty(fileLocation) Then
            Return Nothing
        End If
        Return System.IO.Path.GetDirectoryName(fileLocation)
    End Function

    Public Shared Function GetFileLocationFromCodeBase(assembly As Assembly) As String
        Dim location As String = assembly.Location
        If String.IsNullOrEmpty(location) Then
            location = assembly.CodeBase
            location = location.Replace("file:///", "")
        End If
        Return location
    End Function

End Class
