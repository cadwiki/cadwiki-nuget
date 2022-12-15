Option Strict On
Option Infer Off
Option Explicit On

Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices

Public Class Styles
    Public Class TextStyleProps
        Public FontFilePath As String
        Public HeightInMm As Double
    End Class

    Public Class DimStyleProps
        Public TextStyleName As String
        Public PlaceholderValueInMm As Double
        Public DimCLRD As Short
        Public DimCLRE As Short
        Public DimCLRT As Short
        Public DimEXO As Double
        Public DimASZ As Double
        Public DimCEN As Double
        Public DimTVP As Double
        Public DimDEC As Integer
        Public DimLUNIT As Integer
    End Class


    Public Shared Sub CreateTextStyle(doc As Document, styleName As String, props As TextStyleProps)
        Dim db As Database = doc.Database

        Using lock As DocumentLock = doc.LockDocument()
            Using tm As Transaction = db.TransactionManager.StartTransaction()

                Dim st As TextStyleTable = CType(tm.GetObject(db.TextStyleTableId, OpenMode.ForWrite, False),
                    TextStyleTable)
                Dim str As TextStyleTableRecord = New TextStyleTableRecord()

                Dim doesStyleExist As Boolean = DoesTextStyleExist(doc, styleName)
                If doesStyleExist Then
                    Dim oId As ObjectId = st.Item(styleName)
                    str = CType(tm.GetObject(oId, OpenMode.ForWrite, False), TextStyleTableRecord)
                End If

                str.FileName = props.FontFilePath

                If doesStyleExist = False Then
                    str.Name = styleName
                    st.Add(str)
                    tm.AddNewlyCreatedDBObject(str, True)
                End If

                db.Textstyle = str.ObjectId
                tm.Commit()
            End Using
        End Using
    End Sub

    Public Shared Function DoesTextStyleExist(doc As Document, styleName As String) As Boolean
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()

                Dim dbObject As DBObject = transaction.GetObject(db.TextStyleTableId, OpenMode.ForWrite)
                Dim textStyleTable As TextStyleTable = CType(dbObject, TextStyleTable)

                Dim textStyleRecord As TextStyleTableRecord = New TextStyleTableRecord()

                Dim doesStyleExist As Boolean = textStyleTable.Has(styleName)
                If doesStyleExist Then
                    transaction.Commit()
                    Return True
                End If
                transaction.Commit()
            End Using
        End Using
        Return False
    End Function


    Public Shared Sub CreateDimensionStyle(doc As Document, styleName As String, props As DimStyleProps)
        Dim db As Database = doc.Database

        Using lock As DocumentLock = doc.LockDocument()
            Using tm As Transaction = db.TransactionManager.StartTransaction()

                Dim st As DimStyleTable = CType(tm.GetObject(db.DimStyleTableId, OpenMode.ForWrite, False),
                    DimStyleTable)
                Dim str As DimStyleTableRecord = New DimStyleTableRecord()

                Dim doesStyleExist As Boolean = DoesDimStyleExist(doc, styleName)
                If doesStyleExist Then
                    Dim oId As ObjectId = st.Item(styleName)
                    str = CType(tm.GetObject(oId, OpenMode.ForWrite, False), DimStyleTableRecord)
                End If

                Dim objectId As ObjectId = GetTextStyleId(doc, props.TextStyleName)

                str.Dimtxsty = objectId
                str.Dimclrd = Color.FromColorIndex(ColorMethod.ByAci, CType(props.DimCLRD, Short))
                str.Dimclre = Color.FromColorIndex(ColorMethod.ByAci, CType(props.DimCLRE, Short))
                str.Dimclrt = Color.FromColorIndex(ColorMethod.ByAci, CType(props.DimCLRT, Short))
                str.Dimexo = props.DimEXO
                str.Dimasz = props.DimASZ
                str.Dimcen = props.DimCEN
                str.Dimtvp = props.DimTVP
                str.Dimadec = props.DimDEC
                str.Dimlunit = props.DimLUNIT

                If doesStyleExist = False Then
                    str.Name = styleName
                    st.Add(str)
                    tm.AddNewlyCreatedDBObject(str, True)
                End If

                tm.Commit()
            End Using
        End Using
    End Sub

    Public Shared Function DoesDimStyleExist(doc As Document, styleName As String) As Boolean
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()

                Dim dbObject As DBObject = transaction.GetObject(db.DimStyleTableId, OpenMode.ForRead)
                Dim dimStyleTable As DimStyleTable = CType(dbObject, DimStyleTable)

                Dim dimStyleTableRecord As DimStyleTableRecord = New DimStyleTableRecord()

                Dim doesStyleExist As Boolean = dimStyleTable.Has(styleName)
                If doesStyleExist Then
                    transaction.Commit()
                    Return True
                End If


                transaction.Commit()
            End Using
        End Using
        Return False
    End Function

    Public Shared Function GetTextStyleId(doc As Document, styleName As String) As ObjectId
        Dim db As Database = doc.Database
        Using lock As DocumentLock = doc.LockDocument()
            Using transaction As Transaction = db.TransactionManager.StartTransaction()

                Dim dbObject As DBObject = transaction.GetObject(db.TextStyleTableId, OpenMode.ForRead)
                Dim textStyleTable As TextStyleTable = CType(dbObject, TextStyleTable)
                If textStyleTable.Has(styleName) Then
                    transaction.Commit()
                    Dim objectId As ObjectId = textStyleTable.Item(styleName)
                    Return objectId
                End If
                transaction.Commit()
            End Using
        End Using
        Return Nothing
    End Function
End Class


