Option Strict On
Option Infer Off
Option Explicit On
Imports System.IO
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

    Public Shared Function ReadEmbeddedResourceToString(assembly As Assembly, searchPattern As String) As String
        Dim reader As StreamReader = NetUtils.AssemblyUtils.GetStreamReaderFromEmbeddedResource(assembly, searchPattern)
        If (reader IsNot Nothing) Then
            Dim templateString As String = reader.ReadToEnd()
            Return templateString
        End If
        Return Nothing
    End Function

    Public Shared Function GetStreamReaderFromEmbeddedResource(assembly As Assembly, searchPattern As String) As StreamReader
        Dim resourceName As String = assembly.GetManifestResourceNames().FirstOrDefault(
                Function(x) x.Contains(searchPattern)
            )
        If (resourceName IsNot Nothing) Then
            Dim stream As Stream = assembly.GetManifestResourceStream(resourceName)
            If (stream IsNot Nothing) Then
                Dim reader As StreamReader = New StreamReader(stream, Text.Encoding.Default)
                Return reader
            End If
        End If
        Return Nothing
    End Function


End Class
