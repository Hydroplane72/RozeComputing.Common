Imports System.Data
Imports System.Diagnostics.Eventing
Imports System.IO
Imports System.Text
Imports Microsoft.Data
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
    Sub New(pConnectionSetting As SqlConnectionStringBuilder, pCleaningSettings As CleaningSettings)
        Try
            mSimpleCleaner = New SimpleCleaningService(pCleaningSettings)
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
    Sub New(pConnectionString As String, pCleaningSettings As CleaningSettings)
        Try
            mSimpleCleaner = New SimpleCleaningService(pCleaningSettings)
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
                query.Append("[" & whereValue.Name & "] = ")
                query.Append(" = ")

                If whereValue.ValueDataType = DatabaseParameter.ParameterDataType.IsString Then
                    query.Append("'" & whereValue.Value & "'")
                Else
                    query.Append(whereValue.Value)
                End If

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

    Public Function CreateOrAlterTableOnDatabase(pDatabaseName As String, pTableName As String, pColumns As List(Of DataColumn)) As Boolean Implements IRozeDatabaseCompliance.CreateOrAlterTableOnDatabase

        Dim result As Boolean = True

        Try
            OpenConnection()
            Dim query As New StringBuilder
            query.AppendLine("Use " & pDatabaseName)
            query.Append("CREATE TABLE ")
            query.Append(pTableName)
            query.AppendLine(" ( ")

            'Create the table structure
            For Each DataColumn In pColumns
                query.Append(DataColumn.ColumnName)
                query.Append(" "c)
                'Convert types to database type
                Select Case DataColumn.DataType.ToString()
                    Case "System.Int32"
                        query.Append(" int ")
                    Case "System.Int64"
                        query.Append(" bigint ")
                    Case "System.Int16"
                        query.Append(" smallint")
                    Case "System.Byte"
                        query.Append(" tinyint")
                    Case "System.Decimal"
                        query.Append(" decimal ")
                    Case "System.DateTime"
                        query.Append(" datetime ")
                    Case "System.String"
                        If DataColumn.MaxLength = -1 Then
                            query.Append(" nvarchar(MAX) ")
                        Else
                            query.Append(" nvarchar(")
                            query.Append(DataColumn.MaxLength)
                            query.Append(")"c)
                        End If
                    Case Else
                        If DataColumn.MaxLength = -1 Then
                            query.Append(" nvarchar(MAX) ")
                        Else
                            query.Append(" nvarchar(")
                            query.Append(DataColumn.MaxLength)
                            query.Append(")"c)
                        End If
                End Select
                query.Append(DataColumn.DataType)
                query.Append(", ")
            Next

            If pColumns.Count > 1 Then
                query.Length -= 2
            End If

            query.Append(")"c)

            Using sqlQuery As New SqlCommand(query.ToString(), mSqlConnection)
                sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
            result = False
        End Try
        Return result
    End Function

    Public Function UpdateTableData(pDatabaseName As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long Implements IRozeDatabaseCompliance.UpdateTableData
        Dim RowsUpdated As Long = 0

        Try
            OpenConnection()
            Dim query As New StringBuilder

            query.AppendLine("Use " & pDatabaseName)
            query.Append("Update " & pTableName & " (")

            'Set Columns to update
            For Each setValue In pSetDataValues
                query.Append("[" & setValue.Name & "] = ")

                If setValue.ValueDataType = DatabaseParameter.ParameterDataType.IsString Then
                    query.Append("'" & setValue.Value & "'")
                Else
                    query.Append(setValue.Value)
                End If

                query.Append(","c)
            Next

            'Get rid of the extra ,
            query.Length -= 1
            query.AppendLine()

            'Set where statements
            query.Append(GetWhereStatement(pWhereValues, pWhereAnd))

            query.Append(";"c)

            'Run the query
            Using sqlQuery As New SqlCommand(query.ToString(), mSqlConnection)
                RowsUpdated = sqlQuery.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try

        Return RowsUpdated
    End Function

    Public Function SelectTableData(pDatabaseName As String, pTableName As String, pSqlStatement As String) As DataTable Implements IRozeDatabaseCompliance.SelectTableData
        Dim selectedData As New DataTable
        Try
            OpenConnection()

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

    Public Function DeleteTableData(pDatabaseName As String, pTableName As String, pWhereValues As List(Of DatabaseParameter), pWhereAnd As Boolean) As Long Implements IRozeDatabaseCompliance.DeleteTableData
        Dim rowCount As Long = 0
        Try
            OpenConnection()

            Dim query As New StringBuilder
            query.Append("Delete From ")
            query.Append(pTableName)
            query.Append(" "c)

            'Set where statements
            query.Append(GetWhereStatement(pWhereValues, pWhereAnd))

            'Delete the data
            Using sqlQuery As New SqlCommand(query.ToString(), mSqlConnection)
                rowCount = sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Return rowCount
    End Function

    Public Function InsertTableData(pDatabaseName As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As Boolean Implements IRozeDatabaseCompliance.InsertTableData

        Dim rowsInserted As Boolean = True

        Try
            OpenConnection()
            Dim query As New StringBuilder
            Dim queryValues As New StringBuilder

            query.AppendLine("Use " & pDatabaseName)
            query.Append("Insert Into " & pTableName & " (")

            queryValues.Append("Values (")
            For Each pSetValue In pInsertValues
                query.Append("[" & pSetValue.Name & "]")
                query.Append(","c)

                If pSetValue.ValueDataType = DatabaseParameter.ParameterDataType.IsString Then
                    queryValues.Append("'" & pSetValue.Value & "'")
                Else
                    queryValues.Append(pSetValue.Value)
                End If

                query.Append(","c)
                queryValues.Append(","c)
            Next

            query.Length -= 1
            queryValues.Length -= 1

            query.Append(")"c)
            queryValues.Append(");")

            query.AppendLine(queryValues.ToString())

            Using sqlQuery As New SqlCommand(query.ToString(), mSqlConnection)
                rowsInserted = sqlQuery.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Exceptions.Add(ex)
            rowsInserted = False
        End Try

        Return rowsInserted
    End Function

#End Region
End Class
