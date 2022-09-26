Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Windows.Automation
Imports System.Windows.Automation.AutomationElement
Imports Microsoft.Test.Input

Namespace TestEvidence
    Public Class Creator


        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
        Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, <Out> ByRef lpRect As RECT) As Boolean
        End Function

        <DllImport("user32.dll")>
        Private Shared Function PrintWindow(ByVal hWnd As IntPtr, ByVal hdcBlt As IntPtr, ByVal nFlags As Integer) As Boolean
        End Function


        Public Shared Sub PrintWindowToImage(ByVal windowIntPtr As IntPtr, ByVal screenshotPath As String, ByVal format As ImageFormat)
            Dim screenshot As Bitmap = PrintWindow(windowIntPtr)
            screenshot.Save(screenshotPath, format)
        End Sub

        Public Shared Function PrintWindow(ByVal hwnd As IntPtr) As Bitmap
            Dim rc As RECT
            GetWindowRect(hwnd, rc)
            Dim bmp As Bitmap = New Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb)
            Dim gfxBmp As Graphics = Graphics.FromImage(bmp)
            Dim hdcBitmap As IntPtr = gfxBmp.GetHdc()
            PrintWindow(hwnd, hdcBitmap, 0)
            gfxBmp.ReleaseHdc(hdcBitmap)
            gfxBmp.Dispose()
            Return bmp
        End Function

        Public Function ProcessesGetHandleFromUiTitle(ByVal wName As String) As IntPtr
            Dim hWnd As IntPtr = IntPtr.Zero

            For Each pList As Process In Process.GetProcesses()

                If pList.MainWindowTitle.Contains(wName) Then
                    hWnd = pList.MainWindowHandle
                End If
            Next

            Return hWnd
        End Function


        Public Function MicrosoftTestClickUiControl(windowIntPtr As IntPtr, controlName As String) As Boolean

            Dim element As AutomationElement = GetElementByControlName(windowIntPtr, controlName)
            If (element Is Nothing) Then
                Return False
            End If
            Dim clickableSystemDrawingPoint As System.Drawing.Point =
                GetClickableSystemDrawingPointFromElement(element)

            Return MicrosoftTestClickPoint(clickableSystemDrawingPoint)
        End Function

        Private Function GetElementByControlName(ByVal windowIntPtr As IntPtr, ByVal controlNameToFind As String) As AutomationElement
            Dim root = AutomationElement.FromHandle(windowIntPtr)
            Dim elementCollection As AutomationElementCollection = root.FindAll(TreeScope.Subtree, System.Windows.Automation.Condition.TrueCondition)

            For Each element As AutomationElement In elementCollection
                Dim current As AutomationElementInformation = element.Current
                Dim controlType As ControlType = current.ControlType
                Dim controlText As String = current.Name
                Dim controlName As String = current.AutomationId

                If controlName.Equals(controlNameToFind) Then
                    Return element
                End If
            Next

            Return Nothing
        End Function


        Private Function GetClickableSystemDrawingPointFromElement(ByVal element As AutomationElement) As System.Drawing.Point
            Dim windowsPoint As System.Windows.Point = element.GetClickablePoint()
            Dim drawingPoint As System.Drawing.Point = New System.Drawing.Point(CInt(windowsPoint.X), CInt(windowsPoint.Y))
            Return drawingPoint
        End Function

        Private Function MicrosoftTestClickPoint(ByVal clickableSystemDrawingPoint As System.Drawing.Point) As Boolean
            Microsoft.Test.Input.Mouse.MoveTo(clickableSystemDrawingPoint)
            Microsoft.Test.Input.Mouse.Click(MouseButton.Left)
            Return True
        End Function

    End Class











    Public Class RECT
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

        Public Property Location As System.Drawing.Point
            Get
                Return New System.Drawing.Point(Left, Top)
            End Get
            Set(ByVal value As System.Drawing.Point)
                _Left = value.X
                _Top = value.Y
            End Set
        End Property

        Public Property Size As System.Drawing.Size
            Get
                Return New System.Drawing.Size(Width, Height)
            End Get
            Set(ByVal value As System.Drawing.Size)
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
            Return "{Left: " & _Left & "; " & "Top: " + _Top & "; Right: " + _Right & "; Bottom: " + _Bottom & "}"
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return ToString().GetHashCode()
        End Function

        Public Overloads Function Equals(ByVal Rectangle As RECT) As Boolean
            Return Rectangle.Left = _Left AndAlso Rectangle.Top = _Top AndAlso Rectangle.Right = _Right AndAlso Rectangle.Bottom = _Bottom
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If TypeOf obj Is RECT Then
                Return Equals(CType(obj, RECT))
            ElseIf TypeOf obj Is Rectangle Then
                Return Equals(New RECT(CType(obj, Rectangle)))
            End If

            Return False
        End Function

    End Class

End Namespace

