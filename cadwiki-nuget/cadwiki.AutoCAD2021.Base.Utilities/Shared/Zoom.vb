Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry

Public Class Zoom
    Public Shared Sub Window(doc As Document, min As Point3d, max As Point3d)
        Using lock As DocumentLock = doc.LockDocument
            Using tr As Transaction = doc.TransactionManager.StartTransaction()

                Dim min2d As Point2d = New Point2d(min.X, min.Y)
                Dim max2d As Point2d = New Point2d(max.X, max.Y)

                Dim view As ViewTableRecord = New ViewTableRecord()

                view.CenterPoint = min2d + ((max2d - min2d) / 2.0)
                view.Height = max2d.Y - min2d.Y
                view.Width = max2d.X - min2d.X
                doc.Editor.SetCurrentView(view)
                tr.Commit()
            End Using
        End Using
    End Sub

    Public Shared Sub Extents(doc As Document)
        Using lock As DocumentLock = doc.LockDocument
            Using tr As Transaction = doc.TransactionManager.StartTransaction()

                Dim db As Database = doc.Database

                Dim min As Point3d = db.Extmin
                Dim max As Point3d = db.Extmax

                Dim min2d As Point2d = New Point2d(min.X, min.Y)
                Dim max2d As Point2d = New Point2d(max.X, max.Y)

                Dim view As ViewTableRecord = New ViewTableRecord()

                view.CenterPoint = min2d + ((max2d - min2d) / 2.0)
                view.Height = max2d.Y - min2d.Y
                view.Width = max2d.X - min2d.X
                doc.Editor.SetCurrentView(view)
                tr.Commit()
            End Using
        End Using
    End Sub

    Public Class ZoomCoordinates
        Public LowerLeft As Point2d
        Public UpperRight As Point2d

        Sub New(lowerLeftPt As Point2d, upperRightPt As Point2d)
            LowerLeft = lowerLeftPt
            UpperRight = upperRightPt
        End Sub
    End Class

    Public Shared Function GetApplicationViewZoomCoordinates() As ZoomCoordinates
        Dim sysVarViewCenter As Point3d = CType(Application.GetSystemVariable("VIEWCTR"), Point3d)
        Dim centerX As Double = sysVarViewCenter.X
        Dim centerY As Double = sysVarViewCenter.Y

        Dim sysVarViewResolution As Point2d = CType(Application.GetSystemVariable("SCREENSIZE"), Point2d)
        Dim viewResolution0 As Double = sysVarViewResolution.X
        Dim viewResolution1 As Double = sysVarViewResolution.Y


        Dim sysVarViewHeight As Double = CType(Application.GetSystemVariable("VIEWSIZE"), Double)
        Dim viewWidth As Double = sysVarViewHeight * (viewResolution0 / viewResolution1)

        Dim minX As Double = centerX - (viewWidth / 2)
        Dim minY As Double = centerY - (sysVarViewHeight / 2)
        Dim maxX As Double = centerX + (viewWidth / 2)
        Dim maxY As Double = centerY + (sysVarViewHeight / 2)


        Dim lowerLeft As Point2d = New Point2d(minX, minY)
        Dim upperRight As Point2d = New Point2d(maxX, maxY)

        Dim zc As ZoomCoordinates = New ZoomCoordinates(lowerLeft, upperRight)
        Return zc
    End Function

    Public Shared Sub RestoreZoomCoordinates(doc As Document, zoomCoordinates As ZoomCoordinates)
        If zoomCoordinates IsNot Nothing Then
            Dim pt1 As Point3d = New Point3d(zoomCoordinates.LowerLeft.X, zoomCoordinates.LowerLeft.Y, 0)
            Dim pt2 As Point3d = New Point3d(zoomCoordinates.UpperRight.X, zoomCoordinates.UpperRight.Y, 0)
            Zoom.Window(doc, pt1, pt2)
        End If
    End Sub
End Class

