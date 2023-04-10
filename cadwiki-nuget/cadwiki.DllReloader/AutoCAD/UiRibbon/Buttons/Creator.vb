Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Media.Imaging
Imports Autodesk.Windows

Namespace AutoCAD.UiRibbon.Buttons
    Public Class Creator
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
                Dim image As BitmapSource = NetUtils.Bitmaps.CreateBitmapSourceFromBitmap(bitMap)
                If image IsNot Nothing Then
                    ribbonButton.Image = image
                    ribbonButton.ShowImage = True
                End If
            End If

            Dim uiRouter As UiRouter = New UiRouter(assemblyName, fullClassName, methodName, parameters, appDomainReloader, iExtensionAppAssembly)
            ribbonButton.CommandParameter = uiRouter
            ribbonButton.CommandHandler = New GenericClickCommandHandler()
            ribbonButton.ToolTip = tooltip
            Return ribbonButton
        End Function
    End Class

End Namespace
