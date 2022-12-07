Imports System.Drawing
Imports System.Runtime.InteropServices

Namespace WinAPI

    <StructLayout(LayoutKind.Sequential)>
    Public Structure RECT
        Private _Left As Integer
        Private _Top As Integer
        Private _Right As Integer
        Private _Bottom As Integer

        Public Sub New(ByVal Rectangle As RECT)
            Me.New(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
        End Sub
        Public Sub New(ByVal Left As Integer, ByVal Top As Integer, ByVal Right As Integer, ByVal Bottom As Integer)
            _Left = Left
            _Top = Top
            _Right = Right
            _Bottom = Bottom
        End Sub

        Public Property X As Integer
            Get
                Return _Left
            End Get
            Set(ByVal value As Integer)
                _Left = value
            End Set
        End Property
        Public Property Y As Integer
            Get
                Return _Top
            End Get
            Set(ByVal value As Integer)
                _Top = value
            End Set
        End Property
        Public Property Left As Integer
            Get
                Return _Left
            End Get
            Set(ByVal value As Integer)
                _Left = value
            End Set
        End Property
        Public Property Top As Integer
            Get
                Return _Top
            End Get
            Set(ByVal value As Integer)
                _Top = value
            End Set
        End Property
        Public Property Right As Integer
            Get
                Return _Right
            End Get
            Set(ByVal value As Integer)
                _Right = value
            End Set
        End Property
        Public Property Bottom As Integer
            Get
                Return _Bottom
            End Get
            Set(ByVal value As Integer)
                _Bottom = value
            End Set
        End Property
        Public Property Height As Integer
            Get
                Return _Bottom - _Top
            End Get
            Set(ByVal value As Integer)
                _Bottom = value + _Top
            End Set
        End Property
        Public Property Width As Integer
            Get
                Return _Right - _Left
            End Get
            Set(ByVal value As Integer)
                _Right = value + _Left
            End Set
        End Property
        Public Property Location As Drawing.Point
            Get
                Return New Drawing.Point(Left, Top)
            End Get
            Set(ByVal value As Drawing.Point)
                _Left = value.X
                _Top = value.Y
            End Set
        End Property
        Public Property Size As Drawing.Size
            Get
                Return New Drawing.Size(Width, Height)
            End Get
            Set(ByVal value As Drawing.Size)
                _Right = value.Width + _Left
                _Bottom = value.Height + _Top
            End Set
        End Property

        Public Shared Widening Operator CType(ByVal Rectangle As RECT) As Rectangle
            Return New Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height)
        End Operator
        Public Shared Widening Operator CType(ByVal Rectangle As Rectangle) As RECT
            Return New RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
        End Operator
        Public Shared Operator =(ByVal Rectangle1 As RECT, ByVal Rectangle2 As RECT) As Boolean
            Return Rectangle1.Equals(Rectangle2)
        End Operator
        Public Shared Operator <>(ByVal Rectangle1 As RECT, ByVal Rectangle2 As RECT) As Boolean
            Return Not Rectangle1.Equals(Rectangle2)
        End Operator

        Public Overrides Function ToString() As String
            Return "{Left: " & _Left & "; " & "Top: " & _Top & "; Right: " & _Right & "; Bottom: " & _Bottom & "}"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ToString().GetHashCode()
        End Function

        Public Overloads Function Equals(ByVal Rectangle As RECT) As Boolean
            Return Rectangle.Left = _Left AndAlso Rectangle.Top = _Top AndAlso Rectangle.Right = _Right AndAlso Rectangle.Bottom = _Bottom
        End Function

        Public Overrides Function Equals(ByVal [Object] As Object) As Boolean
            If TypeOf [Object] Is RECT Then
                Return Equals(CType([Object], RECT))
            ElseIf TypeOf [Object] Is Rectangle Then
                Return Me.Equals(New RECT(CType([Object], Rectangle)))
            End If

            Return False
        End Function
    End Structure

End Namespace

