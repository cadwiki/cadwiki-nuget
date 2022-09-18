Option Strict On
Option Infer Off
Option Explicit On

Imports System.Windows

Namespace Templates
    Public Class WindowAutoCADException
        Inherits CustomWindowTemplate
        Public WasOkayClicked As Boolean

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub New(ex As Exception)
            InitializeComponent()
            TextBoxMessage.Text = ex.Message
            TextBoxStackTrace.Text = ex.StackTrace
        End Sub

        Private Sub ButtonOk_Click(sender As Object, e As RoutedEventArgs)
            WasOkayClicked = True
            Close()
        End Sub

        Private Sub ButtonCancel_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub
    End Class
End Namespace

