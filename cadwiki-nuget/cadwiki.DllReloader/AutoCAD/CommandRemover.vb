Option Strict On
Option Infer Off
Option Explicit On

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.IO
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Runtime

Namespace AutoCAD
    Public Class CommandRemover

        Public Declare Auto Function RemoveCommand Lib "cadwiki.AcRemoveCmdGroup.dll" Alias "removeCommand" _
            (<MarshalAs(UnmanagedType.LPWStr)> ByVal groupName As StringBuilder,
             <MarshalAs(UnmanagedType.LPWStr)> ByVal commandGlobalName As StringBuilder) As Integer

        Public Shared Sub RemoveAllCommandsFromiExtensionAppAssembly(doc As Document, iExtensionAppAssembly As Assembly, dllPath As String)
            Dim currentTypes As Type() = cadwiki.NetUtils.AssemblyUtils.GetTypesSafely(iExtensionAppAssembly)
            Dim commandMethodAttributesToMethodInfos As Dictionary(Of CommandMethodAttribute, MethodInfo) =
                AcadAssemblyUtils.GetCommandMethodDictionarySafely(currentTypes)
            If doc IsNot Nothing Then
                If commandMethodAttributesToMethodInfos.Count = 0 Then
                    doc.Editor.WriteMessage(vbLf & "No commands found to remove.")
                Else
                    doc.Editor.WriteMessage(vbLf & "Removing all commands from current assembly.")
                    RemoveCommands(doc, dllPath, commandMethodAttributesToMethodInfos)
                End If
            End If

        End Sub

        Private Shared Sub RemoveCommands(doc As Document, dllPath As String,
            commandMethodAttributesToMethodInfos As Dictionary(Of CommandMethodAttribute, MethodInfo))
            For Each dictionaryItem As KeyValuePair(Of CommandMethodAttribute, MethodInfo) _
                In commandMethodAttributesToMethodInfos
                Dim commandMethodAttribute As CommandMethodAttribute = dictionaryItem.Key
                Dim methodInfo As MethodInfo = dictionaryItem.Value
                Dim commandLineMethodString As String = commandMethodAttribute.GlobalName
                Dim commandGroupName As String = commandMethodAttribute.GroupName
                Dim groupName As New StringBuilder(256)
                groupName.Append(commandGroupName)
                Dim commandGlobalName As New StringBuilder(256)
                commandGlobalName.Append(commandLineMethodString)
                Dim dllFolder As String = Path.GetDirectoryName(dllPath)
                If System.IO.File.Exists(dllFolder + "\" + "cadwiki.AcRemoveCmdGroup.dll") Then
                    Dim wasCommandUndefined As Integer = RemoveCommand(groupName, commandGlobalName)
                    If wasCommandUndefined = 0 Then
                        doc.Editor.WriteMessage(vbLf & "Command successfully removed: " + commandGlobalName.ToString)
                    Else
                        doc.Editor.WriteMessage(vbLf & "Remove command failed: " & commandLineMethodString & ".")
                    End If
                Else
                    Throw New System.Exception("cadwiki.AcRemoveCmdGroup.dll not found in assembly folder. Can't unload command group.")
                End If

            Next
        End Sub
    End Class
End Namespace



