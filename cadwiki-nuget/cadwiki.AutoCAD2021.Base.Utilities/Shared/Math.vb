Option Strict On
Option Infer Off
Option Explicit On

Public Class MathUtils
    Public Shared Function RadiansToDegree(radians As Double) As Double
        Dim degrees As Double = 180 * (radians / Math.PI)
        Return degrees
    End Function

    Public Shared Function DegreesToRadians(degrees As Double) As Double
        Dim radians As Double = degrees * (Math.PI / 180)
        Return radians
    End Function


    Public Shared Function AreDoublesEqual(num1 As Double, num2 As Double, fuzz As Double) As Boolean

        Dim dRet As Double = num1 - num2
        Dim bool As Boolean

        If (Math.Abs(dRet) < fuzz) Then
            bool = True
        Else
            bool = False
        End If

        Return bool
    End Function
End Class