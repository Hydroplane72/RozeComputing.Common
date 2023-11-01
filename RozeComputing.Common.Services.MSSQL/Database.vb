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

#Region "Properties"

    Public Property Exceptions As New List(Of Exception) Implements IRozeCompliance.Exceptions

#End Region

#Region "Constructor"
    Sub New()

    End Sub


#End Region

#Region "Methods"


    Public Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String, pDatabaseName As String) As Boolean Implements IRozeDatabaseCompliance.AttachOrCreateDatabaseOnServer
        Throw New NotImplementedException()
    End Function

    Public Function CreateOrAlterTableOnDatabase(pDatabaseName As String, pTableName As String) As Boolean Implements IRozeDatabaseCompliance.CreateOrAlterTableOnDatabase
        Throw New NotImplementedException()
    End Function

    Public Function UpdateTableData(pDatabaseName As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter)) As Long Implements IRozeDatabaseCompliance.UpdateTableData
        Throw New NotImplementedException()
    End Function

    Public Function SelectTableData(pDatabaseName As String, pTableName As String, pSqlStatement As String, pParameters As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.SelectTableData
        Throw New NotImplementedException()
    End Function

    Public Function DeleteTableData(pDatabaseName As String, pTableName As String, pWhereValues As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.DeleteTableData
        Throw New NotImplementedException()
    End Function

    Public Function InsertTableData(pDatabaseName As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.InsertTableData
        Throw New NotImplementedException()
    End Function

    Public Function ExecuteStoredProcedure(pDatabaseName As String, pProcedureName As String, pProcedureParameters As List(Of DatabaseParameter)) As DataTable Implements IRozeDatabaseCompliance.ExecuteStoredProcedure
        Throw New NotImplementedException()
    End Function


#End Region
End Class
