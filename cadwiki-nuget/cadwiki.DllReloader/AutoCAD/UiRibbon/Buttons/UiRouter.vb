Option Strict On
Option Infer Off
Option Explicit On
Imports System.Reflection

Namespace AutoCAD.UiRibbon.Buttons
    Public Class UiRouter
        Public FullClassName As String
        Public MethodName As String
        Public Parameters As Object()
        Public AssemblyName As String
        Public NetReloader As AutoCADAppDomainDllReloader
        Public IExtensionAppAssembly As Assembly

        Public Sub New(fullClassName As String, methodName As String, parameters As Object(),
                       netReloader As AutoCADAppDomainDllReloader, iExtensionAppAssembly As Assembly)
            Me.FullClassName = fullClassName
            Me.MethodName = methodName
            Me.Parameters = parameters
            Me.NetReloader = netReloader
            Me.IExtensionAppAssembly = iExtensionAppAssembly
        End Sub
    End Class
End Namespace
