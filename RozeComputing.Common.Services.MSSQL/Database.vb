Imports System.Data
Imports Microsoft.Data.SqlClient
Imports RozeComputing.Common.Models
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
                ' TODO: dispose managed state (managed objects)
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

#Region "Variables"
    Private ReadOnly mSqlConnection As New Microsoft.Data.SqlClient.SqlConnection
#End Region
#Region "Properties"

    Public Property Exceptions As New List(Of Exception) Implements IRozeCompliance.Exceptions


    ''' <summary>
    ''' This is a Microsoft provided MSSQL Connection String Builder. <br />
    ''' View Documentation here for more information: 
    ''' 
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ConnectionSettings As New SqlConnectionStringBuilder
#End Region

#Region "Constructor"
    ''' <summary>
    ''' Will assume the database server is local and allows for Windows Authentication. <br />
    ''' Any SQL statements will need to include the database name.
    ''' </summary>
    Sub New()
        Try
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
    Sub New(pConnectionSetting As SqlConnectionStringBuilder)
        Try
            ConnectionSettings = pConnectionSetting

            SetSQLConnection()
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Takes the Connection String and adds it to the <see cref="ConnectionSettings"/> property as <see cref="SqlConnectionStringBuilder.ConnectionString"/>
    ''' </summary>
    ''' <param name="pConnectionString">A valid connection string. Invalid connection strings will be add to <see cref="Exceptions"/></param>
    Sub New(pConnectionString As String)
        Try
            ConnectionSettings.ConnectionString = pConnectionString

            SetSQLConnection()
        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
    End Sub
#End Region

#Region "Methods"

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

    Public Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String, pDatabaseName As String) As Boolean Implements IRozeDatabaseCompliance.AttachOrCreateDatabaseOnServer

        Try
            OpenConnection()

        Catch ex As Exception
            Exceptions.Add(ex)
        End Try
        Throw New NotImplementedException()
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
