Imports System.ComponentModel
Imports System.Data
Imports System.Diagnostics.CodeAnalysis
Imports System.IO
Imports System.Security.AccessControl
Imports System.Text
Imports Microsoft.Data.SqlClient
Imports RozeComputing.Common.Models
Imports RozeComputing.Common.Models.Cleaning
Imports RozeComputing.Common.Models.Database

''' <summary>
''' Deals with anything that modifies the database. This includes the following: <br />
''' <list type="bullet">
'''     <item>Attaching databases to the server</item>
'''     <item>Adding tables to a database</item>
'''     <item>Updating tables in a database</item>
'''     <item>Selecting from tables in a database</item>
'''     <item>Deleting from tables in a database</item>
'''     <item>Insert into tables in a database</item>     
'''     <item>Select from views in a database</item>     
'''     <item>Execute stored procedures in a database</item>     
''' </list>
''' </summary>
Public Class Database
    Implements IDisposable, IRozeDatabaseCompliance

#Region "Dispose"

    Private disposedValue As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' dispose managed state (managed objects)
            End If

            ' free unmanaged resources (unmanaged objects) and override finalizer
            ' set large fields to null
            If IsNothing(mSqlConnection) = False Then
                CloseConnection()
                mSqlConnection.Dispose()
                mSqlConnection = Nothing
            End If

            If IsNothing(Exceptions) = False Then
                Exceptions.Clear()
                Exceptions = Nothing
            End If
            If IsNothing(mConnectionSettings) = False Then
                mConnectionSettings = Nothing
            End If
            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

#Region "Variables"
    Private mSqlConnection As New Microsoft.Data.SqlClient.SqlConnection

    Private ReadOnly mSimpleCleaner As New SimpleCleaningService
    Private ReadOnly mDataRowService As New DataRowService
#End Region

#Region "Properties"

    Public Property Exceptions As New List(Of Exception) Implements IRozeCompliance.Exceptions

    Private mConnectionSettings As New SqlConnectionStringBuilder
    ''' <summary>
    ''' This is a Microsoft provided MSSQL Connection String Builder. <br />
    ''' View Documentation here for more information: 
    ''' 
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ConnectionSettings As SqlConnectionStringBuilder
        Get
            Return mConnectionSettings
        End Get
    End Property
#End Region

#Region "Constructor"
    ''' <summary>
    ''' Will assume the database server is local and allows for Windows Authentication. <br />
    ''' Any SQL statements will need to include the database name.
    ''' </summary>
    Sub New(pCleaningSettings As CleaningSettings)
        Try
            mSimpleCleaner = New SimpleCleaningService(pCleaningSettings)
            ConnectionSettings.TrustServerCertificate = True
            ConnectionSettings.DataSource = "."
            ConnectionSettings.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated

            SetSQLConnection()
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try

    End Sub

    ''' <summary>
    ''' Takes the settings and gets the connection string from them. Creating the SQL Connection
    ''' </summary>
    ''' <param name="pConnectionSetting"></param>
    Sub New(pConnectionSetting As SqlConnectionStringBuilder, Optional pCleaningSettings As CleaningSettings = Nothing, Optional pRowDataService As DataRowService = Nothing)
        Try
            If IsNothing(pCleaningSettings) Then
                pCleaningSettings = New CleaningSettings()
            End If
            mSimpleCleaner = New SimpleCleaningService(pCleaningSettings)

            If IsNothing(pRowDataService) Then
                pRowDataService = New DataRowService(pCleaningSettings)
            End If
            mDataRowService = pRowDataService

            mConnectionSettings = pConnectionSetting

            SetSQLConnection()
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Takes the Connection String and adds it to the <see cref="ConnectionSettings"/> property as <see cref="SqlConnectionStringBuilder.ConnectionString"/>
    ''' </summary>
    ''' <param name="pConnectionString">A valid connection string. Invalid connection strings will be add to <see cref="Exceptions"/></param>
    Sub New(pConnectionString As String, Optional pCleaningSettings As CleaningSettings = Nothing, Optional pRowDataService As DataRowService = Nothing)
        Try
            If IsNothing(pCleaningSettings) Then
                pCleaningSettings = New CleaningSettings()
            End If
            mSimpleCleaner = New SimpleCleaningService(pCleaningSettings)

            If IsNothing(pRowDataService) Then
                pRowDataService = New DataRowService(pCleaningSettings)
            End If
            mDataRowService = pRowDataService

            ConnectionSettings.ConnectionString = pConnectionString

            SetSQLConnection()
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub
#End Region

#Region "Private Methods"
    Private Sub SetSQLConnection()
        Try
            mSqlConnection.ConnectionString = ConnectionSettings.ConnectionString
            Dim a As String
            a = ""
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    Private Sub OpenConnection()
        Try
            If mSqlConnection.State = ConnectionState.Closed Then
                mSqlConnection.Open()
            End If
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    Private Sub CloseConnection()
        Try
            If mSqlConnection.State = ConnectionState.Open Then
                mSqlConnection.Close()
            End If
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    Private Function GetWhereStatement(pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As String

        Dim query As New StringBuilder
        Try
            query.Append(" WHERE ")
            'Set where statements
            For Each whereValue In pWhereValues
                query.Append(" "c)
                query.Append("[")
                query.Append(whereValue.ColumnName)
                query.Append("] = ")

                'Set for Parameters
                query.Append(" @" & whereValue.ColumnName.Replace(" ", "") & " ")

                'If whereValue.ValueDataType = DatabaseParameter.ParameterDataType.IsString Then
                '    query.Append("'" & whereValue.Value & "'")
                'Else
                '    query.Append(whereValue.Value)
                'End If

                If pWhereAnd = True Then
                    query.Append(" AND")
                Else
                    query.Append(" OR")
                End If
            Next

            'Get rid of extra And or OR
            query.Length -= 3
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Return query.ToString
    End Function

#End Region

#Region "Public Methods"

    ''' <summary>
    ''' Attach the database to the server if not already attached. Do NOT include the ldf file with the mdf file if the <paramref name="pDatabaseName"/> is set
    ''' </summary>
    ''' <param name="pDatabaseFile">The full path and name to the mdf file to use. <br />
    '''                             **Note:** Remote server, HTTP, And UNC path names are Not supported.
    ''' </param>
    ''' <param name="pDatabaseName">The name of the database to use as an alias</param>
    ''' <returns>Whether there were issues attaching the database and connecting or not. <br />True = good <br />False = bad (Check <see cref="Exceptions"/> for details)</returns>
    Public Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String, Optional pDatabaseName As String = "") As Boolean Implements IRozeDatabaseCompliance.AttachOrCreateDatabaseOnServer

        Dim result As Boolean = True

        Try
#Region "Parameter Cleaning"


            'Clean Arguments if have exceptions then throw bad argument exceptions
            pDatabaseFile = mSimpleCleaner.GetCleanString(pDatabaseFile, "", True, True, 0)
            If mSimpleCleaner.Exceptions.Count <> 0 OrElse File.Exists(pDatabaseFile) = False Then
                'Parameter is not a valid string or mdf does not exist
                Throw New ArgumentException("Parameter is not valid", NameOf(pDatabaseFile))
            ElseIf pDatabaseFile.EndsWith("mdf") = False Then
                Throw New ArgumentException("Parameter does not end with extension of 'mdf'", NameOf(pDatabaseFile))
            End If
            pDatabaseName = mSimpleCleaner.GetCleanString(pDatabaseName, "", True, True, 0)
            If mSimpleCleaner.Exceptions.Count <> 0 Then
                'Parameter is not a valid string
                Throw New ArgumentException("Parameter is not valid", NameOf(pDatabaseName))
            End If
#End Region

            'Make Sure to close the connection before modifying anything
            CloseConnection()

            'Set File Name
            mConnectionSettings.AttachDBFilename = pDatabaseFile

            'Set database name if exists
            If pDatabaseName.Length <> 0 Then
                mConnectionSettings.Add("database", pDatabaseName)
            End If

            'Set Connection
            SetSQLConnection()

            'Try opening the connection
            OpenConnection()

            If mSqlConnection.State = ConnectionState.Closed Then
                Throw New Exception("Unable to open a connection to the new database.")
            End If

        Catch ex As Exception
            Exceptions.Add(ex)
            result = False
        End Try

        Return result
    End Function

    Public Function DropTableOnDatabase(pDatabaseName As String, pTableSchema As String, pTableName As String) As Boolean Implements IRozeDatabaseCompliance.DropTableOnDatabase
        Dim result As Boolean = True

        Try
            Dim query As New StringBuilder
            OpenConnection()

            'Change the database name
            mSqlConnection.ChangeDatabase(pDatabaseName)

            Using sqlQuery As New SqlCommand()
                sqlQuery.Connection = mSqlConnection

                'Check if table exists before we try creating one. If already exists do nothing
                Dim dt As New DataTable
                sqlQuery.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" & pTableName & "'"

                Using adapter As New SqlDataAdapter(sqlQuery)
                    adapter.Fill(dt)
                End Using

                If dt.Rows.Count = 0 Then
                    'Table does not exist no need to do any more work
                    'Throw message into exceptions to let know that we did nothing
                    Throw New WarningException("Table Name: " & pTableSchema & "." & pTableName & " does not exist so can not drop it.")
                End If

                'Get the list of constraints
                query.Append("SELECT sys.schemas.name AS SchemaName, tableNamed.name AS ObjectName, ConstraintNamed.name AS ConstraintName, ConstraintNamed.type_desc
                                FROM            sys.objects AS tableNamed 
                                INNER JOIN sys.objects AS ConstraintNamed ON tableNamed.object_id = ConstraintNamed.parent_object_id 
                                INNER JOIN sys.schemas ON tableNamed.schema_id = sys.schemas.schema_id
                                WHERE (tableNamed.name = N'" & pTableName & "') AND (ConstraintNamed.type_desc LIKE N'%CONSTRAINT') AND (sys.Schemas.name = N'" & pTableSchema & "')")
                sqlQuery.CommandText = query.ToString
                dt = New DataTable
                Using adapter As New SqlDataAdapter(sqlQuery)
                    adapter.Fill(dt)
                End Using

                'See if we have any constraints
                If dt.Rows.Count > 0 Then
                    query.Clear()

                    For Each dtRow As DataRow In dt.Rows
                        'add each constrain to the query to be dropped
                        query.Append("Alter Table ")
                        query.Append(mDataRowService.GetStringFromDataRow(dtRow, "SchemaName", True, True))
                        query.Append("."c)
                        query.Append(mDataRowService.GetStringFromDataRow(dtRow, "ObjectName", True, True))
                        query.Append(" Drop Constraint ")
                        query.Append(mDataRowService.GetStringFromDataRow(dtRow, "ConstraintName", True, True))
                        query.AppendLine(";"c)
                    Next
                End If

                'Drop the Constraints
                sqlQuery.CommandText = query.ToString()
                sqlQuery.ExecuteNonQuery()

                'Drop the table
                sqlQuery.CommandText = "Drop Table " & pTableSchema & ".[" & pTableName & "]"
                sqlQuery.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
            result = False
        End Try

        Return result
    End Function

    Public Function CreateTableOnDatabase(pDatabaseName As String, pTableSchema As String, pTableName As String, pColumns As List(Of DataColumn)) As Boolean Implements IRozeDatabaseCompliance.CreateTableOnDatabase

        Dim result As Boolean = True

        Try
            OpenConnection()

            Dim createTableQuery As New StringBuilder
            Dim constraintsQuery As New StringBuilder
            createTableQuery.AppendLine("Use " & pDatabaseName)
            createTableQuery.Append(" CREATE TABLE [")
            createTableQuery.Append(pTableName)
            createTableQuery.AppendLine("] ( ")

            'Create the table structure
            For Each DataColumn In pColumns
                createTableQuery.Append(" [")
                createTableQuery.Append(DataColumn.ColumnName)
                createTableQuery.Append("] ")
                'Convert types to database type
                Select Case DataColumn.DataType.ToString()
                    Case "System.Int32"
                        createTableQuery.Append(" int ")
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanInteger(DataColumn.DefaultValue) <> Integer.MinValue Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (" & mSimpleCleaner.GetCleanInteger(DataColumn.DefaultValue) & ") FOR [" & DataColumn.ColumnName & "]")
                        End If
                    Case "System.Int64"
                        createTableQuery.Append(" bigint ")
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanInt64(DataColumn.DefaultValue) <> Long.MinValue Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (" & mSimpleCleaner.GetCleanInt64(DataColumn.DefaultValue) & ") FOR [" & DataColumn.ColumnName & "]")
                        End If
                    Case "System.Int16"
                        createTableQuery.Append(" smallint")
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanInt16(DataColumn.DefaultValue) <> Short.MinValue Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (" & mSimpleCleaner.GetCleanInt16(DataColumn.DefaultValue) & ") FOR [" & DataColumn.ColumnName & "]")
                        End If
                    'Commented Out because we don't ahve a GetClean Created for this yet
                    'Case "System.Byte"
                    '    createTableQuery.Append(" tinyint")
                    '    'Create default Constraints when needed
                    '    If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanInteger(DataColumn.DefaultValue) <> Integer.MinValue Then
                    '        constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_"& DataColumn.ColumnName & "] DEFAULT (" & DataColumn.DefaultValue & ") FOR [" & DataColumn.ColumnName & "]")
                    '    End If
                    Case "System.Decimal"
                        createTableQuery.Append(" decimal ")
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanDouble(DataColumn.DefaultValue) <> Double.MinValue Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (" & mSimpleCleaner.GetCleanDouble(DataColumn.DefaultValue) & ") FOR [" & DataColumn.ColumnName & "]")
                        End If
                    Case "System.DateTime"
                        createTableQuery.Append(" datetime ")
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanDateTime(DataColumn.DefaultValue) <> DateTime.MinValue Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (" & mSimpleCleaner.GetCleanDateTime(DataColumn.DefaultValue) & ") FOR [" & DataColumn.ColumnName & "]")
                        End If
                    Case "System.Guid"
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanGUID(DataColumn.DefaultValue) <> Guid.Empty Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT (newid()) FOR [" & DataColumn.ColumnName & "]")
                        End If
                        createTableQuery.Append(" uniqueidentifier ROWGUIDCOL")
                    Case "System.String"
                        'Create default Constraints when needed
                        If IsNothing(DataColumn.DefaultValue) = False AndAlso mSimpleCleaner.GetCleanString(DataColumn.DefaultValue) <> String.Empty Then
                            constraintsQuery.AppendLine("ALTER TABLE [" & pTableSchema & "].[" & pTableName & "] ADD CONSTRAINT [DF_" & pTableName.Replace(" ", "") & "_" & DataColumn.ColumnName & "] DEFAULT ('" & mSimpleCleaner.GetCleanString(DataColumn.DefaultValue) & "') FOR [" & DataColumn.ColumnName & "]")
                        End If
                        If DataColumn.MaxLength = -1 Then
                            createTableQuery.Append(" nvarchar(MAX) ")
                        Else
                            createTableQuery.Append(" nvarchar(")
                            createTableQuery.Append(DataColumn.MaxLength)
                            createTableQuery.Append(")"c)
                        End If
                    Case Else
                        If DataColumn.MaxLength = -1 Then
                            createTableQuery.Append(" nvarchar(MAX) ")
                        Else
                            createTableQuery.Append(" nvarchar(")
                            createTableQuery.Append(DataColumn.MaxLength)
                            createTableQuery.Append(")"c)
                        End If
                End Select

                If DataColumn.AllowDBNull = False Then
                    createTableQuery.Append(" Not Null")
                End If
                'query.Append(DataColumn.DataType)
                createTableQuery.Append(", ")


            Next

            If pColumns.Count > 1 Then
                createTableQuery.Length -= 2
            End If

            createTableQuery.Append(")"c)

            'Change the databasename
            mSqlConnection.ChangeDatabase(pDatabaseName)


            Using sqlQuery As New SqlCommand()
                sqlQuery.Connection = mSqlConnection

                'Check if table exists before we try creating one. If already exists do nothing
                Dim dt As New DataTable
                sqlQuery.CommandText = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" & pTableSchema & "' AND  TABLE_NAME = '" & pTableName & "'"

                Using adapter As New SqlDataAdapter(sqlQuery)
                    adapter.Fill(dt)
                End Using

                If dt.Rows.Count <> 0 Then
                    'Table already exists do nothing
                    Throw New WarningException("Table already exists")
                End If

                'Create the table
                sqlQuery.CommandText = createTableQuery.ToString()
                sqlQuery.ExecuteNonQuery()

                'Create the constraints
                sqlQuery.CommandText = constraintsQuery.ToString()
                sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
            result = False
        End Try
        Return result
    End Function

    Public Function UpdateTableData(pDatabaseName As String, pTableSchema As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long Implements IRozeDatabaseCompliance.UpdateTableData
        Dim RowsUpdated As Long = 0

        Try
            OpenConnection()
            mSqlConnection.ChangeDatabase(pDatabaseName)

            'Run the query
            Using sqlQuery As New SqlCommand()
                sqlQuery.Connection = mSqlConnection

                Dim query As New StringBuilder


                query.AppendLine("Update [" & pTableSchema & "][" & pTableName & "] ")
                query.Append("Set ")
                'Set Columns to update
                For Each setValue In pSetDataValues
                    query.Append("["c)
                    query.Append(setValue.ColumnName)
                    query.Append("] = ")

                    query.Append(" @Sets")
                    query.Append(setValue.ColumnName.Replace(" ", ""))
                    query.Append(" ,")

                    sqlQuery.Parameters.AddWithValue("@Sets" & setValue.ColumnName.Replace(" ", ""), setValue.Value)
                Next

                'Get rid of the extra ,
                query.Length -= 1
                query.AppendLine()

                'Set where statements
                query.Append(GetWhereStatement(pWhereValues, pWhereAnd))

                query.Append(";"c)

                'Get Where statement parameters
                For Each whereParam In pWhereValues
                    sqlQuery.Parameters.AddWithValue("@" & whereParam.ColumnName.Replace(" ", ""), whereParam.Value)
                Next

                sqlQuery.CommandText = query.ToString()
                RowsUpdated = sqlQuery.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try

        Return RowsUpdated
    End Function

    Public Function SelectTableData(pDatabaseName As String, pSqlStatement As String) As DataTable Implements IRozeDatabaseCompliance.SelectTableData
        Dim selectedData As New DataTable
        Try
            OpenConnection()
            mSqlConnection.ChangeDatabase(pDatabaseName)

            Using sqlCmd As New SqlCommand(pSqlStatement, mSqlConnection)
                Using adapter As New SqlDataAdapter(sqlCmd)
                    adapter.Fill(selectedData)
                End Using
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try

        Return selectedData
    End Function

    Public Function DeleteTableData(pDatabaseName As String, pTableSchema As String, pTableName As String, pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long Implements IRozeDatabaseCompliance.DeleteTableData
        Dim rowCount As Long = 0
        Try
            OpenConnection()
            mSqlConnection.ChangeDatabase(pDatabaseName)

            Dim query As New StringBuilder
            query.Append("Delete From [")
            query.Append(pTableSchema)
            query.Append("].[")
            query.Append(pTableName)
            query.Append("] ")
            If pWhereValues.Count > 0 Then
                'Set where statements
                query.Append(GetWhereStatement(pWhereValues, pWhereAnd))
            End If

            'Delete the data
            Using sqlQuery As New SqlCommand(query.ToString(), mSqlConnection)

                If pWhereValues.Count > 0 Then
                    'Add Parameters with values
                    For Each param In pWhereValues
                        sqlQuery.Parameters.AddWithValue(param.ColumnName.Replace(" ", ""), param.Value)
                    Next
                End If

                rowCount = sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Return rowCount
    End Function

    Public Function InsertTableData(pDatabaseName As String, pTableSchema As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As Boolean Implements IRozeDatabaseCompliance.InsertTableData

        Dim rowsInserted As Integer = 0

        Try
            OpenConnection()
            Using sqlQuery As New SqlCommand()
                sqlQuery.Connection = mSqlConnection

                Dim query As New StringBuilder
                Dim queryValues As New StringBuilder

                query.AppendLine("Use " & pDatabaseName)
                query.Append("Insert Into [" & pTableSchema & "].[" & pTableName & "] (")

                queryValues.Append(" Values (")
                For Each pSetValue In pInsertValues

                    'Replace column names to not have a space
                    If IsNothing(pSetValue) Then
                        sqlQuery.Parameters.AddWithValue("@" & pSetValue.ColumnName.Replace(" ", ""), DBNull.Value)
                    Else
                        sqlQuery.Parameters.AddWithValue("@" & pSetValue.ColumnName.Replace(" ", ""), pSetValue.Value)
                    End If

                    query.Append("[" & pSetValue.ColumnName & "]")
                    queryValues.Append(" @" & pSetValue.ColumnName.Replace(" ", ""))

                    'If pSetValue.ValueDataType = DatabaseParameter.ParameterDataType.IsString Then
                    '    queryValues.Append("'" & pSetValue.Value & "'")
                    'Else
                    '    queryValues.Append(pSetValue.Value)
                    'End If

                    query.Append(","c)
                    queryValues.Append(","c)
                Next

                query.Length -= 1
                queryValues.Length -= 1

                query.Append(")"c)
                queryValues.Append(");")

                query.AppendLine(queryValues.ToString())


                sqlQuery.CommandText = query.ToString
                rowsInserted = sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
            rowsInserted = False
        End Try

        Return rowsInserted
    End Function

    Public Function UpdateInsertTableData(pDatabaseName As String, pTableSchema As String, pDataTable As DataTable) As Integer Implements IRozeDatabaseCompliance.UpdateInsertTableData
        Dim rowsChanged As Integer = 0

        Try

            Dim sqlStatement As String = "Select * from [" & pTableSchema & "].[" & pDataTable.TableName & "] WHERE 1=0"

            mSqlConnection.ChangeDatabase(pDatabaseName)




            Using adapter As New SqlDataAdapter(sqlStatement, mSqlConnection)


                Using commandBuilder As New SqlCommandBuilder(adapter) With {
                    .SetAllValues = False
                }
                    adapter.DeleteCommand = commandBuilder.GetDeleteCommand(True)
                    adapter.UpdateCommand = commandBuilder.GetUpdateCommand(True)
                    adapter.InsertCommand = commandBuilder.GetInsertCommand(True)

                    'Start a transaction so we can roll back if need be
                    Using sqlTransaction As SqlTransaction = mSqlConnection.BeginTransaction("Transaction_" & pDataTable.TableName)
                        Try

                            adapter.DeleteCommand.Transaction = sqlTransaction
                            adapter.UpdateCommand.Transaction = sqlTransaction
                            adapter.InsertCommand.Transaction = sqlTransaction


                            'Update the Data table in the database
                            rowsChanged = adapter.Update(pDataTable)

                            'try committing the changes
                            sqlTransaction.Commit()
                        Catch ex As Exception
                            Exceptions.Add(ex)
                            rowsChanged = -1
                            'Try rolling back the transaction since we hit an error
                            Try
                                sqlTransaction.Rollback()
                            Catch rollBackException As Exception
                                Exceptions.Add(rollBackException)
                            End Try

                        End Try
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
            rowsChanged = -1
        End Try

        Return rowsChanged
    End Function

    ''' <summary>
    ''' MSSQL DATABASE SPECIFIC<br />
    ''' Updates the table using the passed in commands as guidelines
    ''' </summary>
    ''' <param name="pDatabaseName">The database to run against</param>
    ''' <param name="pDataTable">Contains rows that you have updated, Added, or marked as deleted</param>
    ''' <param name="pSelectCommand">Use to get the structure that matches the <paramref name="pDataTable"/></param>
    ''' <param name="pDeleteCommand">Use to set the sql statement to delete data by.</param>
    ''' <param name="pUpdateCommand">Use to set the sql statement to update data by</param>
    ''' <param name="pInsertCommand">Use to set the sql statement to insert data by</param>
    ''' <param name="pParameterCollection">The parameters that we will use to substitute within the where statements in the commands.</param>
    ''' <returns>Number of rows Updated, Deleted, and added to the datatable.</returns>
    Public Function UpdateInsertTableData(pDatabaseName As String, pDataTable As DataTable, pSelectCommand As String, pDeleteCommand As String, pUpdateCommand As String, pInsertCommand As String, pParameterCollection As List(Of SqlParameter)) As Integer
        Dim rowsChanged As Integer = 0

        Try
            mSqlConnection.ChangeDatabase(pDatabaseName)

            'Start a transaction so we can roll back if need be
            Using sqlTransaction As SqlTransaction = mSqlConnection.BeginTransaction("Transaction_" & mSimpleCleaner.GetCleanString(pDataTable.TableName, True, True, 20))

                'Create the select cmd so we know the data table we are working with
                Using sqlSelectCmd As New SqlCommand(pSelectCommand, mSqlConnection, sqlTransaction)
                    sqlSelectCmd.Parameters.AddRange(pParameterCollection.ToArray)

                    'Create the adapter for working with the database
                    Using adapter As New SqlDataAdapter(sqlSelectCmd)

                        'Set each cmd type so we can then send the data table through without fear
                        adapter.DeleteCommand = New SqlCommand(pDeleteCommand, mSqlConnection, sqlTransaction)
                        adapter.DeleteCommand.Parameters.Clear()
                        adapter.DeleteCommand.Parameters.AddRange(pParameterCollection.ToArray)

                        adapter.UpdateCommand = New SqlCommand(pUpdateCommand, mSqlConnection, sqlTransaction)
                        adapter.UpdateCommand.Parameters.Clear()
                        adapter.UpdateCommand.Parameters.AddRange(pParameterCollection.ToArray)

                        adapter.InsertCommand = New SqlCommand(pInsertCommand, mSqlConnection, sqlTransaction)
                        adapter.InsertCommand.Parameters.Clear()
                        adapter.InsertCommand.Parameters.AddRange(pParameterCollection.ToArray)


                        Try
                            'Update the Data table in the database
                            rowsChanged = adapter.Update(pDataTable)

                            'try committing the changes
                            sqlTransaction.Commit()
                        Catch ex As Exception
                            Exceptions.Add(ex)
                            rowsChanged = -1
                            'Try rolling back the transaction since we hit an error
                            Try
                                sqlTransaction.Rollback()
                            Catch rollBackException As Exception
                                Exceptions.Add(rollBackException)
                            End Try

                        End Try
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
            rowsChanged = -1
        End Try

        Return rowsChanged
    End Function
#End Region
End Class
