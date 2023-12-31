Imports System.Data
Imports System.Text
Imports Microsoft.Data.SqlClient
Imports NUnit.Framework
Imports RozeComputing.Common.Models.Database

Namespace RozeComputing.Common.Services.Mssql.Tests

    Public Class Tests
        Private strbuilder As SqlConnectionStringBuilder

        Private Const DATABASE_NAME As String = "TestingDatabase"
        Private Const DATABASE_SCHEMA As String = "Testing"
        <SetUp>
        Public Sub Setup()
            strbuilder = New SqlConnectionStringBuilder()
            strbuilder.DataSource = "MSI"
            strbuilder.IntegratedSecurity = True
            strbuilder.Pooling = True
            strbuilder.TrustServerCertificate = True
            strbuilder.Add("database", DATABASE_NAME)
        End Sub



        <Test>
        Public Sub DropTable()

            DropTable("TestDropTable")

        End Sub

        Public Sub DropTable(pTableName As String)

            Dim response As Boolean = False
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Make sure table exists
                Dim dt As DataTable = database.SelectTableData("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" & pTableName & "'")
                If dt.Rows.Count = 0 Then
                    CreateTable(pTableName)
                End If

                'Send to database to save new row.
                response = database.DropTableOnDatabase(pTableName)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while deleting data.", database.Exceptions))
                End If
            End Using

            If response = False Then
                Assert.Fail(GetAssertionExceptionMessage("Failed to drop table for some reason with no exceptions thrown."))
            End If
        End Sub

        <Test>
        Public Sub CreateTable()

            CreateTable("TestCreateTable")
        End Sub

        Public Sub CreateTable(pTableName As String)

            Dim response As Boolean = False
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'If table exists drop it
                Dim dt As DataTable = database.SelectTableData("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" & pTableName & "'")
                If dt.Rows.Count <> 0 Then
                    DropTable(pTableName)
                End If

                Dim columns As New List(Of DataColumn)
                columns.Add(New DataColumn() With {.AllowDBNull = False, .Unique = True, .ColumnName = "id", .DataType = Type.GetType("System.Guid"), .DefaultValue = Guid.NewGuid()})
                columns.Add(New DataColumn("FirstName", System.Type.GetType("System.String")))
                columns.Add(New DataColumn("LastName", System.Type.GetType("System.String")))
                columns.Add(New DataColumn("age", System.Type.GetType("System.Int32")))



                'Send to database to save new row.
                response = database.CreateTableOnDatabase(pTableName, columns)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while creating table.", database.Exceptions))
                End If
            End Using

            If response = False Then
                Assert.Fail(GetAssertionExceptionMessage("Failed to create table for some reason: "))
            End If
        End Sub
        <Test>
        Public Sub UpdateTableData_1Row()

            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Delete Data to make sure we don't have conflicts with past tests
                DeleteTableData_AllRows()

                'Make sure there is data in table
                InsertTableData("UpdateThisPlease")
                InsertTableData("UpdateThis")
                InsertTableData("PleaseUpdateThis")
                InsertTableData("So Close")
                InsertTableData("")

                Dim setValues As New List(Of DatabaseParameter)


                setValues.Add(New DatabaseParameter("name", "To this"))
                setValues.Add(New DatabaseParameter("dateTimeTest", DateTime.Now))

                Dim whereValues As New List(Of DatabaseParameter)

                Dim whereValue As New DatabaseParameter("name", "UpdateThis")
                whereValues.Add(whereValue)

                'Send to database to save new row.
                response = database.UpdateTableData("TestInsertStatements", setValues, whereValues, False)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while deleting data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            ElseIf response <> 1 Then
                Assert.Fail(GetAssertionExceptionMessage("Too much data updated"))
            End If
        End Sub
        <Test>
        Public Sub DeleteTableData_1Row()

            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Make sure there is data in table
                InsertTableData()
                InsertTableData("DeleteThis")

                Dim insertValues As New List(Of DatabaseParameter)

                Dim insertValue As New DatabaseParameter("name", "DeleteThis")
                insertValues.Add(insertValue)

                'Send to database to save new row.
                response = database.DeleteTableData("TestInsertStatements", insertValues, False)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while deleting data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub

        <Test>
        Public Sub DeleteTableData_AllRows()

            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Make sure there is data in table
                InsertTableData()

                Dim insertValues As New List(Of DatabaseParameter)

                'Send to database to save new row.
                response = database.DeleteTableData("TestInsertStatements", insertValues, False)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while deleting data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub

        <Test>
        Public Sub InsertTableData()
            InsertTableData("Test Insert")
        End Sub

        Public Sub InsertTableData(pNameValue As String)

            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                Dim insertValues As New List(Of DatabaseParameter)

                Dim insertValue As New DatabaseParameter("name", pNameValue)
                insertValues.Add(insertValue)

                insertValue = New DatabaseParameter("boolTest", False)
                insertValues.Add(insertValue)

                insertValue = New DatabaseParameter("dateTimeTest", DateTime.Now)
                insertValues.Add(insertValue)

                insertValue = New DatabaseParameter("numberTest", 12313.4565)
                insertValues.Add(insertValue)



                'Send to database to save new row.
                response = database.InsertTableData("TestInsertStatements", insertValues)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while inserting data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub

        <Test>
        Public Sub Update_UpdateInsertTableData()
            Dim dt As New DataTable
            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Get the table data
                dt = database.SelectTableData("Select * From [TestInsertStatements] where 1=1")

                'Set the name of the table to update
                dt.TableName = "TestInsertStatements"

                'Update a random row, make sure we have data or else call the Insert into table before continuing
                If dt.Rows.Count = 0 Then
                    InsertTableData()

                    'Data should now exist
                    dt = database.SelectTableData("Select * From [TestInsertStatements] where 1=1")
                End If

                'Rows exist ... Change the first one
                dt.Rows.Item(0).Item("name") = "Updated " & DateTime.Now
                dt.Rows.Item(0).Item("dateTimeTest") = DateTime.Now

                'Send to database to save new row.
                response = database.UpdateInsertTableData(dt)


                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while updating data.", database.Exceptions))
                End If
            End Using

            If response = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub
        <Test>
        Public Sub Insert_UpdateInsertTableData()
            Dim dt As New DataTable
            Dim response As Integer = 0
            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME

                'Get the table structure
                dt = database.SelectTableData("Select * From [TestInsertStatements] where 1=0")

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
                response = database.UpdateInsertTableData(dt)


                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while Inserting data.", database.Exceptions))
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

                database.DatabaseName = DATABASE_NAME

                dt = database.SelectTableData("Select * From [TestSelectStatements]")
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while querying for data.", database.Exceptions))
                End If
            End Using

            If dt.Rows.Count = 0 Then
                Assert.Fail(GetAssertionExceptionMessage("No data returned from select."))
            End If
        End Sub
        <Test>
        Public Sub SelectTableDataUsingSQLParameters()
            Dim dt As New DataTable

            Using database As New Database(strbuilder)
                If database.Exceptions.Count > 0 Then
                    Assert.Fail(GetAssertionExceptionMessage("Exceptions hit while constructing.", database.Exceptions))
                End If

                database.DatabaseName = DATABASE_NAME
                Dim parameters As New List(Of SqlParameter) From {
                    database.GetNewSQLParameter("@ParameterName", 1),
                    database.GetNewSQLParameter("@StringParameterName", "1")
                }

                dt = database.SelectTableData("Select * From [TestSelectStatements] Where 1 = @ParameterName AND '1' = @StringParameterName", parameters)
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