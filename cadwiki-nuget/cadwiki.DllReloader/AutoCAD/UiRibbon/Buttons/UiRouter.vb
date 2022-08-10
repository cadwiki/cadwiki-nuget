Option Strict On
Option Infer Off
Option Explicit On

Namespace AutoCAD.UiRibbon.Buttons
    Public Class UiRouter
        Public FullClassName As String
        Public MethodName As String
        Public Parameters As Object()
        Public AssemblyName As String
        Public NetReloader As AutoCADAppDomainDllReloader
        Public Sub New(fullClassName As String, methodName As String, parameters As Object(),
                       netReloader As AutoCADAppDomainDllReloader)
            Me.FullClassName = fullClassName
            Me.MethodName = methodName
            Me.Parameters = parameters
            Me.NetReloader = netReloader
        End Sub
    End Class
End Namespace
