Option Strict On
Option Infer Off
Option Explicit On

Imports System.Reflection
Imports Autodesk.AutoCAD.Runtime

Namespace AutoCAD
    Public Class AcadAssemblyUtils
        Public Shared Function GetCommandMethodDictionarySafely(types As Type()) As _
                Dictionary(Of CommandMethodAttribute, MethodInfo)
            Dim commandMethodAttributesToMethodInfos As New Dictionary(Of CommandMethodAttribute, MethodInfo)
            For Each type As Type In types
                Dim methodInfos As MethodInfo() = type.GetMethods()

                For Each methodInfo As MethodInfo In methodInfos
                    Dim commandMethodAttributeObject As Object =
                        DoesMethodInfoHaveAutoCADCommandAttribute(methodInfo)
                    If (commandMethodAttributeObject IsNot Nothing) Then
                        Dim commandMethodAttribute As CommandMethodAttribute =
                            CType(commandMethodAttributeObject, CommandMethodAttribute)
                        commandMethodAttributesToMethodInfos.Add(commandMethodAttribute, methodInfo)
                    End If

                Next
            Next
            Return commandMethodAttributesToMethodInfos
        End Function
        Public Shared Function GetAppObjectSafely(types As Type()) As Object
            If types IsNot Nothing Then
                For Each type As Type In types
                    Dim interfaceList As List(Of Type) = type.GetInterfaces().ToList()
                    Dim isClassIExtensionApplication As Boolean = False

                    If interfaceList.Contains(GetType(IExtensionApplication)) Then
                        isClassIExtensionApplication = True
                    End If

                    If (type.IsClass And isClassIExtensionApplication) Then
                        Dim appObject As Object = Activator.CreateInstance(type)
                        Return appObject
                    End If
                Next
            End If
            Return Nothing
        End Function


        Public Shared Function GetNewestAssembly(assemblies() As Assembly, assemblyName As String, dllLocation As String) _
            As Assembly
            Dim newestAsm As Assembly = Nothing
            Dim match As Assembly = Nothing
            For Each domainAssembly As Assembly In assemblies
                Dim domainAssemblyName As AssemblyName = domainAssembly.GetName()
                If domainAssemblyName.Name.ToLower().Equals(assemblyName.ToLower()) Then
                    match = domainAssembly
                    Dim matchVersionNumber As String = GetAssemblyVersionFromFullName(domainAssembly.FullName)
                    If newestAsm Is Nothing Then
                        newestAsm = match
                    Else
                        Dim newestVersionNumber As String = GetAssemblyVersionFromFullName(newestAsm.FullName)
                        Dim comparisonResult As Integer = CompareFileVersion(matchVersionNumber, newestVersionNumber)
                        If comparisonResult = 1 Then
                            newestAsm = domainAssembly
                        End If
                    End If
                End If
            Next

            Return newestAsm
        End Function

        Public Shared Function GetAssemblyVersionFromFullName(fullName As String) As String
            Dim strArry As String() = Split(fullName, ", ")
            Dim version As String = strArry(1)
            Dim verArry As String() = Split(version, "=")
            Dim versionNumber As String = verArry(1)
            Return versionNumber
        End Function

        Public Shared Function CompareFileVersion(strFileVersion1 As String, strFileVersion2 As String) As Integer
            ' -1 = File Version 1 is less than File Version 2
            ' 0  = Versions are the same
            ' 1  = File version 1 is greater than File Version 2
            Dim intResult As Integer = 0
            Dim strAryFileVersion1() As String = Split(strFileVersion1, ".")
            Dim strAryFileVersion2() As String = Split(strFileVersion2, ".")
            Dim i As Integer
            For i = 0 To UBound(strAryFileVersion1)
                Dim num1 As Integer = CInt(strAryFileVersion1(i))
                Dim num2 As Integer = CInt(strAryFileVersion2(i))
                If num1 > num2 Then
                    intResult = 1
                ElseIf num1 < num2 Then
                    intResult = -1
                End If
                'If we have found that the result is not > or <, no need to proceed
                If intResult <> 0 Then Exit For
            Next
            Return intResult
        End Function



        Private Shared Function DoesMethodInfoHaveAutoCADCommandAttribute(methodInfo As MethodInfo) As Object
            Dim objectAttributes As Object() = methodInfo.GetCustomAttributes(True)
            For Each objectAttribute As Object In objectAttributes
                If (objectAttribute.GetType() Is GetType(CommandMethodAttribute)) Then
                    Return objectAttribute
                End If
            Next
            Return Nothing
        End Function




    End Class


End Namespace



