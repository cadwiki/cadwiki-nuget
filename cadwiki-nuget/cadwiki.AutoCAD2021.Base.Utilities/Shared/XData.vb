Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput

Public Class XData
    Public Shared Function GetXDataByRegAppAndKey(doc As Document,
        objId As ObjectId,
        regApp As String,
        xDataKey As String) As XDataFound
        Dim db As Database = doc.Database
        Dim xDataNotFound As New XDataFound
        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForRead, False), Entity)
                Dim handle As String = entity.Handle.ToString()
                Dim rb As ResultBuffer = entity.XData
                If (rb IsNot Nothing) Then
                    Dim found As XDataFound = GetXDataValueFromBuffer(rb, regApp, xDataKey)
                    Return found
                End If
                t.Commit()
            End Using
        End Using
        Return xDataNotFound
    End Function

    Public Class XDataFound
        Public Index As Integer = -1
        Public Value As String = ""
    End Class

    Public Shared Function GetXDataValueFromBuffer(rb As ResultBuffer, regApp As String, xDataKey As String) _
        As XDataFound
        Dim tvArray As TypedValue() = rb.AsArray
        Dim hasTagBeenFound As Boolean = False
        Dim xDataTag As String = ""
        Dim xDataValue As String = ""
        Dim i As Integer = 0
        Dim found As New XDataFound
        Dim xDataNotFound As New XDataFound
        For Each tv As TypedValue In tvArray
            Dim code As Short = tv.TypeCode
            Select Case code
                Case CType(DxfCode.ExtendedDataRegAppName, Short)
                    Dim appName As String = tv.Value.ToString()
                    If Not appName.Equals(regApp) Then
                        Return xDataNotFound
                    End If
                    ' "{" or "}" are stored as control strings
                Case CType(DxfCode.ExtendedDataControlString, Short)
                    Dim controlString As String = tv.Value.ToString()
                    ' Tags and Values are stored as strings
                Case CType(DxfCode.ExtendedDataAsciiString, Short)
                    If (hasTagBeenFound = False) Then
                        xDataTag = tv.Value.ToString()
                        hasTagBeenFound = True
                    Else
                        xDataValue = tv.Value.ToString()
                        If xDataTag.Equals(xDataKey) Then
                            found.Index = i
                            found.Value = xDataValue
                            Return found
                        End If
                        xDataTag = ""
                        xDataValue = ""
                        hasTagBeenFound = False
                    End If

            End Select
            i = i + 1
        Next
        Return xDataNotFound
    End Function

    Public Shared Sub AddRegAppTableRecord(regAppName As String)
        Dim doc As Document = Application.DocumentManager.MdiActiveDocument

        Dim db As Database = doc.Database

        Using lock As DocumentLock = doc.LockDocument()
            Using t As Transaction = db.TransactionManager.StartTransaction()
                Dim rat As RegAppTable = CType(t.GetObject(db.RegAppTableId, OpenMode.ForRead, False), RegAppTable)
                If Not rat.Has(regAppName) Then
                    rat.UpgradeOpen()
                    Dim ratr As RegAppTableRecord = New RegAppTableRecord()
                    ratr.Name = regAppName
                    rat.Add(ratr)
                    t.AddNewlyCreatedDBObject(ratr, True)
                End If
                t.Commit()
            End Using

        End Using
    End Sub

    Public Shared Function StringValue(appKey As String, dataKey As String, dataValue As String) As ResultBuffer
        Return _
            New ResultBuffer(New TypedValue(1001, appKey),
                New TypedValue(1002, "{"),
                New TypedValue(1000, dataKey),
                New TypedValue(1000, dataValue),
                New TypedValue(1002, "}"))
    End Function

    Public Shared Function CreateAppKey(appKey As String) As ResultBuffer
        Return New ResultBuffer(New TypedValue(1001, appKey))
    End Function


    Public Shared Function CreateStringKeyAndValue(dataKey As String, dataValue As String) As ResultBuffer
        Return _
            New ResultBuffer(New TypedValue(1002, "{"),
                New TypedValue(1000, dataKey),
                New TypedValue(1000, dataValue),
                New TypedValue(1002, "}"))
    End Function

    Public Shared Function CreateTypedValues(dataKey As String, dataValue As String) As TypedValue()
        Dim typesValues() As TypedValue =
            {New TypedValue(1002, "{"), New TypedValue(1000, dataKey), New TypedValue(1000, dataValue),
                New TypedValue(1002, "}")}

        Return typesValues
    End Function

    Public Shared Function AddTypedValues(xDataBuffer As ResultBuffer, typedValues() As TypedValue) As ResultBuffer
        For Each tv As TypedValue In typedValues
            xDataBuffer.Add(tv)
        Next
        Return xDataBuffer
    End Function

    Public Shared Function RemovedTypeValue(xDataBuffer As ResultBuffer, regApp As String, xDataKey As String) _
        As ResultBuffer
        Dim found As XDataFound = GetXDataValueFromBuffer(xDataBuffer, regApp, xDataKey)
        Dim newBuffer As New ResultBuffer()
        If found IsNot Nothing Then
            If Not found.Index = -1 Then
                Dim tvArray As TypedValue() = xDataBuffer.AsArray
                Dim i As Integer = 0
                For Each tv As TypedValue In tvArray
                    If _
                        Not i = found.Index And Not i + 1 = found.Index And Not i - 1 = found.Index And
                            Not i + 2 = found.Index Then
                        newBuffer.Add(tv)
                    End If
                    i = i + 1
                Next
                Return newBuffer
            End If
        End If
        Return xDataBuffer
    End Function

    Public Shared Function GetSsOfMatchingXdata(doc As Document,
        ss As SelectionSet,
        xDataRegAppKey As String,
        xDataKey As String,
        xdataValue As String) As List(Of ObjectId)
        Try
            Dim matchingXDataObjectIdList As New List(Of ObjectId)
            Dim db As Database = doc.Database
            Using lock As DocumentLock = doc.LockDocument()
                Using t As Transaction = db.TransactionManager.StartTransaction()
                    Dim currentSpace As BlockTableRecord = CType(t.GetObject(db.CurrentSpaceId, OpenMode.ForWrite),
                        BlockTableRecord)
                    For Each objId As ObjectId In ss.GetObjectIds
                        Dim entity As Entity = CType(t.GetObject(objId, OpenMode.ForWrite), Entity)
                        Dim existingXData As ResultBuffer = entity.XData
                        Dim xdataFound As XDataFound = GetXDataValueFromBuffer(existingXData, xDataRegAppKey, xDataKey)
                        If xdataFound IsNot Nothing Then
                            If Not xdataFound.Index = -1 Then
                                If xdataFound.Value = xdataValue Then
                                    matchingXDataObjectIdList.Add(entity.Id)
                                End If
                            End If
                        End If
                    Next
                    t.Commit()
                End Using
            End Using
            Return matchingXDataObjectIdList
        Catch ex As Exception
            Throw New Exception("GetMatchingXdata error: " + ex.Message)
        End Try
        Return Nothing
    End Function

    Public Shared Function SetXdataValue(doc As Document,
        existingBuffer As ResultBuffer,
        xDataRegAppKey As String,
        xDataKey As String,
        xdataValue As String) As ResultBuffer
        Dim existingDataEntry As String =
            XData.GetXDataValueFromBuffer(existingBuffer, xDataRegAppKey, xDataKey).Value
        Dim newKeyAndValue() As TypedValue = XData.CreateTypedValues(xDataKey, xdataValue)
        Dim newBuffer As ResultBuffer = existingBuffer
        If existingDataEntry Is Nothing Then
            newBuffer = XData.AddTypedValues(newBuffer, newKeyAndValue)
        Else
            newBuffer = XData.RemovedTypeValue(newBuffer, xDataRegAppKey, xDataKey)
            newBuffer = XData.AddTypedValues(newBuffer, newKeyAndValue)
        End If
        Return newBuffer
    End Function
End Class
