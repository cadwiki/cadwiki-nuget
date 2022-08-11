Option Strict On
Option Infer Off
Option Explicit On

Imports System.Windows
Imports System.Windows.Media

'https://www.codeproject.com/Tips/1155345/How-to-Remove-the-Close-Button-from-a-WPF-ToolWind
Namespace Templates
    Partial Public Class CustomWindowTemplate
        Inherits Window

        Public Property _solidColorBrushLimeGreen As New SolidColorBrush

        Public Property SolidColorBrushLimeGreen() As SolidColorBrush
            Get
                Return New SolidColorBrush(CType(ColorConverter.ConvertFromString("#00FF00"), Color))
            End Get
            Set(ByVal value As SolidColorBrush)
                _solidColorBrushLimeGreen = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#00FF00"), Color))
            End Set
        End Property


        Private Const GWL_STYLE As Integer = -16
        Private Const WS_SYSMENU As Integer = &H80000

        <System.Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)>
        Private Shared Function GetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer) As Integer
        End Function

        <System.Runtime.InteropServices.DllImport("user32.dll")>
        Private Shared Function SetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) _
        As Integer
        End Function

        Public Sub New()
            'https://stackoverflow.com/questions/25468920/converting-c-sharp-to-vb-trouble-with-events-and-lambda-expression
            AddHandler Loaded, Sub(s, e) ToolWindow_Loaded(s, e)
            DataContext = Me
        End Sub

        Private Sub ToolWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim hwnd As IntPtr = New System.Windows.Interop.WindowInteropHelper(Me).Handle
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) And Not WS_SYSMENU)
        End Sub
    End Class

End Namespace

