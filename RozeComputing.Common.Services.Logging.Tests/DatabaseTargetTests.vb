Imports System.IO
Imports Microsoft.Data.SqlClient
Imports NLog
Imports NUnit.Framework

Namespace RozeComputing.Common.Services.Logging.Tests

    Public Class DatabaseTargetTests

        Private mLoggingService As New LoggingService()

        Private Const mTestLoggingName = "TestLoggingName"

        Private strbuilder As SqlConnectionStringBuilder

        Private Const DATABASE_NAME As String = "TestingDatabase"
        Private Const DATABASE_SCHEMA As String = "Testing"

        <SetUp>
        Public Sub Setup()
            mLoggingService = New LoggingService(mTestLoggingName)
            strbuilder = New SqlConnectionStringBuilder()
            strbuilder.DataSource = "MSI"
            strbuilder.IntegratedSecurity = True
            strbuilder.Pooling = True
            strbuilder.TrustServerCertificate = True
            strbuilder.Add("database", DATABASE_NAME)
        End Sub

        <Test>
        Public Sub AddDatabaseTarget()
            mLoggingService.AddDatabaseTarget("TestTargetName", LoggingService.DBProviderEnum.Microsoft_Data_SqlClient, strbuilder.ConnectionString)

            mLoggingService.Logger.Log(LogLevel.Info, "Add Database Target")
        End Sub
        <Test>
        Public Sub TestDatabaseTargetException()
            mLoggingService.AddDatabaseTarget("TestTargetName", LoggingService.DBProviderEnum.Microsoft_Data_SqlClient, strbuilder.ConnectionString)

            mLoggingService.Logger.Log(LogLevel.Fatal, New Exception("Test exception thrown", New Exception("This is a test inner exception")))
        End Sub

    End Class

End Namespace