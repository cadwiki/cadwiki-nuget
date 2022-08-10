'Option Strict On
Option Infer Off
Option Explicit On

Imports System.IO
Imports System.Reflection
Imports System.Windows.Input
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.Windows

Namespace AutoCAD.UiRibbon.Buttons
    Public Class GenericClickCommandHandler
        Implements ICommand

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged


        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument

            If TypeOf parameter Is RibbonButton Then
                Dim button As RibbonButton = TryCast(parameter, RibbonButton)
                Dim netReloader As AutoCADAppDomainDllReloader = Nothing
                If (doc IsNot Nothing) Then
                    doc.Editor.WriteMessage(vbLf & "RibbonButton Executing Method.")
                    Dim uiRouter As UiRouter = Nothing
                    Try
                        uiRouter = button.CommandParameter
                        netReloader = uiRouter.NetReloader
                        Dim assemblyName As String = Split(uiRouter.FullClassName, ".")(0)
                        doc.Editor.WriteMessage(vbLf & "Full class name: " & uiRouter.FullClassName)
                        doc.Editor.WriteMessage(vbLf & "Method name: " & uiRouter.MethodName)
                        doc.Editor.WriteMessage(vbLf & vbLf)
                        Dim dllRepo As String = Path.GetDirectoryName(netReloader.GetDllPath())
                        Dim asm As Assembly = GetNewestAssembly(AppDomain.CurrentDomain.GetAssemblies(), assemblyName,
                                                                dllRepo + "\" + assemblyName + ".dll")
                        'Dim asm As System.Reflection.Assembly = If(App.ReloadedAssembly, Assembly.GetExecutingAssembly)
                        Dim types As Type() = NetUtils.AssemblyUtils.GetTypesSafely(asm)
                        Dim type As Type = asm.GetType(uiRouter.FullClassName)
                        Dim methodInfo As MethodInfo = type.GetMethod(uiRouter.MethodName)
                        If methodInfo = Nothing Then
                            doc.Editor.WriteMessage(vbLf & "Method not found: " & uiRouter.MethodName)
                            doc.Editor.WriteMessage(vbLf & vbLf)
                        Else
                            Dim o As Object = Activator.CreateInstance(type)
                            methodInfo.Invoke(o, uiRouter.Parameters)
                        End If

                    Catch ex As Exception
                        doc.Editor.WriteMessage(vbLf & "Exception: " & ex.Message)
                        doc.Editor.WriteMessage(
                            vbLf & "Mostly likely caused by incorrect solution name in UiRouter object: " &
                            netReloader.GetIExtensionApplicationClassName())
                        If (uiRouter IsNot Nothing) Then
                            doc.Editor.WriteMessage(vbLf & "UiRouter object: " & uiRouter.FullClassName)
                            doc.Editor.WriteMessage(vbLf & vbLf)
                        End If
                    End Try
                End If
            End If
        End Sub

        Public Shared Function GetNewestAssembly(assemblies() As Assembly, assemblyName As String, dllLocation As String) _
            As Assembly
            Dim newestAsm As Assembly = Nothing
            Dim match As Assembly = Nothing
            For Each domainAssembly As Assembly In assemblies
                Dim domainAssemblyName As AssemblyName = domainAssembly.GetName()
                If domainAssemblyName.Name.ToLower().Equals(assemblyName.ToLower()) Then
                    match = domainAssembly
                    Dim matchVersionNumber = GetAssemblyVersionFromFullName(domainAssembly.FullName)
                    If newestAsm Is Nothing Then
                        newestAsm = match
                    Else
                        Dim newestVersionNumber = GetAssemblyVersionFromFullName(newestAsm.FullName)
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

        Sub CallMethod(f As Action)
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            doc.Editor.WriteMessage(vbLf & "Calling...")
            f()
        End Sub

        Public Shared Function CompareFileVersion(strFileVersion1 As String, strFileVersion2 As String) As Integer
            ' -1 = File Version 1 is less than File Version 2
            ' 0  = Versions are the same
            ' 1  = File version 1 is greater than File Version 2
            Dim intResult = 0
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
    End Class
End Namespace
