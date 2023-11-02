Imports System.Data
Imports System.IO
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

    Private mSimpleCleaner As New SimpleCleaningService
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

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function

    Public Function UpdateTableData(pDatabaseName As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter)) As Long Implements IRozeDatabaseCompliance.UpdateTableData

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function

    Public Function SelectTableData(pDatabaseName As String, pTableName As String, pSqlStatement As String, pParameters As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.SelectTableData

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function

    Public Function DeleteTableData(pDatabaseName As String, pTableName As String, pWhereValues As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.DeleteTableData

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function

    Public Function InsertTableData(pDatabaseName As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.InsertTableData

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function

    Public Function ExecuteStoredProcedure(pDatabaseName As String, pProcedureName As String, pProcedureParameters As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.ExecuteStoredProcedure

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
    End Function


#End Region
End Class
