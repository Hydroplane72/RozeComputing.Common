Imports System.Data
Imports System.Text
Imports Microsoft.Data.SqlClient
Imports NUnit.Framework

Namespace RozeComputing.Common.Services.Mssql.Tests

    Public Class Tests
        Private strbuilder As SqlConnectionStringBuilder
        <SetUp>
        Public Sub Setup()
            strbuilder = New SqlConnectionStringBuilder()
            strbuilder.DataSource = "MSI"
            strbuilder.IntegratedSecurity = True
            strbuilder.Pooling = True
            strbuilder.TrustServerCertificate = True
        End Sub

        <Test>
        Public Sub SimpleTest()
            Assert.Pass()
        End Sub

        <Test>
        Public Sub InsertTableData()
            Dim dt As New DataTable
            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If
                'Get the table structure
                dt = database.SelectTableData("TestingDatabase", "Select * From [TestInsertStatements] where 1=0")

                'Set the name of the table to update
                dt.TableName = "TestInsertStatements"

                'Create a row
                Dim dr As DataRow = dt.NewRow

                'Intentionally don't set ID to test ID auto creation
                dr.Item("name") = "Test"
                dr.Item("boolTest") = True
                dr.Item("dateTimeTest") = DateTime.Now
                dr.Item("numberTest") = 1213213.254

                'Add row to table
                dt.Rows.Add(dr)

                'Send to database to save new row.
                response = database.UpdateInsertTableData("TestingDatabase", dt)


                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while querying for data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub

        <Test>
        Public Sub SelectTableData()
            Dim dt As New DataTable

            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If
                dt = database.SelectTableData("TestingDatabase", "Select * From [TestSelectStatements]")
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while querying for data.", database.Exceptions))
                End If
            End Using

            If dt.Rows.Count = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub

        Private Function GetAssertionExceptionMessage(pBeginningMessage As String, Optional pExceptions As List(Of Exception) = Nothing)
            Dim strBuilder As New StringBuilder

            strBuilder.AppendLine(pBeginningMessage)

            Dim exceptionID As Integer = 1
            If IsNothing(pExceptions) = False Then
                For Each ex In pExceptions
                    strBuilder.Append("Exception " & exceptionID & " Message: ")
                    strBuilder.Append(ex.Message)
                    strBuilder.Append(" Stack Trace: ")
                    strBuilder.AppendLine(ex.StackTrace)
                    strBuilder.AppendLine()
                Next
            End If
            Return strBuilder.ToString
        End Function
    End Class

End Namespace