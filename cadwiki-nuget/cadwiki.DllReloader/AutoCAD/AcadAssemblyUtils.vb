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



