Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.Windows

Namespace AutoCAD.UiRibbon.Buttons
    Public Class DllReloadClickCommandHandler
        Implements System.Windows.Input.ICommand


        Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
            Return True
        End Function

        Public Event CanExecuteChanged(ByVal sender As Object, ByVal e As System.EventArgs) Implements System.Windows.Input.ICommand.CanExecuteChanged

        Public Shared Event ReloadComplete()

        Public Sub Execute(ByVal parameter As Object) Implements System.Windows.Input.ICommand.Execute
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument

            doc.Editor.WriteMessage(vbLf & "NetReloader started..")

            If TypeOf parameter Is RibbonButton Then
                Dim button As RibbonButton = TryCast(parameter, RibbonButton)
                Dim netReloader As AutoCADAppDomainDllReloader = TryCast(button.CommandParameter, AutoCADAppDomainDllReloader)
                If (doc IsNot Nothing) Then
                    Dim userInputDllPath As String = netReloader.UserInputGetDllPath()
                    If String.IsNullOrEmpty(userInputDllPath) Then
                        Return
                    Else
                        netReloader.ReloadDll(doc, userInputDllPath)
                        RaiseEvent ReloadComplete()
                    End If

                End If
            End If
        End Sub


    End Class



End Namespace
