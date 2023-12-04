Imports System.IO
Imports NLog
Imports NUnit.Framework

Namespace RozeComputing.Common.Services.Logging.Tests

    Public Class FileTargetTests

        Private mLoggingService As New LoggingService()

        Private Const mTestLoggingName = "TestLoggingName"
        <SetUp>
        Public Sub Setup()
            mLoggingService = New LoggingService(mTestLoggingName)
        End Sub

        <Test>
        Public Sub AddFileTarget()
            mLoggingService.AddFileTarget(Environment.CurrentDirectory & "\TestFileName.log", "TestTargetName")

            mLoggingService.Logger.Log(LogLevel.Info, "AddFileTarget")

            If (File.Exists(Environment.CurrentDirectory & "\TestFileName.log")) = False Then
                Assert.Fail("TestFileName not created")
            End If
        End Sub

    End Class

End Namespace