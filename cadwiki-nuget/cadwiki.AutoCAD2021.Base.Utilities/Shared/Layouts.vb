Option Explicit On
Option Strict On
Option Infer Off

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Class Layouts
    Public Shared Function CreateTab(doc As Document, originalTabName As String) As ObjectId
        Dim actualTabName As String = originalTabName
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()

                Dim existingId As ObjectId = LayoutManager.Current.GetLayoutId(actualTabName)
                Dim count As Integer = 1
                While (Not existingId.IsNull)
                    actualTabName = originalTabName + "-" + count.ToString
                    existingId = LayoutManager.Current.GetLayoutId(actualTabName)
                    count += 1
                End While
                Dim id As ObjectId = LayoutManager.Current.CreateLayout(actualTabName)
                Dim dbObj As DBObject = t.GetObject(id, OpenMode.ForWrite)
                Dim layout As Layout = CType(dbObj, Layout)
                LayoutManager.Current.CurrentLayout = layout.LayoutName
                't.AddNewlyCreatedDBObject(dbObj, True)
                t.Commit()
                Return id
            End Using
        End Using

        Return Nothing
    End Function

    Public Shared Sub SetCurrentTab(doc As Document, tab As String)
        Dim db As Database = doc.Database
        Dim number2 As Integer = db.TransactionManager.NumberOfActiveTransactions()
        Using lock As DocumentLock = doc.LockDocument()
            Using tm As Transaction = db.TransactionManager.StartTransaction()
                Dim existingId As ObjectId = LayoutManager.Current.GetLayoutId(tab)
                If Not existingId.IsNull Then
                    LayoutManager.Current.CurrentLayout = tab
                    tm.Commit()
                End If

            End Using
        End Using
    End Sub
End Class

