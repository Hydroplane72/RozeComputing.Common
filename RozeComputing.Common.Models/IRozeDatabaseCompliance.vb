Imports System.Data

Namespace Database
    ''' <summary>
    ''' All Database Type Services use this to provide a minimum amount of functionality. <br />
    ''' Each service will have specific functionality depending on the database type
    ''' </summary>
    Public Interface IRozeDatabaseCompliance
        Inherits IRozeCompliance
#Region "Properties"

#End Region

#Region "Methods"
        ''' <summary>
        ''' Attach the Database to the server if the database does not already exist
        ''' </summary>
        ''' <param name="pDatabaseFile">The full path and name of the database file</param>
        ''' <param name="pDatabaseName">The name of the database</param>
        ''' <returns>If the operation was successful</returns>
        Function AttachOrCreateDatabaseOnServer(pDatabaseFile As String, Optional pDatabaseName As String = "") As Boolean

        Function CreateOrAlterTableOnDatabase(pDatabaseName As String, pTableName As String, pColumns As List(Of DataColumn)) As Boolean

        Function UpdateTableData(pDatabaseName As String, pTableName As String, pSetDataValues As List(Of DatabaseParameter), pWhereValues As List(Of DatabaseParameter)) As Long

        Function SelectTableData(pDatabaseName As String, pTableName As String, pSqlStatement As String, pWhereParameters As List(Of DatabaseParameter)) As DataTable

        Function DeleteTableData(pDatabaseName As String, pTableName As String, pWhereValues As List(Of DatabaseParameter)) As DataTable

        Function InsertTableData(pDatabaseName As String, pTableName As String, pInsertValues As List(Of DatabaseParameter)) As DataTable

        Function ExecuteStoredProcedure(pDatabaseName As String, pProcedureName As String, pProcedureParameters As List(Of DatabaseParameter)) As DataTable

#End Region
    End Interface
End Namespace