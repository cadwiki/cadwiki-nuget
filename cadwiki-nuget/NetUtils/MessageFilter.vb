Option Strict On
Option Explicit On
Option Infer Off
Imports System.Runtime.InteropServices
Public Class MessageFilter
    Implements IOleMessageFilter
    <DllImport("Ole32.dll")>
    Private Shared Function CoRegisterMessageFilter(ByVal newFilter As IOleMessageFilter, ByRef oldFilter As IOleMessageFilter) As Integer

    End Function
    Public Shared Sub Register()
        Dim newFilter As IOleMessageFilter = New MessageFilter()
        Dim oldFilter As IOleMessageFilter = Nothing
        CoRegisterMessageFilter(newFilter, oldFilter)
    End Sub
    Public Shared Sub Revoke()
        Dim oldFilter As IOleMessageFilter = Nothing
        CoRegisterMessageFilter(Nothing, oldFilter)
    End Sub

    Public Function HandleInComingCall(dwCallType As Integer, hTaskCaller As IntPtr, dwTickCount As Integer, lpInterfaceInfo As IntPtr) As Integer Implements IOleMessageFilter.HandleInComingCall
        Return 0
    End Function

    Public Function RetryRejectedCall(hTaskCallee As IntPtr, dwTickCount As Integer, dwRejectType As Integer) As Integer Implements IOleMessageFilter.RetryRejectedCall
        If dwRejectType = 2 Then
            ' flag = SERVERCALL_RETRYLATER.

            ' Retry the thread call immediately if return >=0 & 
            ' <100.
            Return 99
        End If
        ' Too busy; cancel call.
        Return -1
    End Function

    Public Function MessagePending(hTaskCallee As IntPtr, dwTickCount As Integer, dwPendingType As Integer) As Integer Implements IOleMessageFilter.MessagePending
        Return 2
    End Function
End Class
<ComImport(), Guid("00000016-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
Interface IOleMessageFilter
    <PreserveSig>
    Function HandleInComingCall(ByVal dwCallType As Integer, ByVal hTaskCaller As IntPtr, ByVal dwTickCount As Integer, ByVal lpInterfaceInfo As IntPtr) As Integer
    <PreserveSig>
    Function RetryRejectedCall(ByVal hTaskCallee As IntPtr, ByVal dwTickCount As Integer, ByVal dwRejectType As Integer) As Integer
    <PreserveSig>
    Function MessagePending(ByVal hTaskCallee As IntPtr, ByVal dwTickCount As Integer, ByVal dwPendingType As Integer) As Integer
End Interface