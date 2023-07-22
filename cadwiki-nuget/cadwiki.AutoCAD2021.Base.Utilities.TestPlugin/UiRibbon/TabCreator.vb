Imports System
Imports System.Collections.Generic
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.Windows
Imports Microsoft.VisualBasic

Namespace cadwiki.AutoCAD2021.Base.Utilities.TestPlugin.UiRibbon.Tabs
    Public Class TabCreator
        Public Shared Sub AddDevTab(ByVal doc As Document)
            Dim devTab = Tabs.DevTab.Create()
            Tabs.DevTab.AddAllPanels(devTab)
            Dim allRibbonTabs = New List(Of RibbonTab)(New RibbonTab() {devTab})
            AddTabs(doc, allRibbonTabs)
        End Sub

        Private Shared Sub AddTabs(ByVal doc As Document, ByVal ribbonTabs As List(Of RibbonTab))
            doc.Editor.WriteMessage(vbLf & "Adding tabs...")
            For Each ribbonTab In ribbonTabs
                AddTab(doc, ribbonTab)
            Next
        End Sub

        Private Shared Sub AddTab(ByVal doc As Document, ByVal ribbonTab As RibbonTab)
            doc.Editor.WriteMessage(vbLf & "Add tab...")
            Dim ribbonControl = ComponentManager.Ribbon
            If ribbonControl IsNot Nothing And ribbonTab IsNot Nothing Then
                If Equals(ribbonTab.Name, Nothing) Then
                    Throw New Exception("Ribbon Tab does not have a name. Please add a name to the ribbon tab.")
                End If
                Dim tabName = ribbonTab.Name
                doc.Editor.WriteMessage(vbLf & tabName)
                Dim doesTabAlreadyExist = ribbonControl.FindTab(tabName)
                If doesTabAlreadyExist IsNot Nothing Then
                    ribbonControl.Tabs.Remove(doesTabAlreadyExist)
                End If
                ribbonControl.Tabs.Add(ribbonTab)
                ribbonTab.IsActive = True
            End If
        End Sub
    End Class
End Namespace
