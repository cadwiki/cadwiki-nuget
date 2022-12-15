Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput

Public Class SelectionFilters
    Private Shared startOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "<or")
    Private Shared endOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "or>")
    Private Shared startAnd As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "<and")
    Private Shared endAnd As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "and>")

    Public Shared Function GetAllLineEntitiesOnLayers(layers As List(Of String)) As SelectionFilter

        Dim typedValues As New List(Of TypedValue)

        typedValues.Add(startAnd)
        typedValues.Add(startOr)
        For Each layerName As String In layers
            typedValues.Add(New TypedValue(8, layerName))
        Next
        typedValues.Add(endOr)
        Dim lines As TypedValue = New TypedValue(0, "LINE")
        typedValues.Add(lines)
        typedValues.Add(endAnd)
        Dim filter As SelectionFilter = New SelectionFilter(typedValues.ToArray)
        Return filter
    End Function

    Public Shared Function GetAllLineEntities() As SelectionFilter
        Dim lines As TypedValue = New TypedValue(0, "LINE")
        Dim typedValues As TypedValue() = {lines}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function

    Public Shared Function GetAllLineEntitiesOnLayer(layerNameStr As String) As SelectionFilter
        Dim lwPolyLine As TypedValue = New TypedValue(0, "LINE")
        Dim layerName As TypedValue = New TypedValue(8, layerNameStr)
        Dim typedValues As TypedValue() = {lwPolyLine, layerName}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function

    Public Shared Function GetAllLineBasedEntitiesOnLayer(layerName As String) As SelectionFilter
        Dim startAnd As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "<and")
        Dim startOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "<or")
        Dim line As TypedValue = New TypedValue(0, "LINE")
        Dim polyLine As TypedValue = New TypedValue(0, "POLYLINE")
        Dim arc As TypedValue = New TypedValue(0, "ARC")
        Dim lwPolyLine As TypedValue = New TypedValue(0, "LWPOLYLINE")
        Dim endOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "or>")
        Dim layer As TypedValue = New TypedValue(8, layerName)
        Dim endAnd As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "and>")
        Dim typedValues As TypedValue() = {startAnd, startOr, line, polyLine, arc, lwPolyLine, endOr, layer, endAnd}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function

    Public Shared Function GetAllLineBasedEntities() As SelectionFilter
        Dim startOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "<or")
        Dim line As TypedValue = New TypedValue(0, "LINE")
        Dim polyLine As TypedValue = New TypedValue(0, "POLYLINE")
        Dim arc As TypedValue = New TypedValue(0, "ARC")
        Dim lwPolyLine As TypedValue = New TypedValue(0, "LWPOLYLINE")
        Dim endOr As TypedValue = New TypedValue(CType(DxfCode.Operator, Integer), "or>")
        Dim typedValues As TypedValue() = {startOr, line, polyLine, arc, lwPolyLine, endOr}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function

    Public Shared Function LwPoyllinesByLayer(layerNameStr As String) As SelectionFilter
        Dim lwPolyLine As TypedValue = New TypedValue(0, "LWPOLYLINE")
        Dim layerName As TypedValue = New TypedValue(8, layerNameStr)

        Dim typedValues As TypedValue() = {lwPolyLine, layerName}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function

    Public Shared Function BlocksByName(blockNameStr As String) As SelectionFilter
        Dim blockInsert As TypedValue = New TypedValue(0, "INSERT")
        Dim blockName As TypedValue = New TypedValue(2, blockNameStr)

        Dim typedValues As TypedValue() = {blockInsert, blockName}
        Dim filter As SelectionFilter = New SelectionFilter(typedValues)
        Return filter
    End Function
End Class

