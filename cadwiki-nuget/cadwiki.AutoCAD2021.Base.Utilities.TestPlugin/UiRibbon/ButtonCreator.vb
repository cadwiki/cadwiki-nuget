Imports Autodesk.Windows
Imports cadwiki.DllReloader.AutoCAD
Imports cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
Imports System.Drawing
Imports System.Reflection

Namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon
    Friend Class ButtonCreator
        Public Shared Function Create(ByVal name As String,
                                      ByVal text As String,
                                      ByVal tooltip As String,
                                      ByVal bitMap As Bitmap,
                                      ByVal assemblyName As String,
                                      ByVal fullClassName As String,
                                      ByVal methodName As String,
                                      ByVal parameters As Object(),
                                      ByVal appDomainReloader As AutoCADAppDomainDllReloader,
                                      ByVal iExtensionAppAssembly As Assembly) As RibbonButton


            Dim ribbonButton = New RibbonButton()
            ribbonButton.Name = name
            ribbonButton.ShowText = True
            ribbonButton.Text = text
            ribbonButton.Size = RibbonItemSize.Standard

            If bitMap IsNot Nothing Then
                Dim image = FileStore.Bitmaps.CreateBitmapSourceFromGdiBitmapForAutoCADButtonIcon(bitMap)
                If image IsNot Nothing Then
                    ribbonButton.Image = image
                    ribbonButton.ShowImage = True
                End If
            End If

            Dim uiRouter = New UiRouter(assemblyName, fullClassName, methodName, parameters, appDomainReloader, iExtensionAppAssembly)
            ribbonButton.CommandParameter = uiRouter
            ribbonButton.CommandHandler = New GenericClickCommandHandler()
            ribbonButton.ToolTip = tooltip
            Return ribbonButton
        End Function
    End Class
End Namespace
