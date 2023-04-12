

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

Namespace Workflows
    Public Class BreakSs

        Public Class UserInput
            Public LayerNameToBreak As String
        End Class

        Public Class Result

        End Class

        Public Class BreakSsInputs
            Public SelectionToBreak As SelectionSet
            Public SelectionToBreakWith As SelectionSet
            Public Self As Boolean
            Public Gap As Double
            Public NewLayer As String = "ss-broken-at-intersection-points"
            Public DeleteOriginal As Boolean
        End Class

        Public Class BreakList
            Public ObjectidToBreakPointList As New Dictionary(Of ObjectId, List(Of Point3d))
        End Class


        Public Shared Function BreakSsWithSs(doc As Document, inputs As BreakSsInputs) As List(Of ObjectId)
            Dim intersectionPoints As Point3dCollection = New Point3dCollection()
            Dim breakList As New BreakList()
            Dim newIds As New List(Of ObjectId)
            Dim db As Database = doc.Database
            Using lock As DocumentLock = doc.LockDocument
                Using t As Transaction = db.TransactionManager.StartTransaction()
                    Dim ltr As LayerTableRecord = Layers.GetLayer(doc, inputs.NewLayer)
                    If ltr Is Nothing Then
                        ltr = Layers.CreateFirstAvailableLayerName(doc, inputs.NewLayer)
                    End If
                    ltr.UpgradeOpen()
                    ltr.Color = Color.FromColorIndex(ColorMethod.ByLayer, 4)
                    For Each objIdToBreak As ObjectId In inputs.SelectionToBreak.GetObjectIds()
                        Dim entityBreakPoints As New List(Of Point3d)
                        Dim entityToBreak As Entity = CType(t.GetObject(objIdToBreak, OpenMode.ForWrite), Entity)
                        For Each objIdToBreakWith As ObjectId In inputs.SelectionToBreakWith.GetObjectIds()
                            Dim entityToBreakWith As Entity = CType(t.GetObject(objIdToBreakWith, OpenMode.ForWrite), Entity)
                            intersectionPoints = New Point3dCollection()
                            If inputs.Self Or Not objIdToBreak = objIdToBreakWith Then
                                entityToBreak.IntersectWith(entityToBreakWith, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero, IntPtr.Zero)
                            End If
                            If intersectionPoints.Count > 0 Then
                                entityBreakPoints.AddRange(intersectionPoints.Cast(Of Point3d).ToList)
                            End If
                        Next
                        breakList.ObjectidToBreakPointList.Add(objIdToBreak, entityBreakPoints)
                    Next
                    For Each pair As KeyValuePair(Of ObjectId, List(Of Point3d)) In breakList.ObjectidToBreakPointList
                        Dim objectId As ObjectId = pair.Key
                        Dim lineToBreak As Line = CType(t.GetObject(objectId, OpenMode.ForWrite), Line)
                        Dim breakPointList As List(Of Point3d) = pair.Value

                        Dim sortedBreakPoints As List(Of Point3d) = Points.SortPointsByProximityToPoint(breakPointList, lineToBreak.StartPoint)
                        Dim nearSide As Point3d = lineToBreak.StartPoint
                        Dim farSide As Point3d = sortedBreakPoints(0)
                        Dim curSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)
                        Dim newLine As Line
                        For Each point As Point3d In sortedBreakPoints
                            farSide = point
                            newLine = New Line(nearSide, farSide)
                            newLine.Layer = inputs.NewLayer
                            nearSide = farSide
                            curSpace.AppendEntity(newLine)
                            t.AddNewlyCreatedDBObject(newLine, True)
                        Next

                        farSide = lineToBreak.EndPoint
                        newLine = New Line(nearSide, farSide)
                        newLine.Layer = inputs.NewLayer
                        nearSide = farSide
                        Dim newId As ObjectId = curSpace.AppendEntity(newLine)
                        newIds.Add(newId)
                        t.AddNewlyCreatedDBObject(newLine, True)
                        newIds.Add(newId)
                        If inputs.DeleteOriginal Then
                            lineToBreak.Erase()
                        End If

                    Next
                    t.Commit()
                End Using
            End Using
            Return newIds
        End Function

        Public Shared Function Run() As Result
            Try
                Dim doc As Document = Application.DocumentManager.MdiActiveDocument
                Dim userInput As UserInput = GetUserInput()
                If userInput Is Nothing Then
                    'Commands.AutoCADExceptions.WriteInvalidUserInputToCommandLine()
                Else
                    Return RunWithUserInput(userInput)
                End If
            Catch ex As Exception
                'Commands.AutoCADExceptions.Handle(ex)
            End Try
            Return Nothing
        End Function

        Private Shared Function GetUserInput() As UserInput
            Dim doc As Document = Application.DocumentManager.MdiActiveDocument
            Dim promptEntityOptions As New PromptEntityOptions("Select entity on layer to break: " + vbLf)
            Dim objectId As ObjectId = Utilities.UserInput.GetEntityFromUser(doc, promptEntityOptions)
            Dim db As Database = doc.Database
            Using lock As DocumentLock = doc.LockDocument
                Using t As Transaction = db.TransactionManager.StartTransaction()
                    Dim entityOnLayer As Entity = CType(t.GetObject(objectId, OpenMode.ForWrite), Entity)
                    Dim layer As String = entityOnLayer.Layer
                    Dim userInput As New UserInput
                    userInput.LayerNameToBreak = layer
                    Return userInput
                End Using
            End Using
            Return Nothing
        End Function

        Public Shared Function RunWithUserInput(userInput As UserInput) As Result
            Try
                Dim acadApp As Object = Application.AcadApplication
                Dim doc As Document = Application.DocumentManager.MdiActiveDocument
                Dim filter As SelectionFilter = SelectionFilters.GetAllLineBasedEntitiesOnLayer(userInput.LayerNameToBreak)
                Dim selectionToBreak As SelectionSet = SelectionSets.SelectAll(doc, filter)
                Dim inputs As New BreakSsInputs
                inputs.SelectionToBreak = selectionToBreak
                inputs.SelectionToBreakWith = selectionToBreak
                inputs.Self = True
                BreakSs.BreakSsWithSs(doc, inputs)
                Dim result As Result = New Result()
                Return result
            Catch ex As Exception
                'AutoCADExceptions.Handle(ex)
            End Try

            Return Nothing

        End Function








    End Class

End Namespace