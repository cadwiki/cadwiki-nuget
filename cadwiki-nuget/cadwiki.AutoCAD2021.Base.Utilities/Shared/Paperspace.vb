Option Explicit On
Option Strict On
Option Infer Off

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.GraphicsSystem

Public Class Paperspace
    Public Shared Function CreateViewPort(doc As Document) As ObjectId
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim bt As BlockTable = CType(t.GetObject(db.BlockTableId, OpenMode.ForRead), BlockTable)
                Dim rec As BlockTableRecord = CType(t.GetObject(bt(BlockTableRecord.PaperSpace), OpenMode.ForWrite),
                    BlockTableRecord)

                Application.SetSystemVariable("TILEMODE", 0)
                doc.Editor.SwitchToPaperSpace()

                Dim vp As Viewport = New Viewport()
                vp.SetDatabaseDefaults()
                vp.CenterPoint = New Point3d(3.25, 3, 0)
                vp.Width = 6
                vp.Height = 5

                Dim objectId As ObjectId = rec.AppendEntity(vp)
                t.AddNewlyCreatedDBObject(vp, True)
                vp.On = True

                t.Commit()
                Return objectId
            End Using
        End Using


        Return Nothing
    End Function


    Private Shared Sub ZoomCurrentViewPort(doc As Document, lowerLeft As Point3d, upperRight As Point3d)
        Dim cvId As Integer = Convert.ToInt32((Application.GetSystemVariable("CVPORT")))
        Using gm As Manager = doc.GraphicsManager
            Using vw As View = gm.GetCurrentAcGsView(cvId)
                Dim currentTab As String = CStr(Application.GetSystemVariable("CTAB"))
                Layouts.SetCurrentTab(doc, "Model")
                vw.ZoomExtents(lowerLeft, upperRight)
                vw.Zoom(0.95)
                'SetCurrentLayoutTab(currentTab)
                'gm.SetViewportFromView(cvId, vw, True, True, False)
            End Using
        End Using
    End Sub
End Class
