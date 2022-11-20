'Option Strict On
'Option Infer Off
'Option Explicit On

Imports System.IO
Imports System.Reflection
Imports System.Windows.Input
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.Windows

Namespace AutoCAD.UiRibbon.Buttons
    Public Class GenericClickCommandHandler
        Implements ICommand

        Private _doc As Document = Nothing

        Public Sub New(doc As Document)
            _doc = doc
        End Sub

        Public Sub New()
        End Sub

        Public Sub ConsoleOut(message As String)
            If (_doc Is Nothing) Then
                Console.WriteLine(message)
            Else
                _doc.Editor.WriteMessage(vbLf & message)
                _doc.Editor.WriteMessage(vbLf)
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged


        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim uiRouter As UiRouter = Nothing
            Dim netReloader As AutoCADAppDomainDllReloader = Nothing
            Try
                If TypeOf parameter Is RibbonButton Then
                    Dim button As RibbonButton = TryCast(parameter, RibbonButton)
                    ConsoleOut("GenericClickCommandHandler Executing Method.")
                    uiRouter = button.CommandParameter
                    netReloader = uiRouter.NetReloader
                    Dim assemblyName As String = uiRouter.AssemblyName
                    ConsoleOut("Full class name: " & uiRouter.FullClassName)
                    ConsoleOut("Method name: " & uiRouter.MethodName)
                    Dim dllRepo As String = Path.GetDirectoryName(netReloader.GetDllPath())
                    Dim asm As Assembly = AcadAssemblyUtils.GetNewestAssembly(AppDomain.CurrentDomain.GetAssemblies(), assemblyName,
                                                                dllRepo + "\" + assemblyName + ".dll")
                    'Dim asm As System.Reflection.Assembly = If(App.ReloadedAssembly, Assembly.GetExecutingAssembly)
                    Dim types As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(asm)
                    Dim type As Type = asm.GetType(uiRouter.FullClassName)
                    Dim methodInfo As MethodInfo = type.GetMethod(uiRouter.MethodName)
                    If methodInfo = Nothing Then
                        ConsoleOut("Method not found: " & uiRouter.MethodName)
                    Else
                        Dim o As Object = Activator.CreateInstance(type)
                        If (uiRouter.Parameters IsNot Nothing) Then
                            methodInfo.Invoke(o, uiRouter.Parameters)
                        Else
                            methodInfo.Invoke(o, Nothing)
                        End If
                    End If
                End If
            Catch ex As Exception
                Dim window As cadwiki.WpfUi.Templates.WindowAutoCADException =
                        New WpfUi.Templates.WindowAutoCADException(ex)
                window.Show()
                ConsoleOut("Exception: " & ex.Message)
                If (ex.Message.Equals("The path is not of a legal form.")) Then
                    ConsoleOut("Mostly likely caused by incorrect method name in UiRouter object.")
                    ConsoleOut("Double check that the Method name and Full class name above are correct.")
                Else
                    ConsoleOut("Mostly likely caused by incorrect solution name in UiRouter object: " &
                            netReloader.GetIExtensionApplicationClassName())
                End If

                If (UiRouter IsNot Nothing) Then
                    ConsoleOut("UiRouter object: " & UiRouter.FullClassName)
                End If
            End Try

        End Sub



        Sub CallMethod(f As Action)
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            doc.Editor.WriteMessage(vbLf & "Calling...")
            f()
        End Sub


    End Class
End Namespace
