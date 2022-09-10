Option Strict On
Option Infer Off
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports NUnit.Framework

Public Class Utils
    Public Shared Function GetTypeByFullName(fullName As String, types As Type()) As Type
        For Each type As Type In types
            If (type.FullName.Equals(fullName)) Then
                Return type
            End If
        Next
        Return Nothing
    End Function

    Public Shared Function GetSetupMethod(types As Type()) As Tuple(Of Type, MethodInfo)

        Dim typeToMethodInfo As New List(Of Tuple(Of Type, MethodInfo))
        For Each type As Type In types
            Dim methodInfos As MethodInfo() = type.GetMethods()

            For Each methodInfo As MethodInfo In methodInfos
                Dim setupAttribute As Object = DoesMethodInfoHaveSetupAttribute(methodInfo)
                If (setupAttribute IsNot Nothing) Then
                    Dim tuple As Tuple(Of Type, MethodInfo) = New Tuple(Of Type, MethodInfo)(type, methodInfo)
                    Return tuple
                End If

            Next
        Next
        Return Nothing
    End Function

    Public Shared Function GetTearDownMethod(types As Type()) As Tuple(Of Type, MethodInfo)

        Dim typeToMethodInfo As New List(Of Tuple(Of Type, MethodInfo))
        For Each type As Type In types
            Dim methodInfos As MethodInfo() = type.GetMethods()

            For Each methodInfo As MethodInfo In methodInfos
                Dim tearDownAttribute As Object = DoesMethodInfoHaveTearDownAttribute(methodInfo)
                If (tearDownAttribute IsNot Nothing) Then
                    Dim tuple As Tuple(Of Type, MethodInfo) = New Tuple(Of Type, MethodInfo)(type, methodInfo)
                    Return tuple
                End If

            Next
        Next
        Return Nothing
    End Function

    Public Shared Function GetTestMethodDictionarySafely(types As Type()) As List(Of Tuple(Of Type, MethodInfo))

        Dim typeToMethodInfo As New List(Of Tuple(Of Type, MethodInfo))
        For Each type As Type In types
            Dim methodInfos As MethodInfo() = type.GetMethods()

            For Each methodInfo As MethodInfo In methodInfos
                Dim testAttribute As Object = DoesMethodInfoHaveTestAttribute(methodInfo)
                If (testAttribute IsNot Nothing) Then
                    Dim commandMethodAttribute As TestAttribute = CType(testAttribute, TestAttribute)
                    Dim tuple As Tuple(Of Type, MethodInfo) = New Tuple(Of Type, MethodInfo)(type, methodInfo)
                    typeToMethodInfo.Add(tuple)
                End If

            Next
        Next
        Return typeToMethodInfo
    End Function

    Private Shared Function DoesMethodInfoHaveSetupAttribute(methodInfo As MethodInfo) As Object
        Dim objectAttributes As Object() = methodInfo.GetCustomAttributes(True)
        For Each objectAttribute As Object In objectAttributes
            If (objectAttribute.GetType() Is GetType(SetUpAttribute)) Then
                Return objectAttribute
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function DoesMethodInfoHaveTearDownAttribute(methodInfo As MethodInfo) As Object
        Dim objectAttributes As Object() = methodInfo.GetCustomAttributes(True)
        For Each objectAttribute As Object In objectAttributes
            If (objectAttribute.GetType() Is GetType(TearDownAttribute)) Then
                Return objectAttribute
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function DoesMethodInfoHaveTestAttribute(methodInfo As MethodInfo) As Object
        Dim objectAttributes As Object() = methodInfo.GetCustomAttributes(True)
        For Each objectAttribute As Object In objectAttributes
            If (objectAttribute.GetType() Is GetType(TestAttribute)) Then
                Return objectAttribute
            End If
        Next
        Return Nothing
    End Function
End Class

