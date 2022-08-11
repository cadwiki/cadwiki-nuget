Option Strict On
Option Infer Off
Option Explicit On

Imports System.Reflection
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.Windows

Namespace AutoCAD.UiRibbon.Buttons
    Public Class DllReloadClickCommandHandler
        Implements System.Windows.Input.ICommand


        Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
            Return True
        End Function

        Public Event CanExecuteChanged(ByVal sender As Object, ByVal e As System.EventArgs) Implements System.Windows.Input.ICommand.CanExecuteChanged

        Public Sub Execute(ByVal parameter As Object) Implements System.Windows.Input.ICommand.Execute
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument

            doc.Editor.WriteMessage(vbLf & "DllReloadClickCommandHandler started..")

            If TypeOf parameter Is RibbonButton Then
                Dim button As RibbonButton = TryCast(parameter, RibbonButton)
                Dim uiRouter As UiRouter = TryCast(button.CommandParameter, UiRouter)
                If (doc IsNot Nothing) Then
                    Dim netReloader As AutoCADAppDomainDllReloader = uiRouter.NetReloader
                    Dim iExtensionAppAssembly As Assembly = uiRouter.IExtensionAppAssembly
                    Dim userInputDllPath As String = netReloader.UserInputGetDllPath()
                    If String.IsNullOrEmpty(userInputDllPath) Then
                        Return
                    Else
                        netReloader.ReloadDll(doc, iExtensionAppAssembly, userInputDllPath)
                    End If

                End If
            End If
        End Sub


    End Class



End Namespace
